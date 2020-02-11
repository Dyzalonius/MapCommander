using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

//[CustomEditor(typeof(TerrainMapRenderer))]
public class TerrainMapRendererEditor : Editor
{
    private const float CLIP_PADDING = 0.33f;

    public enum Map
    {
        Albedo,
        Height,
        Normal,
        AmbientOcclusion,
        Curvature,
        Contour,
        Flow,
        Hillshade,
        Shoreline,
        Slope
    }

    private static Material mat;
    private static Shader shader { get { return Shader.Find("Hidden/TerrainMapRenderer"); } }
    private static RenderTexture rt;

    private void OnEnable()
    {
        Align();
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Open window"))
        {
            TerrainMapRendererWindow.Init();
        }
        base.OnInspectorGUI();
    }

    public static void Align()
    {
        TerrainMapRenderer.Instance.bounds = TerrainMapRendererUtilities.GetBounds();

        TerrainMapRenderer.Instance.renderCam.name = "TerrainMapRenderer";

        //Set up a square camera rect
        TerrainMapRenderer.Instance.renderCam.rect = new Rect(0, 0, 1, 1);

        //Camera set up
        TerrainMapRenderer.Instance.renderCam.clearFlags = CameraClearFlags.SolidColor;
        TerrainMapRenderer.Instance.renderCam.backgroundColor = Color.magenta;
        TerrainMapRenderer.Instance.renderCam.orthographic = true;
        TerrainMapRenderer.Instance.renderCam.orthographicSize = (TerrainMapRenderer.Instance.bounds.size.x);
        TerrainMapRenderer.Instance.renderCam.allowMSAA = false;

        TerrainMapRenderer.Instance.renderCam.farClipPlane = (TerrainMapRenderer.Instance.bounds.size.y * 2f) + CLIP_PADDING;
        TerrainMapRenderer.Instance.renderCam.useOcclusionCulling = false;

        TerrainMapRenderer.Instance.renderCam.cullingMask = TerrainMapRenderer.Instance.layer;

        //Rendering in Forward mode is a tad darker, so a Directional Light is used to make up for the difference
        //renderCam.renderingPath = (useAlternativeRenderer || workflow == TerrainUVUtil.Workflow.Mesh) ? RenderingPath.Forward : RenderingPath.VertexLit;

        //Position cam in given center of terrain(s)
        TerrainMapRenderer.Instance.renderCam.transform.position = new Vector3(
            TerrainMapRenderer.Instance.bounds.center.x,
            (TerrainMapRenderer.Instance.bounds.center.y + TerrainMapRenderer.Instance.bounds.size.y) + CLIP_PADDING,
            TerrainMapRenderer.Instance.bounds.center.z
            );

        TerrainMapRenderer.Instance.renderCam.transform.localEulerAngles = new Vector3(90, 0, 0);
    }

    public static void Preview(Map type)
    {
        ExecuteRender(type);
    }

    public static void ExecuteRender(Map type)
    {
        //Align();

        switch (type)
        {
            case Map.Albedo:
                RenderAlbedo();
                break;
            case Map.Normal:
                RenderNormals();
                break;
            case Map.Height:
                RenderHeight();
                break;
            case Map.AmbientOcclusion:
                RenderAmbientOcclusion();
                break;
            case Map.Curvature:
                RenderCurvature();
                break;
            case Map.Hillshade:
                RenderAspect();
                break;
            case Map.Contour:
                RenderContour();
                break;
            case Map.Flow:
                RenderFlow();
                break;
            case Map.Shoreline:
                RenderShoreline();
                break;
            case Map.Slope:
                RenderSlope();
                break;
        }
    }

    private static void RenderAlbedo()
    {
        TerrainMapRenderer.Instance.AlbedoTex = RenderToTexture(Map.Albedo);
        Shader.SetGlobalTexture("_TerrainAlbedo", TerrainMapRenderer.Instance.AlbedoTex);

    }

    private static void RenderHeight()
    {
        TerrainMapRenderer.Instance.HeightTex = RenderToTexture(Map.Height);
        Shader.SetGlobalTexture("_Heightmap", TerrainMapRenderer.Instance.HeightTex);
    }

