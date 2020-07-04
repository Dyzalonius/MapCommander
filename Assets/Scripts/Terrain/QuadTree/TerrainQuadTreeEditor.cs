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
                terrainQuadTree.filepathPrefixInEditor = EditorGUILayout.TextField("Filepath Prefix In Editor", terrainQuadTree.filepathPrefixInEditor);
                terrainQuadTree.filepathTerrainData = EditorGUILayout.TextField("Filepath Terrain Data", terrainQuadTree.filepathTerrainData);
                terrainQuadTree.mapFolderName = EditorGUILayout.TextField("Map Folder Name", terrainQuadTree.mapFolderName);
                break;

            case TerrainLoadType.GENERATE:
                terrainQuadTree.filepathPrefixInEditor = EditorGUILayout.TextField("Filepath Prefix In Editor", terrainQuadTree.filepathPrefixInEditor);
                terrainQuadTree.filepathTerrainData = EditorGUILayout.TextField("Filepath Terrain Data", terrainQuadTree.filepathTerrainData);
                if (GUILayout.Button("Generate"))
                    terrainQuadTree.BuildTree();
                break;

            default:
                break;
        }

    }
}
#endif