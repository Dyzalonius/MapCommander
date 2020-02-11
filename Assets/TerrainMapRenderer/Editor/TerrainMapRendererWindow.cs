using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Editor = TerrainMapRendererUtilities;
using Renderer = TerrainMapRenderer;

public class TerrainMapRendererWindow : EditorWindow
{
    public static void Init()
    {
        EditorWindow window = EditorWindow.GetWindow(typeof(TerrainMapRendererWindow), false);

        //Options
        window.autoRepaintOnSceneChange = true;
        window.maxSize = new Vector2(900f, 610f);
        window.minSize = window.maxSize;
        window.titleContent.image = EditorGUIUtility.IconContent("camera gizmo.png").image;
        window.titleContent.text = "Terrain Map Renderer";

        Editor.RefreshTerrains();

        //Show
        window.Show();

        TerrainMapRendererEditor.Align();
    }

    private void OnDisable()
    {
        //DestroyImmediate(Renderer.Instance.gameObject);
    }

    Vector2 terrainScrollPos;

    void OnGUI()
    {
        Rect leftRect = new Rect(5, 5, 350f, 600f);
        Rect rightRect = new Rect(360f, 5, 510f, 600f);

        GUILayout.BeginArea(leftRect, EditorStyles.helpBox);
        {
            DrawOptions();
        }
        GUILayout.EndArea();
        GUILayout.BeginArea(rightRect, EditorStyles.helpBox);
        {
            DrawViewport();
        }
        GUILayout.EndArea();
    }

