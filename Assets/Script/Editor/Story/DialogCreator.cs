using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogCreator : EditorWindow
{
    
    [MenuItem("Story/Dialog Creator")]
    public static void ShowWindow()
    {
        DialogCreator dc = GetWindow<DialogCreator>("Dialog Creator");
        dc.titleContent = new GUIContent("Dialog Creator");
    }

    public void CreateGUI()
    {
        var root = rootVisualElement;
        root.Add(new Label("Dialog Creator"));
    }
}
