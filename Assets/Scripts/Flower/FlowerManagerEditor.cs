using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FlowerManager))]
public class FlowerManagerEditor : Editor {
    public override void OnInspectorGUI() {

        DrawDefaultInspector();

        FlowerManager flowers = (FlowerManager)target;

        if (GUILayout.Button("Cycle Displayed Grid"))
            flowers.CycleDisplayedGrid();

        if (GUILayout.Button("Match Displayed Grid To Map Bounds"))
            flowers.MatchGridToMap();


    }
}