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
    private TerrainTexture terrainPlanePrefab = null;

    [Header("Settings")]
    [SerializeField]
    private Vector2Int position = Vector2Int.zero;

    [SerializeField]
    private QuadrantSize minSize = QuadrantSize.p512;

    [SerializeField]
    private QuadrantSize maxSize = QuadrantSize.k8;

    [SerializeField]
    public TerrainLoadType loadType = TerrainLoadType.LOAD;

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
    private bool allowQuadrantRequests = true;

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
        UpdateQuadrantChanges();

        if (Input.GetKeyDown(KeyCode.S))
            SaveTerrain();

        if (Input.GetKeyDown(KeyCode.H))
            ShareTerrain();

        if (Input.GetKeyDown(KeyCode.U))
        {
            List<Quadrant> quadrants = root.GetLeafQuadrants();
            quadrants.ForEach(x => x.ApplyAllTerrainChanges());
        }

        if (Input.GetKeyDown(KeyCode.C))
            ConvertTerrainTo512();

        if (Input.GetKeyDown(KeyCode.E))
        {
            allowQuadrantRequests = !allowQuadrantRequests;
            Debug.LogError("AllowQuadrantRequests = " + allowQuadrantRequests);
        }
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
        float texelsPerMeter = (int)minSize / ((float)visibleQuadrantScale);
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
                quadrant.SetVisible(false);
                TerrainTexture terrainTexture = visibleQuadrantPairs[quadrant];
                terrainTexture.Deactivate();
                visibleQuadrantPairs.Remove(quadrant);
                terrainTexturePool.Add(terrainTexture);
            }
        }

        // Activate new visible quadrants
        visibleQuadrants = newActiveQuadrants;
        foreach (Quadrant quadrant in visibleQuadrants)
        {
            if (!visibleQuadrantPairs.ContainsKey(quadrant))
            {
                quadrant.SetVisible(true);

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
    }

    private void UpdateQuadrantChanges()
    {
        List<Quadrant> leafQuadrants = root.GetLeafQuadrants();
        leafQuadrants.ForEach(x => x.UpdateTerrainChanges());
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

    // This was a one use method for converting to 512x512
    private void ConvertTerrainTo512()
    {
        Debug.Log("convert start");

        // add every color to the color map
        Color[] colorMap = new Color[(int)maxSize * (int)maxSize];
        foreach (Quadrant quadrant in root.GetLeafQuadrants())
            for (int i = 0; i < quadrant.colorMap.Length; i++)
                colorMap[((quadrant.position.y + Mathf.FloorToInt(i / (int)minSize)) * (int)maxSize) + quadrant.position.x + (i % (int)minSize)] = quadrant.colorMap[i];

        Debug.Log("colormap done");

        // create new quadrants with new scale
        minSize = QuadrantSize.p256;
        root = new Quadrant(maxSize, minSize, position);

        Debug.Log("quadrant creation done");

        // give quadrants appropriate colormaps
        foreach (Quadrant quadrant in root.GetLeafQuadrants())
        {
            quadrant.colorMap = new Color[(int)minSize * (int)minSize];
            for (int i = 0; i < quadrant.colorMap.Length; i++)
                quadrant.colorMap[i] = colorMap[((quadrant.position.y + Mathf.FloorToInt(i / (int)minSize)) * (int)maxSize) + quadrant.position.x + (i % (int)minSize)];
        }

        // give parent quadrants appropriate colormaps
        root.BuildColorMaps();

        Debug.Log("colormap assignment done");

        // generate textures for all quadrants
        DrawTerrain();

        // save all textures
    }

    private void ShareTerrain()
    {
        SendTerrainData(root.colorMap, root.id);
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

    public void SendQuadrantRequest(string id)
    {
        if (!allowQuadrantRequests)
        {
            return;
        }
        Debug.LogError("SendQuadrantRequest " + id);

        // Send RPC
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("ReceiveQuadrantRequestRPC", RpcTarget.MasterClient, id);
    }

    [PunRPC]
    public void ReceiveQuadrantRequestRPC(string id, PhotonMessageInfo info)
    {
        Debug.LogError("ReceiveQuadrantRequest " + id);
        Quadrant quadrant = root.FindQuadrantByID(id);

        // Exit if no quadrant found
        if (quadrant == null)
        {
            Debug.LogWarning("No quadrant found of id " + id);
            return;
        }

        SendQuadrant(quadrant, info.Sender);
    }

    public void SendQuadrant(Quadrant quadrant, Photon.Realtime.Player recipient)
    {
        // Exit if quadrant is null
        if (quadrant == null || quadrant.colorMap == null)
        {
            Debug.LogWarning("Quadrant or it's colorMap is null!");
            return;
        }

        Debug.LogError("SendQuadrant " + quadrant.id);

        byte[] quadrantData = new byte[(int)minSize * (int)minSize * 2];
        for (int i = 0; i < (int)minSize * (int)minSize; i++)
        {
            quadrantData[2 * i] = (byte)(quadrant.colorMap[i].r * 255);
            quadrantData[(2 * i) + 1] = (byte)(quadrant.colorMap[i].g * 255);
        }

        // Send RPC
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("ReceiveQuadrantRPC", recipient, quadrantData, quadrant.id);
    }

    [PunRPC]
    public void ReceiveQuadrantRPC(byte[] quadrantData, string id, PhotonMessageInfo info)
    {
        Debug.LogError("ReceiveQuadrantRPC " + id);
        // Create color map using quadrantData
        Color[] colorMap = new Color[(int)minSize * (int)minSize];
        for (int i = 0; i < (int)minSize * (int)minSize; i++)
        {
            Color color = new Color
            {
                r = ((float)quadrantData[2 * i]) / 255,
                g = ((float)quadrantData[(2 * i) + 1]) / 255,
                b = 0
            };
            colorMap[i] = color;
        }

        Quadrant quadrant = root.FindQuadrantByID(id);
        if (quadrant != null)
        {
            quadrant.LoadTerrainFromArray(colorMap);
        }
        else
        {
            Debug.LogWarning("No quadrant found!");
        }
    }

    public void SendTerrainData(Color[] colorMap, string id)
    {
        Debug.LogError("start sharing");

        byte[] terrainData = new byte[(int)minSize * (int)minSize * 2];

        for (int i = 0; i < (int)minSize * (int)minSize; i++)
        {
            terrainData[2 * i] = (byte)(colorMap[i].r * 255);
            terrainData[(2 * i) + 1] = (byte)(colorMap[i].g * 255);
        }

        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("ReceiveTerrainDataRPC", RpcTarget.AllViaServer, terrainData, id);

        Debug.LogError("finish sharing");
    }

    [PunRPC]
    public void ReceiveTerrainDataRPC(byte[] terrainData, string id, PhotonMessageInfo info)
    {
        Debug.LogError("start receiving");

        // Create color map using terrainData
        Color[] colorMap = new Color[(int)minSize * (int)minSize];
        for (int i = 0; i < (int)minSize * (int)minSize; i++)
        {
            Color color = new Color
            {
                r = ((float)terrainData[2 * i]) / 255,
                g = ((float)terrainData[(2 * i) + 1]) / 255,
                b = 0
            };
            colorMap[i] = color;
        }
        
        Quadrant quadrant = root.FindQuadrantByID(id);
        if (quadrant != null)
        {
            quadrant.LoadTerrainFromArray(colorMap);
        }
        else
        {
            Debug.LogWarning("No quadrant found!");
        }

        Debug.LogError("finish receiving " + id);
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

        quadrant.EditTerrain(new Vector2Int(posX, posY), color);
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