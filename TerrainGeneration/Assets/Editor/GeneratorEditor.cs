using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(TerrainGenerator))]
public class GeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        TerrainGenerator genScript = (TerrainGenerator) target;
        if (GUILayout.Button("Generate"))
        {
            genScript.Start();
        }
    }
}
