using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class TerrainMapRendererUtilities : Editor
{
    public static string SaveFolder
    {
        get
        {
            return EditorPrefs.GetString(PlayerSettings.productName + "_TMR_DIR", "Assets/TerrainMaps");
        }
        set { EditorPrefs.SetString(PlayerSettings.productName + "_TMR_DIR", value); }
    }

    public static string AlbedoTexPath
    {
        get
        {
            return EditorPrefs.GetString(PlayerSettings.productName + "_TMR_ALBEDO_PATH", string.Empty);
        }
        set { EditorPrefs.SetString(PlayerSettings.productName + "_TMR_ALBEDO_PATH", value); }
    }

    private static Terrain[] _Terrains;
    public static Terrain[] Terrains
    {
        get
        {
            if (_Terrains == null) RefreshTerrains();
            return _Terrains;
        }
        set
        {
            _Terrains = value;
        }
    }

    public static int TerrainTilesCount
    {
        get
        {
            return Terrains.Length;
        }
    }

    public static void RefreshTerrains()
    {
        Terrains = Terrain.activeTerrains;
    }

    private static void GetChildTerrains(Transform transform)
    {
        //Clear array
        Terrains = null;

        //All childs, recursive
        Transform[] children = transform.GetComponentsInChildren<Transform>(true);

        int childCount = 0;

        //Count first level transforms
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].parent == transform) childCount++;
        }

        //Temp list
        List<Terrain> terrainList = new List<Terrain>();

        //Init array with childcount length
        Terrains = new Terrain[childCount];

        //Fill array with first level transforms that are terrains
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].parent == transform)
            {
                Terrain t = children[i].GetComponent<Terrain>();

                if (t) terrainList.Add(t);

            }
        }

        Terrains = terrainList.ToArray();

        //Sort alphanumerically
        Terrains = Terrains.OrderBy(go => go.name, new TerrainNameComparer()).ToArray();
    }

    internal static Bounds GetBounds()
    {
        Bounds bounds = new Bounds();
        Vector3 size = Vector3.zero;

        Terrain terrain;

        RefreshTerrains();

        for (int i = 0; i < Terrains.Length; i++)
        {
            terrain = Terrains[i];

            Bounds terrainBounds = new Bounds(terrain.terrainData.bounds.center + terrain.transform.position, terrain.terrainData.bounds.size);

            bounds.Encapsulate(terrainBounds);
        }

        bounds.size /= 2;

        return bounds;
    }

    public class TerrainNameComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            var t1 = x.Split('_');
            var t2 = y.Split('_');

            var num1 = Convert.ToInt32(t1[1]);
            var num2 = Convert.ToInt32(t2[1]);

            return num1.CompareTo(num2);
        }
    }

    public static float GetRelativeWaterHeight(Transform t)
    {
        float height = (t.position.y - GetBounds().min.y) / GetBounds().size.y;

        Shader.SetGlobalFloat("_WaterHeight", height);

        Debug.Log("Relative water height: " + height);

        return height;
    }

    //TODO: Check if even nessesary for otho cameras
    private static void ModifyTerrainQuality()
    {
        int originalTerrainQuality = (int)Terrains[0].heightmapPixelError;

        for (int i = 0; i < Terrains.Length; i++)
        {
            Terrains[i].heightmapPixelError = 1;
        }
    }

    //Calculate the lowest and height positions across all terrains
    public static void GetTerrainMinMaxHeight(out float min, out float max)
    {
        min = 0;
        max = 0;

        for (int i = 0; i < Terrains.Length; i++)
        {
            Terrain t = Terrains[i];

            if (t.terrainData.size.y > max)
            {
                max = t.transform.position.y + t.terrainData.size.y;
            }
            if (t.transform.position.y < min)
            {
                min = t.transform.position.y;
            }
        }
    }

    // Check if folder exists, otherwise create it
    public static string CheckFolderValidity(string folder)
    {
        if (!Directory.Exists(folder))
        {
            Debug.Log("Directory \"" + folder + "\" didn't exist and was created...");
            Directory.CreateDirectory(folder);

            AssetDatabase.Refresh();
        }

        return folder;
    }

    public enum FileFormat
    {
        JPG,
        PNG,
        EXR
    }

    public static void SaveTexture(Texture2D t, FileFormat format = FileFormat.PNG)
    {
        if (!t)
        {
            Debug.LogError("Trying to save null texture");
            return;
        }

        CheckFolderValidity(SaveFolder);

        //Encode
        byte[] bytes = null;

        switch (format)
        {
            case FileFormat.PNG:
                bytes = t.EncodeToPNG();
                break;
            case FileFormat.JPG:
                bytes = t.EncodeToJPG();
                break;
            case FileFormat.EXR:
                bytes = t.EncodeToEXR();
                break;
            default:
                bytes = t.EncodeToPNG();
                break;

        }

        string savePath = SaveFolder + "/Terrain_" + t.name + "." + format.ToString();

        //Debug.Log("Saving to " + savePath);

        if (format == FileFormat.EXR)
        {
            FileStream fileSave = new FileStream(savePath, FileMode.Create);

            BinaryWriter binary = new BinaryWriter(fileSave);
            binary.Write(bytes);
            fileSave.Close();
        }
        else
        {
            //Create file
            File.WriteAllBytes(savePath, bytes);
        }

        //Import file
        AssetDatabase.Refresh();

        //Load the file
        //t = new Texture2D(t.width, t.height, TextureFormat.RGB24, true);
        //t = AssetDatabase.LoadAssetAtPath(savePath, typeof(Texture2D)) as Texture2D;

        //return t;
    }

    public class Styles
    {
        private static GUIStyle _PathField;
        public static GUIStyle PathField
        {
            get
            {
                if (_PathField == null)
                {
                    _PathField = new GUIStyle(GUI.skin.textField)
                    {
                        alignment = TextAnchor.MiddleRight,
                        stretchWidth = true
                    };
                }

                return _PathField;
            }
        }
    }
}
#endif