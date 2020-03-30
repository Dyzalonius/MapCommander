//https://www.alanzucconi.com/2015/07/08/screen-shaders-and-postprocessing-effects-in-unity3d/
//http://catlikecoding.com/unity/tutorials/advanced-rendering/bloom/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PostProcessingTerrain : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Camera cam;

	[SerializeField]
	private Material material;

	[SerializeField]
	private MapGrid mapGrid;

	[SerializeField]
	private UnitWaypointLines unitWaypointLines;

	[SerializeField]
	private UnitRangeCircles unitRangeCircles;

	[Header("Settings")]
	[SerializeField]
	private float terrainIncrementFactor;

	[SerializeField]
	private float camScaleStep;

	[SerializeField]
	private float terrainIncrementMinimum = 0.005f;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		material.SetFloat("_Increment", Mathf.Max(Mathf.Floor(cam.orthographicSize / camScaleStep) * camScaleStep * terrainIncrementFactor, terrainIncrementMinimum));
		Graphics.Blit(source, destination, material);

		mapGrid.DrawGrid();
		unitWaypointLines.DrawLines();
		unitRangeCircles.DrawCircles();
	}
}
