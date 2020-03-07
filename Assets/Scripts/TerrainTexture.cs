using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTexture : MonoBehaviour
{
    [SerializeField]
    private Renderer textureRenderer;

    private Vector3 planeScale;
    private Vector3 planePosition;
    private Quadrant quadrant;

    private bool generate;

    public void Setup(Quadrant quadrant, bool generate)
    {
        this.quadrant = quadrant;
        this.generate = generate;
        planeScale = new Vector3(((float)quadrant.size.x) / 10f, 1f, ((float)quadrant.size.y) / 10f);
        planePosition = new Vector3(quadrant.position.x + ((float) quadrant.size.x) / 2f, 0f, quadrant.position.y + ((float)quadrant.size.y) / 2f);

        if (generate)
            textureRenderer.material.mainTexture = quadrant.texture;
        textureRenderer.transform.localScale = planeScale;
        textureRenderer.transform.localPosition = planePosition;
    }

    private void Update()
    {
        if (generate && quadrant.texture != textureRenderer.material.mainTexture)
            textureRenderer.material.mainTexture = quadrant.texture;
    }

    public void Deactivate()
    {
        //textureRenderer.material.mainTexture = null;
        gameObject.SetActive(false);
    }
}