    private static void RenderNormals()
    {
        if (!TerrainMapRenderer.Instance.HeightTex)
        {
            RenderHeight();
        }
        else
        {
            Shader.SetGlobalTexture("_Heightmap", TerrainMapRenderer.Instance.HeightTex);
        }

        TerrainMapRenderer.Instance.NormalTex = RenderToTexture(Map.Normal);
        Shader.SetGlobalTexture("_Normals", TerrainMapRenderer.Instance.NormalTex);

    }

    private static void RenderAmbientOcclusion()
    {

        if(TerrainMapRenderer.Instance.noiseTex) Shader.SetGlobalTexture("_NoiseTex", TerrainMapRenderer.Instance.noiseTex);
        if (!TerrainMapRenderer.Instance.HeightTex)
        {
            RenderHeight();
        }
        else
        {
            Shader.SetGlobalTexture("_Heightmap", TerrainMapRenderer.Instance.HeightTex);
        }
        if (!TerrainMapRenderer.Instance.NormalTex)
        {
            RenderNormals();
        }
        else
        {
            Shader.SetGlobalTexture("_Normals", TerrainMapRenderer.Instance.NormalTex);
        }

        TerrainMapRenderer.Instance.AmbientOcclusionTex = RenderToTexture(Map.AmbientOcclusion);
    }

    private static void RenderCurvature()
    {
        if (!TerrainMapRenderer.Instance.NormalTex)
        {
            RenderNormals();
        }
        else
        {
            Shader.SetGlobalTexture("_Normals", TerrainMapRenderer.Instance.NormalTex);
        }
        TerrainMapRenderer.Instance.CurvatureTex = RenderToTexture(Map.Curvature);
    }

    private static void RenderFlow()
    {
        if (!TerrainMapRenderer.Instance.NormalTex)
        {
            RenderNormals();
        }
        else
        {
            Shader.SetGlobalTexture("_Normals", TerrainMapRenderer.Instance.NormalTex);
        }
        TerrainMapRenderer.Instance.FlowTex = RenderToTexture(Map.Flow);
    }

    private static void RenderAspect()
    {
        if (!TerrainMapRenderer.Instance.NormalTex)
        {
            RenderNormals();
        }
        else
        {
            Shader.SetGlobalTexture("_Normals", TerrainMapRenderer.Instance.NormalTex);
        }

        TerrainMapRenderer.Instance.CurvatureTex = RenderToTexture(Map.Hillshade);
    }
    private static void RenderContour()
    {
        if (!TerrainMapRenderer.Instance.HeightTex)
        {
            RenderHeight();
        }
        else
        {
            Shader.SetGlobalTexture("_Heightmap", TerrainMapRenderer.Instance.HeightTex);
        }

        TerrainMapRenderer.Instance.ContourTex = RenderToTexture(Map.Contour);
    }

    private static void RenderShoreline()
    {
        if (!TerrainMapRenderer.Instance.HeightTex)
        {
            RenderHeight();
        }
        else
        {
            Shader.SetGlobalTexture("_Heightmap", TerrainMapRenderer.Instance.HeightTex);
        }

        if (TerrainMapRenderer.Instance.waterPlane) TerrainMapRendererUtilities.GetRelativeWaterHeight(TerrainMapRenderer.Instance.waterPlane.transform);

        TerrainMapRenderer.Instance.ShorelineTex = RenderToTexture(Map.Shoreline);
    }

    private static void RenderSlope()
    {
        if (!TerrainMapRenderer.Instance.HeightTex)
        {
            RenderHeight();
        }
        else
        {
            Shader.SetGlobalTexture("_Heightmap", TerrainMapRenderer.Instance.HeightTex);
        }

        TerrainMapRenderer.Instance.SlopeTex = RenderToTexture(Map.Slope);
    }

