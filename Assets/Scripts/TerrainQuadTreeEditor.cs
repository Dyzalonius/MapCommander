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

        if (DrawDefaultInspector())
        {
            //terrainQuadTree.BuildTree();
        }

        if (GUILayout.Button("Generate"))
        {
            terrainQuadTree.BuildTree();
        }
    }
}
