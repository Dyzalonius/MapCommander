//https://www.alanzucconi.com/2015/07/08/screen-shaders-and-postprocessing-effects-in-unity3d/
//http://catlikecoding.com/unity/tutorials/advanced-rendering/bloom/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PostProcessingTerrain : MonoBehaviour
{
	[SerializeField]
	private Material material;

	[SerializeField]
	private MapGrid mapGrid;

	[SerializeField]
	private UnitWaypointLines unitWaypointLines;

	[SerializeField]
	private UnitRangeCircles unitRangeCircles;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, material);

		mapGrid.DrawGrid();
		unitWaypointLines.DrawLines();
		unitRangeCircles.DrawCircles();
	}
}