    private static Texture2D RenderToTexture(Map map)
    {
        if (mat == null) mat = new Material(shader);

        //Set up render texture
        RenderTextureFormat rtFormat = RenderTextureFormat.ARGB32;
        RenderTextureReadWrite readWrite = RenderTextureReadWrite.Linear;
        switch (map)
        {
            case Map.Height:
                rtFormat = RenderTextureFormat.ARGBFloat;
                readWrite = RenderTextureReadWrite.Linear;
                break;
            case Map.Normal: readWrite = RenderTextureReadWrite.Linear; //SHOULD BE GAMMA FOR SAVING
                break;
            case Map.Hillshade:
                readWrite = RenderTextureReadWrite.Linear;
                break;
            case Map.Albedo: readWrite = RenderTextureReadWrite.sRGB;
                break;
            case Map.Flow:
                readWrite = RenderTextureReadWrite.Linear;
                break;
            case Map.Contour:
                readWrite = RenderTextureReadWrite.Linear;
                break;
        }
        rt = new RenderTexture(TerrainMapRenderer.Instance.resolution, TerrainMapRenderer.Instance.resolution, 0, rtFormat, readWrite);
        rt.filterMode = FilterMode.Bilinear;

        //Get result from shader pass
        if (map != Map.Albedo)
        {
            TerrainMapRenderer.Instance.cmd.Blit(BuiltinRenderTextureType.CameraTarget, rt, mat, (int)map);
        }
        else
        {
            TerrainMapRenderer.Instance.renderCam.SetReplacementShader(Shader.Find("Unlit/Texture"), "");
        }

        RenderTexture.active = rt;
        TerrainMapRenderer.Instance.renderCam.targetTexture = rt;
        TerrainMapRenderer.Instance.renderCam.Render();

        //Store result in texture
        TextureFormat textureFormat = TextureFormat.RGB24;
        if (map == Map.Height)
        {
            textureFormat = TextureFormat.RGBAFloat;
        }
        //Normal map requires alpha channel
        if(map == Map.Normal)
        {
            textureFormat = TextureFormat.ARGB32;
        }

        Texture2D render = new Texture2D(TerrainMapRenderer.Instance.resolution, TerrainMapRenderer.Instance.resolution, textureFormat, true, true);
        render.name = map.ToString();
        render.ReadPixels(new Rect(0, 0, TerrainMapRenderer.Instance.resolution, TerrainMapRenderer.Instance.resolution), 0, 0, false);
        render.Apply();

        TerrainMapRenderer.Instance.previewTex = render;

        if (map == Map.Albedo) TerrainMapRenderer.Instance.renderCam.ResetReplacementShader();

        RenderTexture.active = null;

        return render;
    }

    public static void SaveTextures()
    {
        EditorUtility.DisplayProgressBar("Terrain Map Renderer", "Saving textures...", 1f);

        if (TerrainMapRenderer.Instance.renderAlbedo)
        {
            if (!TerrainMapRenderer.Instance.AlbedoTex) RenderAlbedo();
            TerrainMapRendererUtilities.SaveTexture(TerrainMapRenderer.Instance.AlbedoTex);
        }
        if (TerrainMapRenderer.Instance.renderHeight)
        {
            if (!TerrainMapRenderer.Instance.HeightTex) RenderHeight();
            TerrainMapRendererUtilities.SaveTexture(TerrainMapRenderer.Instance.HeightTex, TerrainMapRendererUtilities.FileFormat.PNG);
        }
        if (TerrainMapRenderer.Instance.renderNormals)
        {
            if (!TerrainMapRenderer.Instance.NormalTex) RenderNormals();
            TerrainMapRendererUtilities.SaveTexture(TerrainMapRenderer.Instance.NormalTex);
        }
        if (TerrainMapRenderer.Instance.renderAmbientOcclusion)
        {
            if (!TerrainMapRenderer.Instance.AmbientOcclusionTex) RenderAmbientOcclusion();
            TerrainMapRendererUtilities.SaveTexture(TerrainMapRenderer.Instance.AmbientOcclusionTex);
        }
        if (TerrainMapRenderer.Instance.renderContour)
        {
            if (!TerrainMapRenderer.Instance.ContourTex) RenderContour();
            TerrainMapRendererUtilities.SaveTexture(TerrainMapRenderer.Instance.ContourTex);
        }
        if (TerrainMapRenderer.Instance.renderCurvature)
        {
            if (!TerrainMapRenderer.Instance.CurvatureTex) RenderCurvature();
            TerrainMapRendererUtilities.SaveTexture(TerrainMapRenderer.Instance.CurvatureTex);
        }
        if (TerrainMapRenderer.Instance.renderShoreline)
        {
            if (!TerrainMapRenderer.Instance.ShorelineTex) RenderShoreline();
            TerrainMapRendererUtilities.SaveTexture(TerrainMapRenderer.Instance.ShorelineTex);
        }
        if (TerrainMapRenderer.Instance.renderSlope)
        {
            if (!TerrainMapRenderer.Instance.SlopeTex) RenderSlope();
            TerrainMapRendererUtilities.SaveTexture(TerrainMapRenderer.Instance.SlopeTex);
        }

        EditorUtility.ClearProgressBar();
    }


}
