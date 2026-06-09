using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogCreator : EditorWindow
{
    
    private Texture2D texture;
    
    [MenuItem("Story/Dialog Creator")]
    public static void ShowWindow()
    {
        DialogCreator dc = GetWindow<DialogCreator>("Dialog Creator");
        dc.titleContent = new GUIContent("Dialog Creator");
    }

    public void CreateGUI()
    {
        var root = rootVisualElement;
        root.style.paddingTop = 10;
        root.style.paddingBottom = 10;
        root.style.paddingLeft = 10;
        root.style.paddingRight = 10;
        root.style.backgroundColor = new Color(0, 0, 0, 1);
        
        // --- ANIMATOR GRID TEXTURE BACKGROUND ---
        texture = GenerateGridTexture(32, new Color(0.18f, 0.18f, 0.18f), new Color(0.14f, 0.14f, 0.14f));
        
        
        VisualElement embeddedWindow = new ScrollView
        {
            mode = ScrollViewMode.Vertical,
            style =
            {
                flexGrow = 1,
                borderTopWidth = 1,
                borderBottomWidth = 1,
                borderLeftWidth = 1,
                borderRightWidth = 1,
                borderBottomColor = new Color(0.5f, 0.5f, 0.5f, 1),
                borderTopColor = new Color(0.5f, 0.5f, 0.5f, 1),
                borderLeftColor = new Color(0.5f, 0.5f, 0.5f, 1),
                borderRightColor = new Color(0.5f, 0.5f, 0.5f, 1),
                borderBottomLeftRadius = 2,
                borderBottomRightRadius = 2,
                borderTopLeftRadius = 2,
                borderTopRightRadius = 2,
                marginBottom = Length.Percent(30),
                paddingBottom = 8,
                paddingTop = 8,
                paddingLeft = 8,
                paddingRight = 8,
                backgroundSize = new StyleBackgroundSize(new BackgroundSize(Length.Pixels(16),Length.Pixels(16))),
                backgroundImage = texture,
                backgroundRepeat =  new StyleBackgroundRepeat(new BackgroundRepeat(Repeat.Repeat, Repeat.Repeat))
            }
        };
        
        
        root.Add(embeddedWindow);

        var addDialogueButton = new Button
        {
            style =
            {
                flexShrink = 1,
                backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1),
                borderTopWidth = 1,
                borderBottomWidth = 1,
                borderLeftWidth = 1,
                borderRightWidth = 1,
                borderBottomColor = new Color(0.5f, 0.5f, 0.5f, 1),
                borderTopColor = new Color(0.5f, 0.5f, 0.5f, 1),
                borderLeftColor = new Color(0.5f, 0.5f, 0.5f, 1),
                borderRightColor = new Color(0.5f, 0.5f, 0.5f, 1),
                borderBottomLeftRadius = 2,
                borderBottomRightRadius = 2,
                borderTopLeftRadius = 2,
                borderTopRightRadius = 2,
                paddingBottom = 8,
                paddingTop = 8,
                paddingLeft = 8,
                paddingRight = 8
            },
            text = "Add Dialogue",
            clickable = new Clickable(() =>
            {
                embeddedWindow.Add(ActorText());
            })
        };
        
        root.Add(addDialogueButton);
    }

    private static VisualElement ActorText()
    {
        var root = new VisualElement();
        root.Add(new TextField("Actor"));
        root.Add(new TextField("Dialogue"));
        
        return root;
    }
    
    private Texture2D GenerateGridTexture(int size, Color bgCellColor, Color gridLineColor)
    {
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Repeat; // Explicitly tell the texture asset to allow repeating

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Draw lines on BOTH sides (0 and size-1) to ensure seamless stitching
                if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
                {
                    tex.SetPixel(x, y, gridLineColor);
                }
                else
                {
                    tex.SetPixel(x, y, bgCellColor);
                }
            }
        }

        tex.Apply();
        return tex;
    }
    
    private void OnDestroy()
    {
        // Clean up the procedural texture from memory when window closes
        if (texture != null)
        {
            DestroyImmediate(texture);
        }
    }
}
