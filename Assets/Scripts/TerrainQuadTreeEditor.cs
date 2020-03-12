using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainQuadTree))]
public class TerrainQuadTreeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TerrainQuadTree terrainQuadTree = (TerrainQuadTree)target;

        DrawDefaultInspector();

        switch (terrainQuadTree.loadType)
        {
            case TerrainLoadType.LOAD:
                terrainQuadTree.terrainDataFilepath = EditorGUILayout.TextField("Terrain Data Filepath", terrainQuadTree.terrainDataFilepath);
                terrainQuadTree.mapFolderName = EditorGUILayout.TextField("Map Folder Name", terrainQuadTree.mapFolderName);
                break;

            case TerrainLoadType.GENERATE:
                if (GUILayout.Button("Generate"))
                    terrainQuadTree.BuildTree();
                break;

            default:
                break;
        }

    }
}
#endif