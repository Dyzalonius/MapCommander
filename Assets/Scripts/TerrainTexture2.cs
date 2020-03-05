using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTexture2 : MonoBehaviour
{
    [SerializeField]
    private Renderer textureRenderer;

    [HideInInspector]
    public Texture2D texture;

    private Vector3 planeScale;
    private Vector3 planePosition;

    public void Setup(Quadrant quadrant)
    {
        planeScale = new Vector3(((float)quadrant.size.x) / 10f, 1f, ((float)quadrant.size.y) / 10f);
        planePosition = new Vector3(quadrant.position.x + ((float) quadrant.size.x) / 2f, 0f, quadrant.position.y + ((float)quadrant.size.y) / 2f);

        textureRenderer.material.mainTexture = quadrant.texture;
        textureRenderer.transform.localScale = planeScale;
        textureRenderer.transform.localPosition = planePosition;
    }
}
