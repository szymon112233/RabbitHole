using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GrassRenderer))]
public class GrassRendererEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var grass = (GrassRenderer)target;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Blades scattered", grass.BladeCount.ToString("N0"));
        EditorGUILayout.HelpBox("Blades are placed by raycasting onto this object's colliders, so swapping the plane for a terrain or any mesh with a collider just works. Draw cost is a couple of indirect draw calls per camera.", MessageType.Info);
        if (GUILayout.Button("Rebuild Grass"))
        {
            grass.Rebuild();
            SceneView.RepaintAll();
        }
    }
}
