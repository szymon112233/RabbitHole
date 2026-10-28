// https://answers.unity.com/questions/1328682/inspector-debug-mode-from-code.html
// https://blog.fireheart.in/a?ID=01700-208c3332-c023-4fbe-9cea-7e60c427d442

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
public class InspectorShortcuts
{
    [MenuItem("Tools/Toggle Inspector Mode %#d")]
    static void ToggleInspectorDebug()
    {
        var type = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");    
        FieldInfo field = type.GetField("m_InspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
        var inspectorWindow = EditorWindow.GetWindow(type);

        InspectorMode mode = (InspectorMode)field.GetValue(inspectorWindow);                                    
        mode = (mode == InspectorMode.Normal ? InspectorMode.Debug : InspectorMode.Normal);                    

        MethodInfo method = type.GetMethod("SetMode", BindingFlags.NonPublic | BindingFlags.Instance);          
        method.Invoke(inspectorWindow, new object[] { mode });
    }

    [MenuItem("Tools/Toggle Inspector Lock %#l")]
    static public void ToggleInspectorLock()
    {
        var inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");

        var isLocked = inspectorType.GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Public);

        var inspectorWindow = EditorWindow.GetWindow(inspectorType);

        var state = isLocked.GetGetMethod().Invoke(inspectorWindow, new object[] { });

        isLocked.GetSetMethod().Invoke(inspectorWindow, new object[] { !(bool)state });
    }
}



