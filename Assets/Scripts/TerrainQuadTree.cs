using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.IO;
using Photon.Pun;

public class TerrainQuadTree : MonoBehaviour//, IPunObservable
{
    [Header("References")]
    [SerializeField]
    private TerrainTexture terrainPlanePrefab;

    [Header("Settings")]
    [SerializeField]
    private Vector2Int position;

    [SerializeField]
    private QuadrantSize minSize;

    [SerializeField]
    private QuadrantSize maxSize;

    [SerializeField]
    public TerrainLoadType loadType;

    [HideInInspector]
    public string filepathPrefixInEditor = "/..";

    [HideInInspector]
    public string filepathTerrainData;

    [HideInInspector]
    public string mapFolderName;

    [HideInInspector]
    public static TerrainQuadTree Instance { get; private set; } // static singleton

    private Quadrant root;
    private List<Quadrant> visibleQuadrants;
    private List<TerrainTexture> terrainTexturePool;
    private Dictionary<Quadrant, TerrainTexture> visibleQuadrantPairs;
    private QuadrantSize visibleQuadrantScale;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        visibleQuadrants = new List<Quadrant>();
        terrainTexturePool = new List<TerrainTexture>();
        visibleQuadrantPairs = new Dictionary<Quadrant, TerrainTexture>();
        visibleQuadrantScale = maxSize;
        BuildTree();
    }

    private void Update()
    {
        UpdateVisibleQuadrantScale();
        UpdateVisibleQuadrants();

        if (Input.GetKeyDown(KeyCode.S))
            SaveTerrain();

        if (Input.GetKeyDown(KeyCode.H))
            ShareTerrain();
    }

    public void BuildTree()
    {
        root = new Quadrant(maxSize, minSize, position);
        switch (loadType)
        {
            case TerrainLoadType.NONE:
                break;
            case TerrainLoadType.LOAD:
                LoadTerrain();
                break;
            case TerrainLoadType.GENERATE:
                StartCoroutine(Threaded.RunOnThread(CreateTerrainData, DrawTerrain));
                break;
        }
    }

    private void UpdateVisibleQuadrantScale()
    {
        float pos1 = Camera.main.ScreenToWorldPoint(Vector3.zero).x;
        float pos2 = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.scaledPixelWidth, 0f, 0f)).x;
        float metersPerPixel = (pos2 - pos1) / Camera.main.scaledPixelWidth;
        float texelsPerMeter = (int)QuadrantSize.k4 / ((float)visibleQuadrantScale);
        float texelsPerPixel = metersPerPixel * texelsPerMeter;
        //Debug.Log("tpp = " + texelsPerPixel);

        // Decrease activeQuadrantScale when zooming out
        if (texelsPerPixel >= 2f && visibleQuadrantScale != maxSize)
            visibleQuadrantScale = NextSize(visibleQuadrantScale, true);

        // Increase activeQuadrantScale when zooming in
        if (texelsPerPixel < 1f && visibleQuadrantScale != minSize)
            visibleQuadrantScale = NextSize(visibleQuadrantScale, false);
    }

    private void UpdateVisibleQuadrants()
    {
        // Find quadrants that have to be visible
        List<Quadrant> newActiveQuadrants = new List<Quadrant>();
        foreach (Quadrant quadrant in root.FindQuadrantsOfSize(visibleQuadrantScale))
            if (quadrant.VisibleByMainCam())
                newActiveQuadrants.Add(quadrant);

        // Deactivate visible quadrants that have to be invisible
        for (int i = visibleQuadrants.Count - 1; i >= 0; i--)
        {
            Quadrant quadrant = visibleQuadrants[i];
            if (!newActiveQuadrants.Contains(quadrant))
            {
                TerrainTexture terrainTexture = visibleQuadrantPairs[quadrant];
                terrainTexture.Deactivate();
                visibleQuadrantPairs.Remove(quadrant);
                terrainTexturePool.Add(terrainTexture);
            }
        }

        // Activate new visible quadrants
        visibleQuadrants = newActiveQuadrants;
        foreach (Quadrant quadrant in visibleQuadrants)
            if (!visibleQuadrantPairs.ContainsKey(quadrant))
            {
                // Only instantiate a new terrainTexture if the pool is empty
                if (terrainTexturePool.Count == 0)
                    terrainTexturePool.Add(Instantiate(terrainPlanePrefab, transform));

                TerrainTexture terrainTexture = terrainTexturePool[0];
                terrainTexturePool.Remove(terrainTexture);
                terrainTexture.gameObject.SetActive(true);
                terrainTexture.Setup(quadrant, loadType != TerrainLoadType.NONE);
                visibleQuadrantPairs.Add(quadrant, terrainTexture);
            }
    }

    private void SaveTerrain()
    {
        Debug.Log("start saving");
        string folderName = "map_" + System.DateTime.Now.ToString("yyyy-MM-dd_hhmmss");
        root.SaveTerrainToPNG((Application.isEditor ? filepathPrefixInEditor : "") + filepathTerrainData + folderName);
        Debug.Log("finish saving");
    }

    private void LoadTerrain()
    {
        Debug.Log("start loading");
        root.LoadTerrainFromPNG((Application.isEditor ? filepathPrefixInEditor : "") + filepathTerrainData + mapFolderName + "/");
        Debug.Log("finish loading");
    }

    private void ShareTerrain()
    {
        Debug.LogError("start sharing");
        SendTerrainData(root.colorMap, root.id);
        Debug.LogError("finish sharing");
    }

    private void CreateTerrainData()
    {
        root.GenerateTerrainData();
    }

    private void DrawTerrain()
    {
        root.GenerateTerrainTexture();
    }

    // Grab next size from QuadrantSize enum
    private QuadrantSize NextSize(QuadrantSize size, bool nextRatherThanPrevious = true)
    {
        QuadrantSize[] list = (QuadrantSize[])Enum.GetValues(typeof(QuadrantSize));
        if (nextRatherThanPrevious)
            return list[Array.IndexOf<QuadrantSize>(list, size) + 1];
        else
            return list[Array.IndexOf<QuadrantSize>(list, size) - 1];
    }

    void OnDrawGizmos()
    {
        if (visibleQuadrantPairs == null) return;

        foreach (var pair in visibleQuadrantPairs)
        {
            Vector2Int position = pair.Key.position;
            Vector2Int size = pair.Key.size;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(new Vector3(position.x + size.x / 2, 0f, position.y + size.y / 2), new Vector3(size.x, 0f, size.y));
        }
        Gizmos.DrawWireCube(new Vector3(root.position.x + root.size.x / 2, 0f, root.position.y + root.size.y / 2), new Vector3(root.size.x, 0f, root.size.y));
    }

    public void UpdateTerrainTexture(Quadrant quadrant)
    {
        if (visibleQuadrantPairs.ContainsKey(quadrant))
            visibleQuadrantPairs[quadrant].UpdateTexture();
    }

    public void SendTerrainData(Color[] colorMap, string id)
    {

        for (int i = 0; i < 128; i++)
        {
            string fileName = (id == "" ? "Quadrant" : "Quadrant_") + id + ".png" + " (" + i + ")";
            byte[] terrainData = new byte[Quadrant.textureSize * 32 * 2];

            for (int j = 0; j < Quadrant.textureSize * 32; j++)
            {
                terrainData[2 * j] = (byte)(colorMap[(128 * i) + j].r * 255);
                terrainData[(2 * j) + 1] = (byte)(colorMap[(128 * i) + j].g * 255);
            }

            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("ReceiveTerrainDataRPC", RpcTarget.Others, terrainData, fileName);
        }
    }

    [PunRPC]
    public void ReceiveTerrainDataRPC(byte[] terrainData, string fileName, PhotonMessageInfo info)
    {
        //Debug.LogError("start receiving");
        // Create color map using terrainData
        Color[] colorMap = new Color[Quadrant.textureSize * 32];
        for (int i = 0; i < Quadrant.textureSize * 32; i++)
        {
            Color color = new Color
            {
                r = terrainData[2 * i],
                g = terrainData[(2 * i) + 1],
                b = 0
            };
            colorMap[i] = color;
        }
        //Debug.LogError("> colormap done");

        // Create texture using color map
        /*Texture2D texture = new Texture2D(Quadrant.textureSize, Quadrant.textureSize)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        texture.SetPixels(colorMap);
        texture.Apply();
        Debug.LogError("> texture done");

        // Create png using texture
        byte[] bytes = texture.EncodeToPNG();
        var dirPath = Application.dataPath + (Application.isEditor ? filepathPrefixInEditor : "") + filepathTerrainData;

        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);

        File.WriteAllBytes(dirPath + fileName, bytes);
        Debug.LogError("> texture done");*/

        //Debug.LogFormat("Info: {0} {1} {2}", info.Sender, info.photonView, info.SentServerTime);
        //Debug.LogFormat("Difference = ", PhotonNetwork.Time - info.SentServerTimestamp);
        Debug.LogError("finish receiving " + fileName);

        //Temp
        //LoadTerrain();
    }

    public void TryPaint(Vector2Int position, TerrainBrushMode mode)
    {
        Quadrant quadrantContainingPosition = null;
        Vector2Int positionOnQuadrant = new Vector2Int(-1, -1);

        foreach (Quadrant quadrant in root.GetLeafQuadrants()) //TODO: Optimize by only getting leafquadrants of visible quadrants
        {
            positionOnQuadrant = quadrant.PositionOnQuadrant(position);

            if (positionOnQuadrant != new Vector2Int(-1, -1))
            {
                quadrantContainingPosition = quadrant;
                break;
            }
        }

        if (quadrantContainingPosition != null && positionOnQuadrant != new Vector2Int(-1, -1))
            quadrantContainingPosition.TryPaint(positionOnQuadrant, mode);
        else
            Debug.LogError("Couldn't find a quadrant or a local position to paint on - " + positionOnQuadrant);
    }

    public void PaintSend(Vector2Int position, Color color, string quadrantID)
    {
        PhotonView photonView = PhotonView.Get(this);
        byte[] colorArray = new byte[] { (byte)(color.r * 255), (byte)(color.g * 255), (byte)(color.b * 255), (byte)(color.a * 255) };
        photonView.RPC("PaintReceiveRPC", RpcTarget.All, position.x, position.y, colorArray, quadrantID);
    }

    [PunRPC]
    public void PaintReceiveRPC(int posX, int posY, byte[] colorArray, string quadrantID, PhotonMessageInfo info)
    {
        List<Quadrant> allQuadrants = root.GetLeafQuadrants();
        Quadrant quadrant = allQuadrants.Find(x => x.id == quadrantID);
        Color color = new Color((float)colorArray[0] / 255, (float)colorArray[1] / 255, (float)colorArray[2] / 255, (float)colorArray[3] / 255);

        quadrant.Paint(new Vector2Int(posX, posY), color);
    }

    /*public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //stream.SendNext(new object());
        }

        if (stream.IsReading)
        {
            //object thing = stream.ReceiveNext();
        }
    }*/
}

public enum QuadrantSize
{
    k4 = 4096,
    k8 = 8192,
    k16 = 16384,
    k32 = 32768,
    k64 = 65536,
    k128 = 131072
}

public enum TerrainLoadType
{
    NONE,
    LOAD,
    GENERATE
}