    void DrawOptions()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Terrains (" + Editor.Terrains.Length + ")", EditorStyles.boldLabel);
                if (GUILayout.Button("Refresh", GUILayout.MaxWidth(75f)))
                {
                    TerrainMapRendererEditor.Align();
                }
            }

            terrainScrollPos = EditorGUILayout.BeginScrollView(terrainScrollPos, EditorStyles.textArea, GUILayout.Height(100f));
            {
                foreach (var t in Editor.Terrains)
                {
                    DrawTerrainInto(t);
                }

            }
            EditorGUILayout.EndScrollView();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            EditorGUILayout.LabelField("Maps", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.textArea))
            {
                TerrainMapRenderer.Instance.renderAlbedo = DrawMapField("Albedo", TerrainMapRenderer.Instance.renderAlbedo, TerrainMapRendererEditor.Map.Albedo);
                TerrainMapRenderer.Instance.renderHeight = DrawMapField("Height", TerrainMapRenderer.Instance.renderHeight, TerrainMapRendererEditor.Map.Height);
                TerrainMapRenderer.Instance.renderNormals = DrawMapField("Normal", TerrainMapRenderer.Instance.renderNormals, TerrainMapRendererEditor.Map.Normal);
                TerrainMapRenderer.Instance.renderAmbientOcclusion = DrawMapField("Ambient Occlusion", TerrainMapRenderer.Instance.renderAmbientOcclusion, TerrainMapRendererEditor.Map.AmbientOcclusion);
                TerrainMapRenderer.Instance.renderCurvature = DrawMapField("Curvature", TerrainMapRenderer.Instance.renderCurvature, TerrainMapRendererEditor.Map.Curvature);
                TerrainMapRenderer.Instance.renderFlow = DrawMapField("Flow", TerrainMapRenderer.Instance.renderFlow, TerrainMapRendererEditor.Map.Flow);
                TerrainMapRenderer.Instance.renderHillshade = DrawMapField("Hillshade", TerrainMapRenderer.Instance.renderHillshade, TerrainMapRendererEditor.Map.Hillshade);
                TerrainMapRenderer.Instance.renderContour = DrawMapField("Contour", TerrainMapRenderer.Instance.renderContour, TerrainMapRendererEditor.Map.Contour);
                TerrainMapRenderer.Instance.renderShoreline = DrawMapField("Shoreline", TerrainMapRenderer.Instance.renderShoreline, TerrainMapRendererEditor.Map.Shoreline);
                TerrainMapRenderer.Instance.renderSlope = DrawMapField("Slope", TerrainMapRenderer.Instance.renderSlope, TerrainMapRendererEditor.Map.Slope);
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            GUILayout.Label("Output folder", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField(TerrainMapRenderer.Instance.outputFolder, Editor.Styles.PathField);

            if (GUILayout.Button("...", GUILayout.ExpandWidth(true)))
            {
                TerrainMapRenderer.Instance.outputFolder = EditorUtility.SaveFolderPanel("Output folder", TerrainMapRenderer.Instance.outputFolder, "Assets/");
            }
            if (GUILayout.Button("Open", GUILayout.ExpandWidth(true)))
            {
                Application.OpenURL(TerrainMapRenderer.Instance.outputFolder);
            }
            EditorGUILayout.EndHorizontal();

            TerrainMapRendererUtilities.SaveFolder = TerrainMapRenderer.Instance.outputFolder;

        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            GUILayout.Label("Options", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("No options available (not yet implemented)");
            EditorGUILayout.Slider("Slider", 0f, 0f, 1f);
            EditorGUILayout.Toggle("Toggle", false);

        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();

            EditorGUI.BeginDisabledGroup(Editor.SaveFolder == null);
            {
                if (GUILayout.Button("Save all", GUILayout.MinHeight(25), GUILayout.MinWidth(100f)))
                {
                    TerrainMapRendererEditor.SaveTextures();

                    /*
                    foreach (Renderer.Map item in (Renderer.Map[])Renderer.Map.GetValues(typeof(Renderer.Map)))
                    {
                        Renderer.Instance.ExecuteRender(item);
                    }
                    */
                }
            }
            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.EndHorizontal();
    }

    Rect previewRext;
    void DrawViewport()
    {
        if (TerrainMapRenderer.Instance.previewTex) EditorGUILayout.LabelField(TerrainMapRenderer.Instance.previewTex.name, Header);

        EditorGUILayout.Space();

        previewRext = EditorGUILayout.GetControlRect();
        previewRext.width = 500f;
        previewRext.height = 500f;

        if (TerrainMapRenderer.Instance.previewTex)
        {
            EditorGUI.DrawPreviewTexture(previewRext, TerrainMapRenderer.Instance.previewTex, null, ScaleMode.ScaleToFit);
        }
        else
        {
            EditorGUI.DrawPreviewTexture(previewRext, Texture2D.blackTexture, null, ScaleMode.ScaleToFit);
        }
        GUILayout.Space(previewRext.height);

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(Editor.SaveFolder == null);
            {
                if (GUILayout.Button("Save", GUILayout.MinHeight(25), GUILayout.MaxWidth(100f)))
                {
                    TerrainMapRendererUtilities.SaveTexture(TerrainMapRenderer.Instance.previewTex);
                }
            }
            EditorGUI.EndDisabledGroup();

        }
        EditorGUILayout.EndHorizontal();

    }

    private void DrawTerrainInto(Terrain t)
    {
        if (!t) return;

        string size = t.terrainData.size.ToString();
        EditorGUILayout.LabelField(t.name + " " + size);
    }

    private bool DrawMapField(string label, bool value, TerrainMapRendererEditor.Map map = TerrainMapRendererEditor.Map.Albedo)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            value = EditorGUILayout.Toggle(value, GUILayout.MaxWidth(15f));

            using (new EditorGUI.DisabledGroupScope(!value))
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(150f));

                EditorGUILayout.Popup(0, new[] { "1024 px" }, GUILayout.Width(50f));

                if (GUILayout.Button("Preview", GUILayout.Width(100f)))
                {
                    TerrainMapRendererEditor.Preview(map);
                }
            }
        }

        return value;
    }

    private static GUIStyle _Header;
    public static GUIStyle Header
    {
        get
        {
            if (_Header == null)
            {
                _Header = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    stretchWidth = true,
                    richText = true,
                    wordWrap = true,
                    fontSize = 24,
                    fontStyle = FontStyle.Normal,
                };
            }

            return _Header;
        }
    }
}
