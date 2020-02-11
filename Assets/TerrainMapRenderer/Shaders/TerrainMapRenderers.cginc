float2 kernel3x3[9] = {
	float2(-1, -1),
	float2(0, -1),
	float2(1, -1),
	float2(-1, 0),
	float2(0, 0),
	float2(1, 0),
	float2(-1, 1),
	float2(0, 1),
	float2(1, 1)
};

float HeightFromDepth(sampler2D depthTex, float4 screenPos) {
	float z = tex2Dproj(depthTex, UNITY_PROJ_COORD(screenPos)).r;
	float height = _ProjectionParams.y + z * (_ProjectionParams.y - _ProjectionParams.z);

	return height;
}

float4 NormalFromHeight(sampler2D heightmap, float2 uv) {

	float strenght = 32;

	float radius = 1 * 0.001;

	//Remove edge
	if (uv.x < radius + 0.001 || uv.x > 1 - radius || uv.y < radius + 0.001 || uv.y > 1 - radius - 0.001) strenght = 0;

	float xLeft = tex2D(heightmap, uv - float2(radius, 0.0)) * strenght;
	float xRight = tex2D(heightmap, uv + float2(radius, 0.0)) * strenght;

	float yUp = tex2D(heightmap, uv - float2(0.0, radius)) * strenght;
	float yDown = tex2D(heightmap, uv + float2(0.0, radius)) * strenght;

	float xDelta = ((xLeft - xRight) + 1.0) * 0.5f;
	float yDelta = ((yUp - yDown) + 1.0) * 0.5f;

	/*
#if UNITY_VERSION >= 201810
#if !UNITY_COLORSPACE_GAMMA
	xLeft = GammaToLinearSpace(xLeft);
	xRight = GammaToLinearSpace(xRight);
	yUp = GammaToLinearSpace(yUp);
	yDown = GammaToLinearSpace(yDown);
	xDelta = GammaToLinearSpace(xDelta);
	yDelta = GammaToLinearSpace(yDelta);
#endif
#endif
*/
	float4 normals = float4(xDelta, yDelta, 1.0, yDelta);

	return normals;
}

float Sign(float v)
{
	if (v > 0) return 1;
	if (v < 0) return -1;
	return 0;
}

//https://pro.arcgis.com/en/pro-app/tool-reference/3d-analyst/how-aspect-works.htm
float4 Aspect(sampler2D normalmap, float2 uv) {

	float3 normals = (tex2D(normalmap, uv.xy)).xyz;
	//normals = normals * 2 - 1;

	//return float4(normals.xyz, 0);


	float3 lightDir = float3(-0.25, 0.25, 0.5);

	float NdotL = dot(lightDir, normals.xyz);

	return float4(NdotL, NdotL, NdotL, 1);
}

//Raymarching?
float Shadows(sampler2D heightmap, float2 uv) {

	float height = tex2D(heightmap, uv).r;

	//North-west
	float3 lightDir = float3(-0.5, 0.5, 0.8);

	return 1;
}

//https://pro.arcgis.com/en/pro-app/tool-reference/3d-analyst/how-hillshade-works.htm
float4 Hillshade(sampler2D normals, float2 uv) {
	half3 norm = UnpackNormal(tex2D(normals, uv));
	norm = norm * 0.5 + 0.5;

	//North-west
	float3 lightDir = float3(-0.5, 0.5, 0.8);

	float NdotL = dot(norm.xyz, lightDir);

	float slope = norm.x * norm.y;

	float Zenith_rad = 45 * (UNITY_PI * 2 / 360);
	float Slope_rad = 15 * (UNITY_PI * 2 / 360);
	float Aspect_rad = 45 * (UNITY_PI * 2 / 360);

	//North-west
	float Azimuth_rad = 315 * (UNITY_PI * 2 / 360);

	NdotL = ((cos(Zenith_rad) * cos(slope)) +
		(sin(Zenith_rad) * sin(Slope_rad) * cos(Azimuth_rad - NdotL)));

	//NdotL = slope;
	return float4(NdotL, NdotL, NdotL, 1);
}
float rand(float n) { return frac(sin(n) * 13758.5453123 * 0.01); }

float3 kernel[9] = {
	float3(0,0,0),
	float3(0,0,0),
	float3(0,0,0),
	float3(0,0,0),
	float3(0,0,0),
	float3(0,0,0),
	float3(0,0,0),
	float3(0,0,0),
	float3(0,0,0)
};

