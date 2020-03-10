using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
