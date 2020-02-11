using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class TerrainMapRenderer : MonoBehaviour
{
    public static TerrainMapRenderer Instance;

    [Header("Settings")]
    public string outputFolder = "Assets/TerrainMaps";
    public int resolution = 1024;
    public LayerMask layer = -1;
    public GameObject waterPlane;

    public Texture2D noiseTex;

    [HideInInspector]
    public bool renderHeight = true;
    [HideInInspector]
    public bool renderAlbedo = true;
    [HideInInspector]
    public bool renderNormals = true;
    [HideInInspector]
    public bool renderAmbientOcclusion = true;
    [HideInInspector]
    public bool renderCurvature = false;
    [HideInInspector]
    public bool renderFlow = false;
    [HideInInspector]
    public bool renderHillshade = false;
    [HideInInspector]
    public bool renderContour = false;
    [HideInInspector]
    public bool renderAspect = false;
    [HideInInspector]
    public bool renderShoreline = false;
    [HideInInspector]
    public bool renderSlope = false;
    [HideInInspector]
    public Camera renderCam;
    [HideInInspector]
    public Bounds bounds;

    [Header("Output")]
    public Texture2D AlbedoTex;
    public Texture2D HeightTex;
    public Texture2D NormalTex;
    public Texture2D AmbientOcclusionTex;
    public Texture2D CurvatureTex;
    public Texture2D FlowTex;
    public Texture2D ContourTex;
    public Texture2D ShorelineTex;
    public Texture2D SlopeTex;

    public CommandBuffer cmd;
    public Texture2D previewTex;

    private void OnEnable()
    {
        Instance = this;

        renderCam = this.GetComponent<Camera>();

        cmd = new CommandBuffer();
        cmd.name = this.name;

        renderCam.AddCommandBuffer(CameraEvent.AfterEverything, cmd);
    }

    private void OnDisable()
    {
        Instance = null;
        if (cmd != null) renderCam.RemoveCommandBuffer(CameraEvent.AfterEverything, cmd);
    }

    private void OnDrawGizmos()
    {
        Vector3 gizmoPos = new Vector3(bounds.center.x, this.transform.position.y - bounds.size.y, bounds.center.z);

        Gizmos.DrawWireCube(gizmoPos, bounds.size * 2);
    }
}
