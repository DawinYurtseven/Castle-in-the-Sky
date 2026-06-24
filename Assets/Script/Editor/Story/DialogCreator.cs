using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogCreator : EditorWindow
{
    private Texture2D texture;
    private Actor currentActor; // Tracks the loaded asset
    private SerializedObject serializedActor;
    private VisualElement contentContainer;
    private int selectedDialogueIndex = -1;

    [MenuItem("Story/Dialog Creator")]
    public static void ShowWindow()
    {
        DialogCreator dc = GetWindow<DialogCreator>("Dialog Creator");
        dc.titleContent = new GUIContent("Dialog Creator");
    }

    public void CreateGUI()
    {
        var root = rootVisualElement;
        root.style.paddingBottom = 10;
        root.style.paddingTop = 10;
        root.style.paddingLeft = 10;
        root.style.paddingRight = 10;
        root.style.backgroundColor = new Color(0, 0, 0, 1);

        texture = GenerateGridTexture(32, new Color(0.18f, 0.18f, 0.18f), new Color(0.14f, 0.14f, 0.14f));

        // This is our main embedded window frame
        ScrollView embeddedWindow = new ScrollView
        {
            mode = ScrollViewMode.Vertical,
            style =
            {
                flexGrow = 1,
                borderBottomWidth = 1,
                borderTopWidth = 1,
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
                marginBottom = 10, // Reduced from 30% to fit both layout buttons nicely
                paddingBottom = 8,
                paddingTop = 8,
                paddingLeft = 8,
                paddingRight = 8,
                backgroundSize = new StyleBackgroundSize(new BackgroundSize(Length.Pixels(16), Length.Pixels(16))),
                backgroundImage = texture,
                backgroundRepeat = new StyleBackgroundRepeat(new BackgroundRepeat(Repeat.Repeat, Repeat.Repeat))
            }
        };
        root.Add(embeddedWindow);

        // Assign our content container reference to the inner scrolling area
        contentContainer = embeddedWindow;

        // --- BOTTOM BUTTON PANEL ---
        VisualElement buttonPanel = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.SpaceBetween
            }
        };

        var createActorButton = new Button(CreateNewActorAsset)
        {
            text = "Create New Actor",
            style =
            {
                flexGrow = 1,
                marginRight = 5,
                paddingBottom = 8,
                paddingTop = 8,
                paddingLeft = 8,
                paddingRight = 8,
                backgroundColor = new Color(0.2f, 0.2f, 0.2f)
            }
        };

        var showAllActorsButton = new Button(ListActors)
        {
            text = "Show All Actors",
            style =
            {
                flexGrow = 1,
                marginLeft = 5,
                paddingBottom = 8,
                paddingTop = 8,
                paddingLeft = 8,
                paddingRight = 8,
                backgroundColor = new Color(0.2f, 0.2f, 0.2f)
            }
        };

        buttonPanel.Add(createActorButton);
        buttonPanel.Add(showAllActorsButton);
        root.Add(buttonPanel);

        // Initial Load: Show the list of all project actors right away
        ListActors();
    }

    private void CreateNewActorAsset()
    {
        // Simple popup to ask the designer for a name
        string defaultName = "New Actor";
        // For a cleaner UI, you could build an inline text field, 
        // but a standard Save File Panel keeps asset management exceptionally safe:
        string fullPath =
            EditorUtility.SaveFilePanelInProject("Create New Actor Asset", defaultName, "asset", "Save Actor File");

        if (string.IsNullOrEmpty(fullPath)) return;

        string fileName = System.IO.Path.GetFileNameWithoutExtension(fullPath);

        Actor newActor = ScriptableObject.CreateInstance<Actor>();
        newActor.actorName = fileName;

        AssetDatabase.CreateAsset(newActor, fullPath);
        AssetDatabase.SaveAssets();

        // Instantly load the editor view for this brand new actor
        currentActor = newActor;
        serializedActor = new SerializedObject(currentActor);

        contentContainer.Clear();
        contentContainer.Add(ActorText());
    }

    private VisualElement ActorText()
    {
        // Update the serialized object data representation
        serializedActor.Update();

        var actorRoot = new VisualElement
        {
            style =
            {
                flexGrow = 1
            }
        };

        // --- SECTION 1: EDITABLE PROPERTIES ---
        var headerLabel = new Label($"{currentActor.name} Properties")
        {
            style =
            {
                unityFontStyleAndWeight = FontStyle.Bold,
                fontSize = 14,
                marginBottom = 10,
                color = Color.white
            }
        };
        actorRoot.Add(headerLabel);

        // Automatically creates correct fields for Name, CurrentProgress, and Sprite
        var actorNameProp = serializedActor.FindProperty("actorName");
        var progressProp = serializedActor.FindProperty("currentProgress");
        var spriteProp = serializedActor.FindProperty("defaultSprite");

        // Use Unity's automatic PropertyFields so things like Sprites open the object picker
        var nameField = new PropertyField(actorNameProp, "Actor Display Name");
        var progressField = new PropertyField(progressProp, "Current Progress");
        var spriteField = new PropertyField(spriteProp, "Default Sprite");

        // Bind them so modifications save to the asset file
        nameField.Bind(serializedActor);
        progressField.Bind(serializedActor);
        spriteField.Bind(serializedActor);

        actorRoot.Add(nameField);
        actorRoot.Add(progressField);
        actorRoot.Add(spriteField);

        // Spacer
        var spacer = new VisualElement
        {
            style =
            {
                height = 15
            }
        };
        actorRoot.Add(spacer);

        // --- SECTION 2: DIALOGUES SCROLL VIEW ---
        var dialogueLabel = new Label("Dialogues / Scenes")
        {
            style =
            {
                unityFontStyleAndWeight = FontStyle.Bold,
                color = Color.white
            }
        };
        actorRoot.Add(dialogueLabel);

        // This internal ScrollView handles scrolling if you add too many dialogues
        var dialogueScrollView = new ScrollView(ScrollViewMode.Vertical)
        {
            style =
            {
                maxHeight = 250, // Limits height so it forces scrolling inside the main window
                marginTop = 5,
                marginBottom = 10,
                backgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.6f),
                paddingBottom = 5,
                paddingLeft = 5,
                paddingRight = 5,
                paddingTop = 5
            }
        };
        actorRoot.Add(dialogueScrollView);

        // Find our list of Dialogues
        var scenesProp = serializedActor.FindProperty("scenes");

        // Populate list for the first time
        RefreshDialogueList();

        // --- SECTION 3: ADD DIALOGUE BUTTON ---
        Button addDialogueButton = new Button(() =>
        {
            serializedActor.Update();
            int newIndex = scenesProp.arraySize;
            scenesProp.InsertArrayElementAtIndex(newIndex);

            // Give it a temporary default name
            SerializedProperty newDialogue = scenesProp.GetArrayElementAtIndex(newIndex);
            newDialogue.FindPropertyRelative("dialogueName").stringValue = $"New Dialogue {newIndex}";

            serializedActor.ApplyModifiedProperties();

            // Refresh the visual display
            RefreshDialogueList(); // Open the chat layout view
        })
        {
            text = "+ Add Dialogue",
            style =
            {
                paddingTop = 6, paddingBottom = 6,
                backgroundColor = new Color(0.15f, 0.4f, 0.15f) // Subtle green theme for adding elements
            }
        };
        actorRoot.Add(addDialogueButton);

        // Track changes globally so typing updates the asset in real-time
        actorRoot.RegisterCallback<SerializedPropertyChangeEvent>(ApplyPropertiesCallback);

        return actorRoot;

        // Function to populate the scroll view with buttons for each dialogue
        void RefreshDialogueList()
        {
            dialogueScrollView.Clear();
            serializedActor.Update();

            if (scenesProp.arraySize == 0)
            {
                dialogueScrollView.Add(new Label("No dialogues added yet.") { style = { color = Color.gray } });
                return;
            }

            for (var i = 0; i < scenesProp.arraySize; i++)
            {
                var dialogueProp = scenesProp.GetArrayElementAtIndex(i);
                var nameProp = dialogueProp.FindPropertyRelative("dialogueName");

                // Fallback display if the user hasn't named the dialogue sequence yet
                var displayName = string.IsNullOrEmpty(nameProp.stringValue) ? $"Dialogue {i}" : nameProp.stringValue;
                var index = i; // Cache index for button interactions

                var dialogueButton = new Button(() =>
                {
                    Debug.Log($"Clicked Dialogue Index: {index} ({displayName})");
                    // Future step: Load this specific dialogue's flat sentence list!
                    selectedDialogueIndex = index;
                    LoadDialogueEditorView();
                })
                {
                    text = displayName,
                    style =
                    {
                        marginTop = 2,
                        marginBottom = 2,
                        paddingTop = 6,
                        paddingBottom = 6,
                        backgroundColor = new Color(0.25f, 0.25f, 0.25f)
                    }
                };
                dialogueScrollView.Add(dialogueButton);
            }
        }
    }

    private void LoadDialogueEditorView()
    {
        contentContainer.Clear();
        serializedActor.Update();

        // Grab the specific dialogue sequence we are editing
        var scenesProp = serializedActor.FindProperty("scenes");
        var currentDialogueProp = scenesProp.GetArrayElementAtIndex(selectedDialogueIndex);
        var sentencesProp = currentDialogueProp.FindPropertyRelative("sentences");
        var dialogueNameProp = currentDialogueProp.FindPropertyRelative("dialogueName");

        // --- HEADER ---
        Label headerLabel = new Label($"Editing Dialogue: {dialogueNameProp.stringValue}")
        {
            style =
            {
                unityFontStyleAndWeight = FontStyle.Bold,
                fontSize = 14,
                marginBottom = 10,
                color = Color.white
            }
        };
        contentContainer.Add(headerLabel);

        // --- CHAT TIMELINE CONTAINER ---
        ScrollView chatScrollView = new ScrollView(ScrollViewMode.Vertical)
        {
            style =
            {
                flexGrow = 1,
                marginBottom = 15,
                backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.5f),
                paddingBottom = 8,
                paddingTop = 8,
                paddingLeft = 8,
                paddingRight = 8
            }
        };
        contentContainer.Add(chatScrollView);

        RefreshSentenceList();

        // --- BOTTOM NAVIGATION BUTTON PANEL ---
        VisualElement controlPanel = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.SpaceBetween
            }
        };

        Button addSentenceButton = new Button(() =>
        {
            serializedActor.Update();
            var newIndex = sentencesProp.arraySize;
            sentencesProp.InsertArrayElementAtIndex(newIndex);

            // Standard auto-increment settings
            var newSentence = sentencesProp.GetArrayElementAtIndex(newIndex);
            newSentence.FindPropertyRelative("id").intValue = newIndex;
            newSentence.FindPropertyRelative("text").stringValue = "New regular dialogue line...";
            newSentence.FindPropertyRelative("leftImage").boolValue = true; // Default left alignment
            
            //add this sentence to previous sentence's choiceBranchIDs if it exists
            if (newIndex > 0)
            {
                var previousSentence = sentencesProp.GetArrayElementAtIndex(newIndex - 1);
                var choiceBranchIDsProp = previousSentence.FindPropertyRelative("choiceBranchIDs");
                choiceBranchIDsProp.arraySize++;
                choiceBranchIDsProp.GetArrayElementAtIndex(choiceBranchIDsProp.arraySize - 1    ).intValue = newIndex;
            }

            serializedActor.ApplyModifiedProperties();
            RefreshSentenceList();
        })
        {
            text = "Add New Sentence",
            style =
            {
                flexGrow = 1,
                marginRight = 4,
                paddingTop = 6,
                paddingBottom = 6,
                paddingRight = 6,
                paddingLeft = 6,
                backgroundColor = new Color(0.2f, 0.35f, 0.2f)
            }
        };

        var addChoiceButton = new Button(() =>
        {
            serializedActor.Update();
            var newIndex = sentencesProp.arraySize;
            sentencesProp.InsertArrayElementAtIndex(newIndex);

            var newSentence = sentencesProp.GetArrayElementAtIndex(newIndex);
            newSentence.FindPropertyRelative("id").intValue = newIndex;
            newSentence.FindPropertyRelative("text").stringValue = "Branching choice prompt question...";
            newSentence.FindPropertyRelative("leftImage").boolValue = true;

            // Note: For choice handling, you can use your choiceBranchIDs array here later

            serializedActor.ApplyModifiedProperties();
            RefreshSentenceList();
        })
        {
            text = "Add Multiple Choice",
            style =
            {
                flexGrow = 1,
                marginRight = 4,
                marginLeft = 4,
                paddingTop = 6,
                paddingBottom = 6,
                paddingRight = 6,
                paddingLeft = 6, backgroundColor = new Color(0.35f, 0.25f, 0.15f)
            }
        };

        Button backButton = new Button(() =>
        {
            // Save state and return to the primary actor screen layout
            selectedDialogueIndex = -1;
            contentContainer.Clear();
            contentContainer.Add(ActorText());
        })
        {
            text = "Back",
            style =
            {
                flexGrow = 1,
                marginLeft = 4,
                paddingTop = 6,
                paddingBottom = 6,
                paddingRight = 6,
                paddingLeft = 6,
                backgroundColor = new Color(0.35f, 0.2f, 0.2f)
            }
        };

        controlPanel.Add(addSentenceButton);
        controlPanel.Add(addChoiceButton);
        controlPanel.Add(backButton);
        contentContainer.Add(controlPanel);

        // Track data updates
        contentContainer.RegisterCallback<SerializedPropertyChangeEvent>(ApplyPropertiesCallback);
        return;

        // Local function to draw the list of sentences
        void RefreshSentenceList()
        {
            chatScrollView.Clear();
            serializedActor.Update();

            if (sentencesProp.arraySize == 0)
            {
                chatScrollView.Add(new Label("No sentences in this dialogue yet. Add one below!")
                    { style = { color = Color.gray, marginTop = 10, unityTextAlign = TextAnchor.MiddleCenter } });
                return;
            }

            for (var i = 0; i < sentencesProp.arraySize; i++)
            {
                var sentenceProp = sentencesProp.GetArrayElementAtIndex(i);
                var textProp = sentenceProp.FindPropertyRelative("text");
                var leftImageProp = sentenceProp.FindPropertyRelative("leftImage");
                var index = i;

                // Create a wrapper box representing a chat message bubble row
                var messageRow = new VisualElement
                {
                    style =
                    {
                        marginBottom = 8,
                        flexDirection = FlexDirection.Row,
                        // --- CHAT ALIGNMENT LOGIC ---
                        // Based on the 'leftImage' bool, shift the bubble to the left or right side of the screen
                        justifyContent = leftImageProp.boolValue
                            ? Justify.FlexStart
                            : // Align Left
                            Justify.FlexEnd
                    }
                };

                // Align Right

                // The actual bubble container holding properties
                var bubble = new VisualElement
                {
                    style =
                    {
                        width = Length.Percent(70), // Chat bubbles usually take up most but not all width
                        backgroundColor = leftImageProp.boolValue
                            ? new Color(0.2f, 0.25f, 0.3f)
                            : new Color(0.25f, 0.3f, 0.2f),
                        paddingTop = 6,
                        paddingBottom = 6,
                        paddingLeft = 6,
                        paddingRight = 6,
                        borderBottomLeftRadius = 4,
                        borderBottomRightRadius = 4,
                        borderTopLeftRadius = 4,
                        borderTopRightRadius = 4,
                    }
                };

                // Mini top row inside the bubble for quick settings and deletion
                var bubbleHeader = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        justifyContent = Justify.SpaceBetween,
                        alignItems = Align.Center, // Align items vertically in the middle
                        marginBottom = 4
                    }
                };

                // Left side of the header: ID and Side Toggle
                var headerLeftGroup = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        alignItems = Align.Center
                    }
                };

                var idLabel = new Label($"ID: {sentenceProp.FindPropertyRelative("id").intValue}  ")
                {
                    style =
                    {
                        unityFontStyleAndWeight = FontStyle.Bold,
                        color = Color.white
                    }
                };
                headerLeftGroup.Add(idLabel);

                var sideToggleButton = new Button(() =>
                {
                    leftImageProp.boolValue = !leftImageProp.boolValue;
                    serializedActor.ApplyModifiedProperties();
                    RefreshSentenceList();
                })
                {
                    text = leftImageProp.boolValue ? "◀ Left Side" : "Right Side ▶",
                    style =
                    {
                        fontSize = 10, 
                        paddingBottom = 2,
                        paddingTop = 2,
                        paddingRight =  2,
                        paddingLeft = 2,
                    }
                };
                headerLeftGroup.Add(sideToggleButton);
                bubbleHeader.Add(headerLeftGroup);

                // Right side of the header: The "X" Delete Button
                var deleteButton = new Button(() =>
                {
                    serializedActor.Update();

                    var oldSize = sentencesProp.arraySize;
                    sentencesProp.DeleteArrayElementAtIndex(index);

                    // If Unity cleared the content instead of removing the index slot, delete it again
                    if (sentencesProp.arraySize == oldSize)
                    {
                        sentencesProp.DeleteArrayElementAtIndex(index);
                    }

                    // Optional Best Practice: Clean up IDs so they remain sequential (0, 1, 2...) after a deletion
                    for (var k = 0; k < sentencesProp.arraySize; k++)
                    {
                        sentencesProp.GetArrayElementAtIndex(k).FindPropertyRelative("id").intValue = k;
                    }

                    serializedActor.ApplyModifiedProperties();
                    RefreshSentenceList(); // Redraw everything cleanly
                })
                {
                    text = "X",
                    style =
                    {
                        unityFontStyleAndWeight = FontStyle.Bold,
                        backgroundColor = new Color(0.6f, 0.15f, 0.15f), // Red alert styling
                        color = Color.white,
                        paddingLeft = 6, paddingRight = 6,
                        paddingTop = 2, paddingBottom = 2,
                        borderTopLeftRadius = 2, borderTopRightRadius = 2,
                        borderBottomLeftRadius = 2, borderBottomRightRadius = 2
                    }
                };
                bubbleHeader.Add(deleteButton);

                // Add the completed header to the bubble
                bubble.Add(bubbleHeader);

                // The editable Text Area for the dialogue phrase
                var textField = new PropertyField(textProp, "Text");
                textField.Bind(serializedActor);
                bubble.Add(textField);

                // Add fields for Actor, Audio, Sprite data containers
                var speakerField = new PropertyField(sentenceProp.FindPropertyRelative("actor"), "Speaker");
                var audioField = new PropertyField(sentenceProp.FindPropertyRelative("audio"), "Audio Clip");
                var spriteField = new PropertyField(sentenceProp.FindPropertyRelative("actorSprite"), "Sprite");

                speakerField.Bind(serializedActor);
                audioField.Bind(serializedActor);
                spriteField.Bind(serializedActor);

                bubble.Add(speakerField);
                bubble.Add(audioField);
                bubble.Add(spriteField);

                messageRow.Add(bubble);
                chatScrollView.Add(messageRow);
            }
        }
    }

    private void ListActors()
    {
        contentContainer.Clear();
        currentActor = null;
        serializedActor = null;

        var titleLabel = new Label("Select an Actor to Edit")
        {
            style =
            {
                unityFontStyleAndWeight = FontStyle.Bold,
                fontSize = 14,
                marginBottom = 10,
                color = Color.white
            }
        };
        contentContainer.Add(titleLabel);

        // Find all assets in the project with the type 'Actor'
        var guids = AssetDatabase.FindAssets("t:Actor");

        if (guids.Length == 0)
        {
            contentContainer.Add(new Label("No Actors found in the project. Use 'Create New Actor' below.")
            {
                style =
                {
                    color = Color.gray,
                    marginTop = 10
                }
            });
            return;
        }

        foreach (var guid in guids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var actorAsset = AssetDatabase.LoadAssetAtPath<Actor>(assetPath);

            if (!actorAsset) continue;
            // Use the asset name or the assigned internal character name
            var displayLabel = string.IsNullOrEmpty(actorAsset.actorName) ? actorAsset.name : actorAsset.actorName;

            var actorButton = new Button(() =>
            {
                // Assign and load this actor into the properties editor view!
                currentActor = actorAsset;
                serializedActor = new SerializedObject(currentActor);

                contentContainer.Clear();
                contentContainer.Add(ActorText());
            })
            {
                text = displayLabel,
                style =
                {
                    marginTop = 4,
                    marginBottom = 4,
                    paddingTop = 8, paddingBottom = 8,
                    backgroundColor = new Color(0.25f, 0.25f, 0.35f), // Slight blue tint for selection items
                    unityTextAlign = TextAnchor.MiddleLeft
                }
            };
            contentContainer.Add(actorButton);
        }
    }

    private static Texture2D GenerateGridTexture(int size, Color bgCellColor, Color gridLineColor)
    {
        var tex = new Texture2D(size, size)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Repeat // Explicitly tell the texture asset to allow repeating
        };

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
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
        if (texture)
        {
            DestroyImmediate(texture);
        }
    }

    private void ApplyPropertiesCallback(SerializedPropertyChangeEvent evt)
    {
        serializedActor.ApplyModifiedProperties();
    }
}