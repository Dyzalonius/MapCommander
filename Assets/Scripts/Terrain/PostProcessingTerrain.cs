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
	private Camera cam = null;

	[SerializeField]
	private Material material = null;

	[SerializeField]
	private MapGrid mapGrid = null;

	[SerializeField]
	private UnitWaypointLines unitWaypointLines = null;

	[SerializeField]
	private UnitRangeCircles unitRangeCircles = null;

	[Header("Settings")]
	[SerializeField]
	private float terrainIncrementFactor = 0.00001f;

	[SerializeField]
	private float camScaleStep = 100f;

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
