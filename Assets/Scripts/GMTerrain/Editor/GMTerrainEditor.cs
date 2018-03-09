using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GMTerrain))]
public class GMTerrainEditor : Editor
{

    private GMTerrain m_Target;

    void OnEnable()
    {
        m_Target = (GMTerrain) target;
        
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
