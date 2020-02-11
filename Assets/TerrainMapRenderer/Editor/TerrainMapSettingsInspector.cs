using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainMapRenderer))]
public class TerrainMapSettingsInspector : Editor {
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Open window")) TerrainMapRendererWindow.Init();

        base.OnInspectorGUI();
    }
}
