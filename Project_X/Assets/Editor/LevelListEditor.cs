using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelList))]
public class LevelListEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Pega a referência serializada do objeto
        serializedObject.Update();

        // Campo para o array "levels"
        SerializedProperty levels = serializedObject.FindProperty("levels");
        EditorGUILayout.PropertyField(levels, new GUIContent("Levels"), true);

        // Aplica mudanças
        serializedObject.ApplyModifiedProperties();
    }
}
