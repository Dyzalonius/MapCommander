using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTextureData
{
    public Color[] ColorMap;

    public const char CODE = 'T';
    private const short BYTESIZE = 4096;// * 4096 * 3 * 3; // amount of bytes necessary to send 1 terraintexturedata across
    private const int SIZE = 4096;
    private static readonly byte[] memoryQuadrant = new byte[BYTESIZE];

    public TerrainTextureData(Color[] colorMap)
    {
        this.ColorMap = colorMap;
    }

    public static short SerializeTerrainTextureData(StreamBuffer outStream, object customObject)
    {
        TerrainTextureData terrainTextureData = (TerrainTextureData)customObject;
        Color color;

        lock (memoryQuadrant)
        {
            byte[] bytes = memoryQuadrant;
            int index = 0;
            for (int i = 0; i < terrainTextureData.ColorMap.Length; i++)
            {
                color = terrainTextureData.ColorMap[i]; //TODO: rgb values probably need to be converted to a byte int
                Protocol.Serialize(color.r, bytes, ref index);
                Protocol.Serialize(color.g, bytes, ref index);
                Protocol.Serialize(color.b, bytes, ref index);
            }
            outStream.Write(bytes, 0, BYTESIZE);
        }
        return BYTESIZE;
    }

    public static object DeserializeTerrainTextureData(StreamBuffer inStream, short length)
    {
        Color[] colorMap = new Color[SIZE * SIZE];
        Color color = new Color();

        lock (memoryQuadrant)
        {
            inStream.Read(memoryQuadrant, 0, BYTESIZE);
            int index = 0;
            for (int i = 0; i < length; i++)
            {
                Protocol.Deserialize(out color.r, memoryQuadrant, ref index);
                Protocol.Deserialize(out color.g, memoryQuadrant, ref index);
                Protocol.Deserialize(out color.b, memoryQuadrant, ref index);
                colorMap[i] = color;
            }
        }
        TerrainTextureData terrainTextureData = new TerrainTextureData(colorMap);

        return terrainTextureData;
    }
}
