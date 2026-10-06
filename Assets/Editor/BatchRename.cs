using System.Linq;
using UnityEditor;
using UnityEngine;

// Place this file inside a folder named "Editor" (e.g. Assets/Editor/BatchRename.cs)
// Open via: Tools > Batch Rename
public class BatchRename : EditorWindow
{
    string baseName = "object_";
    int startNumber = 1;
    int digits = 3;

    [MenuItem("Tools/Batch Rename")]
    static void Open()
    {
        GetWindow<BatchRename>("Batch Rename");
    }

    void OnGUI()
    {
        baseName = EditorGUILayout.TextField("Base Name", baseName);
        startNumber = EditorGUILayout.IntField("Start Number", startNumber);
        digits = EditorGUILayout.IntSlider("Digits", digits, 1, 6);

        int count = Selection.gameObjects.Length;
        EditorGUILayout.HelpBox(
            $"Selected: {count} objects\nPreview: {baseName}{startNumber.ToString().PadLeft(digits, '0')}",
            MessageType.Info);

        GUI.enabled = count > 0;
        if (GUILayout.Button("Rename Selected"))
            Rename();
        GUI.enabled = true;
    }

    void OnSelectionChange()
    {
        Repaint();
    }

    void Rename()
    {
        // Sort by order in the Hierarchy (top to bottom)
        var objects = Selection.gameObjects
            .OrderBy(go => go.transform.parent ? go.transform.parent.GetSiblingIndex() : -1)
            .ThenBy(go => go.transform.GetSiblingIndex())
            .ToArray();

        Undo.RecordObjects(objects, "Batch Rename");

        for (int i = 0; i < objects.Length; i++)
        {
            string number = (startNumber + i).ToString().PadLeft(digits, '0');
            objects[i].name = baseName + number;
        }
    }
}