float3 noiseVectors[4] = {
	float3(0,0,0),
	float3(0,0,0),
	float3(0,0,0),
	float3(0,0,0)
};
float4 AOFromNormal(sampler2D noiseTex, sampler2D heightmap, sampler2D normals, float2 uv) {

	UNITY_UNROLL
	for (int k = 0; k < 9; ++k) {
		kernel[k] = float3(
			rand(uv.x) * 2 - 1,
			rand(uv.y) * 2 - 1,
			rand(uv.x + uv.y));

			normalize(kernel[k]);

			//Distribute within hemisphere
			kernel[k] *= rand(1);

			//Scale towards center of kernel
			float scale = (float)k / (float)9;
			scale = lerp(0.1, 1, scale * scale);
			kernel[k] *= scale;
	}

	//Sample normals
	half3 norm = tex2Dlod(normals, float4(uv.xy, 0, 0)).rgb;
	//Remap to 0-1
	norm = norm * 2.0 - 1.0;
	norm = normalize(norm);

	/*
	UNITY_UNROLL
	for (int n = 0; n < 4; ++n) {
		noiseVectors[n] = float3(
			rand(uv.x) * 2 - 1,
			rand(uv.y) * 2 - 1,
			0.0f
		);
		normalize(noiseVectors[n]);
	}
	*/

	float3 noise = tex2D(noiseTex, uv * 1024/32).xyz * 2.0 - 1.0;
	noise = noise * 0.5 + 0.5;
	noise = normalize(noise);
	float3 tangent = normalize(noise - norm * dot(noise, norm));
	float3 bitangent = cross(norm, tangent);
	//float3x3 tbn = float3x3(tangent, bitangent, norm);

	float ao = 0;
	float depth = tex2D(heightmap, uv.xy).r;

	//depth = LinearToGammaSpace(depth);

	float radius = 50 * 0.001;

	//float centerDepth = tex2D(heightmap, uv.xy).r;
	for (uint i = 0; i < 9; i++)
	{
		//Sample point
		//float2 s = mul(tbn, kernel[i]);
		float2 s = radius + uv.xy * kernel[i] * norm;

		//Project sample point
		float2 offset = s;

		float sampleDepth = tex2D(heightmap, offset).r;

		float rangeCheck = abs(depth - sampleDepth) <= radius ? 1.0 : 0.0;
		ao += (sampleDepth <= depth ? 1 : 0.0) * rangeCheck;
		
	}

	ao = (1 - ao );
	//return float4(norm, 1);
	return float4(ao, ao, ao, 1);
}

#define CurvStrength 2

//single channel overlay
float BlendOverlay(float a, float b)
{
	return (b < 0.5) ? 2.0 * a * b : 1.0 - 2.0 * (1.0 - a) * (1.0 - b);
}

//https://blender.stackexchange.com/questions/89278/how-to-get-a-smooth-curvature-map-from-a-normal-map
float4 CurvatureFromNormal(sampler2D normals, float2 uv) {

		float width = (1 * _ScreenParams.z);

		float posX = tex2D(normals, float2(uv.x + width, uv.y)).x * CurvStrength;
		float negX = tex2D(normals, float2(uv.x - width, uv.y)).x * CurvStrength;

		float x = (posX - negX) + 0.5;

		float posY = tex2D(normals, float2(uv.x, uv.y + width)).y * CurvStrength;
		float negY = tex2D(normals, float2(uv.x, uv.y - width)).y * CurvStrength;

		float y = (posY - negY) + 0.5;

		return BlendOverlay(x,y);
}

float Contours(sampler2D heightmap, float2 uv) {

	float contourLineFactor = 16;
	float majorLineSteps = contourLineFactor / 8;
	float thickness = 2 * 0.001;
	uint mip = 4;

	//White base
	float lines = 1;

	//Partial derivative
	float corner0 = tex2Dlod(heightmap, float4(uv.x, uv.y, 0, mip)).r;
	float corner1 = tex2Dlod(heightmap, float4(uv.x + thickness, uv.y, 0, mip)).r;
	float corner2 = tex2Dlod(heightmap, float4(uv.x, uv.y + thickness, 0, mip)).r;
	float corner3 = tex2Dlod(heightmap, float4(uv.x + thickness, uv.y + thickness, 0, mip)).r;

	// Calculate the elevation range of the pixel's area
	float minHeight = min(min(corner0, corner1), min(corner2, corner3));
	float maxHeight = max(max(corner0, corner1), max(corner2, corner3));

	// Check if the pixel's area crosses at least one contour line
	if (floor(minHeight * contourLineFactor) != floor(maxHeight * contourLineFactor))
	{
		lines = 0.5;
	}

	//Major lines
	if (floor(minHeight * contourLineFactor / majorLineSteps) != floor(maxHeight * contourLineFactor / majorLineSteps))
	{
		lines = 0;
	}

	return lines;
}

float4 Flowmap(sampler2D normals, float2 uv)
{
	float4 n = float4((tex2D(normals, uv.xy)).xyz, 1);
	//n = pow(n, 2.2);
	//n = normalize(n);

	//Swizzle channels
	n.b = n.a;
	//n.a = 1;

	n = n * 2 -1;

	//return n;

	float radius = 5 * 0.001;
	float2 dir = float2(1, 0);
	float2 output = float2(0.5, 0.5);

	float steepest = 0;
	//float centerDepth = tex2D(heightmap, uv.xy).r;
	for (uint i = 0; i < 9; i++)
	{
		//Sample point
		float2 s = uv.xy + (kernel3x3[i] * radius);

		float4 sampleNormal = (tex2D(normals, s)).rgba;
		//sampleNormal = normalize(sampleNormal);
		sampleNormal.b = sampleNormal.a;

		//Remap from [-1.1] to [0.1]
		sampleNormal = sampleNormal * 2 -1;

		float steepness = max(0, dot(sampleNormal, float3(0, 0, 1)));
		//diffCheck = sampleNormal * float3(0, 1, 1);

		//Find the steepest point
		if (steepness > steepest) {
			steepest = steepness;
			output.xy = sampleNormal.xy;
		}
		else {
			output.xy = n.xy;
		}
	}

	return float4(output.xy, 0, 1);
}


float ClippedHeight(sampler2D heightmap, float waterHeight, float2 uv) {

	float height = tex2D(heightmap, uv).r;

	float mask = 1;
	if (height > waterHeight) mask = 0;

	float shoreline = pow((height * mask) * 2, 8);

	return shoreline;
}

float GetSlope(sampler2D heightmap, float2 uv) {

	float height = tex2Dlod(heightmap, float4(uv, 0, 1));
	
	float dx = tex2D(heightmap, float2(uv.x + 1, uv.y)) - height;
	float dy = tex2D(heightmap, float2(uv.x, uv.y + 1)) - height;

	float m = sqrt(dx * dx + dy * dy) * 64;

	return m;
}