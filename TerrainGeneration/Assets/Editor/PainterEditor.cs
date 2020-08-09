using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(Painter))]
public class PainterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Painter genScript = (Painter) target;
        if (GUILayout.Button("Paint"))
        {
            genScript.Paint();
        }
    }
}