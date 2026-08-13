using System.Collections.Generic;
using System.Linq;
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
    private VisualElement buttonPanel;
    private int selectedDialogueIndex = -1;

    [MenuItem("Tools/Story/Dialog Creator")]
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
        buttonPanel = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.SpaceBetween
            }
        };

        ResetButtonPanel();
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

    private void ResetButtonPanel()
    {
        buttonPanel.Clear();
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
        buttonPanel.Add(createActorButton);
    }
    
    private bool filler;
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

        // change the whole name of the actor asset when the display name is changed
        nameField.RegisterCallback<GeometryChangedEvent>(_ => 
        {
            // 2. Query inside the PropertyField to find the actual underlying TextField
            TextField actualTextField = nameField.Q<TextField>();

            if (actualTextField != null)
            {
                // 3. Unregister standard callbacks and use FocusOutEvent instead!
                actualTextField.RegisterCallback<FocusOutEvent>(_ =>
                {
                    var newName = actualTextField.value;
                    if (!currentActor || string.IsNullOrEmpty(newName)) return;

                    var assetPath = AssetDatabase.GetAssetPath(currentActor);

                    if (string.IsNullOrEmpty(assetPath)) return;
                    var result = AssetDatabase.RenameAsset(assetPath, newName.Trim());

                    if (!string.IsNullOrEmpty(result))
                    {
                        Debug.LogWarning($"Failed to rename asset: {result}");
                    }
                    else
                    {
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }

                    serializedActor.ApplyModifiedProperties();
                });
            }
        });

        
        

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

        // Find our list of Dialogues
        var scenesProp = serializedActor.FindProperty("scenes");
        var fillerScenesProp = serializedActor.FindProperty("fillerScenes");

        var buttonsContainer = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.SpaceBetween
            }
        };

        var normalScenes = new Button(() =>
        {
            filler = false;
            RefreshDialogueList();
        })
        {
            text = "Normal Scenes",
            style =
            {
                flexGrow = 1,
                marginRight = 5,
                paddingTop = 6,
                paddingBottom = 6,
                backgroundColor = new Color(0.2f, 0.2f, 0.2f)
            }
        };
        var fillerScenes = new Button(() =>
        {
            filler = true;
            RefreshDialogueList();
        })
        {
            text = "Filler Scenes",
            style =
            {
                flexGrow = 1,
                marginLeft = 5,
                paddingTop = 6,
                paddingBottom = 6,
                backgroundColor = new Color(0.2f, 0.2f, 0.2f)
            }
        };

        buttonsContainer.Add(normalScenes);
        buttonsContainer.Add(fillerScenes);
        actorRoot.Add(buttonsContainer);
        actorRoot.Add(dialogueScrollView);

        // Populate list for the first time
        RefreshDialogueList();

        // --- SECTION 3: ADD DIALOGUE BUTTON ---
        Button addDialogueButton = new Button(() =>
        {
            serializedActor.Update();
            var activeScenesProp = filler ? fillerScenesProp : scenesProp;
            int newIndex = activeScenesProp.arraySize;
            activeScenesProp.InsertArrayElementAtIndex(newIndex);

            // Give it a temporary default name
            var newDialogue = activeScenesProp.GetArrayElementAtIndex(newIndex);
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
        
        //change the buttons at the bottom
        buttonPanel.Clear();
        var backToStart = new Button(() =>
        {
            ResetButtonPanel();
            ListActors();
        })
        {
            text = "Back to Actor List",
            style =
            {
                flexGrow = 1,
                marginRight = 5,
                paddingBottom = 8,
                paddingTop = 8,
                paddingLeft = 8,
                paddingRight = 8,
                backgroundColor = new Color(0.4f, 0.2f, 0.2f)
            }
        };

        buttonPanel.Add(backToStart);

        return actorRoot;

        
        // Function to populate the scroll view with buttons for each dialogue
        void RefreshDialogueList()
        {
            dialogueScrollView.Clear();
            serializedActor.Update();
            
            var activeScenesProp = filler ? fillerScenesProp : scenesProp;

            if (activeScenesProp.arraySize == 0)
            {
                dialogueScrollView.Add(new Label("No dialogues added yet.") { style = { color = Color.gray } });
                return;
            }

            for (var i = 0; i < activeScenesProp.arraySize; i++)
            {
                var dialogueProp = activeScenesProp.GetArrayElementAtIndex(i);
                var nameProp = dialogueProp.FindPropertyRelative("dialogueName");

                // Fallback display if the user hasn't named the dialogue sequence yet
                var displayName = string.IsNullOrEmpty(nameProp.stringValue) ? $"Dialogue {i}" : nameProp.stringValue;
                var index = i; // Cache index for button interactions

                var dialogueButton = new Button(() =>
                {
                    // Future step: Load this specific dialogue's flat sentence list!
                    selectedDialogueIndex = index;
                    currentDialogue = dialogueProp;
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
                        backgroundColor = new Color(0.25f, 0.25f, 0.25f),
                        flexDirection = FlexDirection.Row,
                        justifyContent = Justify.SpaceBetween
                    }
                };
                var deleteButton = new Button(() =>
                {
                    serializedActor.Update();
                    activeScenesProp.DeleteArrayElementAtIndex(index);
                    serializedActor.ApplyModifiedProperties();
                    RefreshDialogueList(); // Redraw the list after deletion
                })
                {
                    text = "Delete",
                    style =
                    {
                        marginLeft = 5,
                        backgroundColor = new Color(0.6f, 0.1f, 0.1f)
                    }
                };
                dialogueButton.Add(deleteButton);
                dialogueScrollView.Add(dialogueButton);
                
            }
        }
    }

    private SerializedProperty currentDialogue;
    private SerializedProperty currentSentence; // Tracks the currently selected sentence for editing
    private readonly List<int> selectedChoices = new ();

    private void LoadDialogueEditorView()
    {
        currentSentence = null;
        selectedChoices.Clear();
        contentContainer.Clear();
        serializedActor.Update();

        // Grab the specific dialogue sequence we are editing
        var currentDialogueProp = currentDialogue;
        var sentencesProp = currentDialogueProp.FindPropertyRelative("sentences");
        var dialogueNameProp = currentDialogueProp.FindPropertyRelative("dialogueName");

        // --- HEADER ---
        var dialogueNameField = new TextField("Dialogue Name")
        {
            value = dialogueNameProp.stringValue,
            style =
            {
                marginBottom = 10,
                fontSize = 14,
                unityFontStyleAndWeight = FontStyle.Bold,
                color = Color.white,
            }
        };

        dialogueNameField.BindProperty(dialogueNameProp);
        dialogueNameField.RegisterValueChangedCallback(_ =>
        {
            serializedActor.ApplyModifiedProperties();
        });

        contentContainer.Add(dialogueNameField);
        
        // add a condition field for the dialogue?
        
        // --- CONDITIONS EDITOR ---
    var conditionsProp = currentDialogueProp.FindPropertyRelative("conditions");
    if (conditionsProp != null)
    {
        var conditionsBox = new VisualElement
        {
            style =
            {
                marginBottom = 12,
                paddingBottom = 8,
                paddingTop = 4,
                paddingLeft = 8,
                paddingRight = 8,
                backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.6f),
                borderBottomWidth = 1,
                borderTopWidth = 1,
                borderLeftWidth = 1,
                borderRightWidth = 1,
                borderBottomColor = new Color(0.25f, 0.25f, 0.25f),
                borderTopColor = new Color(0.25f, 0.25f, 0.25f),
                borderLeftColor = new Color(0.25f, 0.25f, 0.25f),
                borderRightColor = new Color(0.25f, 0.25f, 0.25f),
            }
        };

        // Header Row with Title and Add Button
        var condHeader = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, marginBottom = 6 } };
        var condTitle = new Label("Required Conditions to Play Scene") { style = { unityFontStyleAndWeight = FontStyle.Bold, color = Color.white, alignSelf = Align.Center } };
        
        var addConditionBtn = new Button(() =>
        {
            serializedActor.Update();
            int index = conditionsProp.arraySize;
            conditionsProp.InsertArrayElementAtIndex(index);
            
            // Set some friendly out-of-the-box defaults so it's not blank
            var newCond = conditionsProp.GetArrayElementAtIndex(index);
            newCond.FindPropertyRelative("variableKey").stringValue = "variable_name";
            newCond.FindPropertyRelative("op").enumValueIndex = (int)ConditionOperator.GreaterThanOrEqual;
            newCond.FindPropertyRelative("targetValue").intValue = 0;
            
            serializedActor.ApplyModifiedProperties();
            LoadDialogueEditorView(); // Clean complete redraw to show the new item row
        }) 
        { 
            text = "+ Add", 
            style = { paddingLeft = 10, paddingRight = 10, backgroundColor = new Color(0.2f, 0.3f, 0.4f) } 
        };

        condHeader.Add(condTitle);
        condHeader.Add(addConditionBtn);
        conditionsBox.Add(condHeader);

        // Build a custom clean editable row loop for every single condition entry
        for (int i = 0; i < conditionsProp.arraySize; i++)
        {
            int indexToRemove = i; 
            var conditionItemProp = conditionsProp.GetArrayElementAtIndex(i);

            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 4 } };

            var keyField = new TextField { style = { flexGrow = 2, marginRight = 4 } };
            keyField.BindProperty(conditionItemProp.FindPropertyRelative("variableKey"));
            
            var opField = new EnumField() { style = { flexGrow = 1, marginRight = 4, maxWidth = 130 } };
            opField.BindProperty(conditionItemProp.FindPropertyRelative("op"));

            var valField = new IntegerField() { style = { width = 50, marginRight = 6 } };
            valField.BindProperty(conditionItemProp.FindPropertyRelative("targetValue"));

            var removeBtn = new Button(() =>
            {
                serializedActor.Update();
                conditionsProp.DeleteArrayElementAtIndex(indexToRemove);
                serializedActor.ApplyModifiedProperties();
                LoadDialogueEditorView(); // Clean complete redraw
            }) 
            { 
                text = "X", 
                style = { backgroundColor = new Color(0.45f, 0.15f, 0.15f), color = Color.white } 
            };

            row.Add(keyField);
            row.Add(opField);
            row.Add(valField);
            row.Add(removeBtn);
            conditionsBox.Add(row);
        }

        contentContainer.Add(conditionsBox);
    }

        var chatScrollView = new ScrollView(ScrollViewMode.Vertical)
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


        var controlPanel = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.SpaceBetween
            }
        };

        var addSentenceButton = new Button(() =>
        {
            serializedActor.Update();
            var newIndex = sentencesProp.arraySize;
            sentencesProp.InsertArrayElementAtIndex(newIndex);

            // Standard auto-increment settings
            var newSentence = sentencesProp.GetArrayElementAtIndex(newIndex);

            
            try
            {
                if (currentSentence?.FindPropertyRelative("choiceBranchIDs") != null && currentSentence.FindPropertyRelative("choiceBranchIDs").arraySize == 0)
                {
                    currentSentence.FindPropertyRelative("choiceBranchIDs").arraySize = 1;
                    currentSentence.FindPropertyRelative("choiceBranchIDs").GetArrayElementAtIndex(0).intValue =
                        newIndex; // Default to no next sentence
                }else if (currentSentence?.FindPropertyRelative("choiceBranchIDs") != null &&
                          currentSentence.FindPropertyRelative("choiceBranchIDs").arraySize == 3)
                {
                    Debug.LogWarning("Current sentence is a multiple choice prompt, adding new sentence seems bullshit");
                    return;
                }
            }
            catch
            {
                Debug.LogError("Don't just delete from the data, use the fucking tool!");
            }

            currentSentence = newSentence;
            newSentence.FindPropertyRelative("id").intValue = newIndex;
            newSentence.FindPropertyRelative("text").stringValue = "New regular dialogue line...";
            newSentence.FindPropertyRelative("leftImage").boolValue = true; // Default left alignment

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
            if (currentSentence == null)
            {
                var sentences = serializedActor.FindProperty("scenes").GetArrayElementAtIndex(selectedDialogueIndex)
                    .FindPropertyRelative("sentences");
                if (sentences.arraySize == 0)
                {
                    //add some text here idfk
                    return;
                }

                currentSentence = sentences.GetArrayElementAtIndex(sentences.arraySize - 1);
            }

            var choiceBranchIDsProp = currentSentence.FindPropertyRelative("choiceBranchIDs");
            choiceBranchIDsProp.arraySize = 3;
            var newIndex = sentencesProp.arraySize;
            sentencesProp.InsertArrayElementAtIndex(newIndex);
            choiceBranchIDsProp.GetArrayElementAtIndex(0).intValue = sentencesProp.arraySize - 1;

            var choiceOne = sentencesProp.GetArrayElementAtIndex(newIndex);
            choiceOne.FindPropertyRelative("id").intValue = newIndex - 1;
            choiceOne.FindPropertyRelative("text").stringValue = "Branching choice prompt question...";
            choiceOne.FindPropertyRelative("leftImage").boolValue = true;
            choiceOne.FindPropertyRelative("choiceBranchIDs").arraySize = 0;

            newIndex = sentencesProp.arraySize;
            sentencesProp.InsertArrayElementAtIndex(newIndex);
            choiceBranchIDsProp.GetArrayElementAtIndex(1).intValue = sentencesProp.arraySize - 1;

            var choiceTwo = sentencesProp.GetArrayElementAtIndex(newIndex);
            choiceTwo.FindPropertyRelative("id").intValue = newIndex - 1;
            choiceTwo.FindPropertyRelative("text").stringValue = "Branching choice prompt question...";
            choiceTwo.FindPropertyRelative("leftImage").boolValue = true;
            choiceTwo.FindPropertyRelative("choiceBranchIDs").arraySize = 0;

            newIndex = sentencesProp.arraySize;
            sentencesProp.InsertArrayElementAtIndex(newIndex);
            choiceBranchIDsProp.GetArrayElementAtIndex(2).intValue = sentencesProp.arraySize - 1;

            var choiceThree = sentencesProp.GetArrayElementAtIndex(newIndex);
            choiceThree.FindPropertyRelative("id").intValue = newIndex - 1;
            choiceThree.FindPropertyRelative("text").stringValue = "Branching choice prompt question...";
            choiceThree.FindPropertyRelative("leftImage").boolValue = true;
            choiceThree.FindPropertyRelative("choiceBranchIDs").arraySize = 0;

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

        var mergeButton = new Button(() =>
        {
            serializedActor.Update();
            if (currentSentence == null)
            {
                Debug.LogWarning("No current sentence selected to merge from.");
                return;
            }

            // Create a new sentence that will serve as the merge point
            var newIndex = sentencesProp.arraySize;
            sentencesProp.InsertArrayElementAtIndex(newIndex);
            var mergeSentence = sentencesProp.GetArrayElementAtIndex(newIndex);
            mergeSentence.FindPropertyRelative("id").intValue = newIndex;
            mergeSentence.FindPropertyRelative("text").stringValue = "Merged dialogue point...";
            mergeSentence.FindPropertyRelative("leftImage").boolValue = true;
            mergeSentence.FindPropertyRelative("choiceBranchIDs").arraySize = 0;

            int count = 0;
            // Update all branches that are stoped to point to the new merge sentence
            for (var i = 0; i < sentencesProp.arraySize - 1; i++)
            {
                var sentenceProp = sentencesProp.GetArrayElementAtIndex(i);
                var choiceBranchIDsProp = sentenceProp.FindPropertyRelative("choiceBranchIDs");

                if (choiceBranchIDsProp.arraySize != 0) continue;
                count++;
                choiceBranchIDsProp.arraySize = 1;
                choiceBranchIDsProp.GetArrayElementAtIndex(0).intValue = newIndex;
            }

            switch (count)
            {
                case 1:
                    Debug.LogWarning("Ayo, you know you could have just said new sentence, right?");
                    break;
                case 0:
                    Debug.LogError("there must be a circle! be careful, this is probably a mistake");
                    break;
            }

            serializedActor.ApplyModifiedProperties();
            RefreshSentenceList();
        })
        {
            text = "Add Merger Sentence",
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
        controlPanel.Add(mergeButton);
        contentContainer.Add(controlPanel);

        // Track data updates
        contentContainer.RegisterCallback<SerializedPropertyChangeEvent>(ApplyPropertiesCallback);
        
        //new bottom buttons
        buttonPanel.Clear();
        var backToStart = new Button(() =>
        {
            contentContainer.Clear();
            contentContainer.Add(ActorText());
        })
        {
            text = "Back to Actor",
            style =
            {
                flexGrow = 1,
                marginRight = 5,
                paddingBottom = 8,
                paddingTop = 8,
                paddingLeft = 8,
                paddingRight = 8,
                backgroundColor = new Color(0.4f, 0.2f, 0.2f)
            }
        };

        buttonPanel.Add(backToStart);
        
        
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
            
            bool hasNext = true;
            var nextIndex = 0;
            do
            {
                var sentenceProp = sentencesProp.GetArrayElementAtIndex(nextIndex);
                currentSentence = sentenceProp;
                var choiceAmount = sentenceProp.FindPropertyRelative("choiceBranchIDs")
                    .arraySize;
                var textProp = sentenceProp.FindPropertyRelative("text");
                        var leftImageProp = sentenceProp.FindPropertyRelative("leftImage");
                
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
                                paddingRight = 2,
                                paddingLeft = 2,
                            }
                        };
                        headerLeftGroup.Add(sideToggleButton);
                        bubbleHeader.Add(headerLeftGroup);

                        // Right side of the header: The "X" Delete Button
                        var index = nextIndex;
                        var deleteButton = new Button(() =>
                        {
                            serializedActor.Update();
                            var lastId = currentSentence.FindPropertyRelative("id").intValue;
                            var replaceCurrentSelect = lastId ==  sentencesProp.GetArrayElementAtIndex(index).FindPropertyRelative("id").intValue;
                            var oldSize = sentencesProp.arraySize;
                            sentencesProp.DeleteArrayElementAtIndex(index);

                            // If Unity cleared the content instead of removing the index slot, delete it again
                            if (sentencesProp.arraySize == oldSize)
                            {
                                sentencesProp.DeleteArrayElementAtIndex(index);
                            }

                            // Optional Best Practice: Clean up IDs so they remain sequential (0, 1, 2...) after a deletion
                            //TODO: check if all choices are deleted
                            for (var k = 0; k < sentencesProp.arraySize; k++)
                            {
                                var prop = sentencesProp.GetArrayElementAtIndex(k);
                                prop.FindPropertyRelative("id").intValue = k;
                                if (prop.FindPropertyRelative("choiceBranchIDs")
                                        .arraySize <= 0) continue;
                                for(var o = 0; o < prop.FindPropertyRelative("choiceBranchIDs").arraySize; o++)
                                {
                                    if (prop.FindPropertyRelative("choiceBranchIDs").GetArrayElementAtIndex(o)
                                            .intValue == index)
                                    {
                                        prop.FindPropertyRelative("choiceBranchIDs").DeleteArrayElementAtIndex(o);
                                        if(selectedChoices.Contains(k))
                                            selectedChoices.Remove(k);
                                        break;
                                    }
                                    else if (prop.FindPropertyRelative("choiceBranchIDs").GetArrayElementAtIndex(o)
                                            .intValue > index)
                                    {
                                        prop.FindPropertyRelative("choiceBranchIDs").GetArrayElementAtIndex(o)
                                            .intValue--;
                                    }
                                }
                            }
                            
                            

                            if (sentencesProp.arraySize == 0)
                                currentSentence = null;
                            else if (replaceCurrentSelect)
                            {
                                for (var i = 0; i < sentencesProp.arraySize; i++)
                                {
                                    if (sentencesProp.GetArrayElementAtIndex(i).FindPropertyRelative("choiceBranchIDs")
                                            .arraySize != 1 || sentencesProp.GetArrayElementAtIndex(i)
                                            .FindPropertyRelative("choiceBranchIDs").GetArrayElementAtIndex(0)
                                            .intValue != lastId) continue;
                                    sentencesProp.GetArrayElementAtIndex(i).FindPropertyRelative("choiceBranchIDs")
                                        .arraySize = 0;
                                    if (selectedChoices.Contains(i))
                                    {
                                        selectedChoices.Remove(i);
                                    }
                                    break;
                                }
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
                switch (choiceAmount)
                {
                    case 1:
                    {
                        
                        choiceAmount = sentencesProp.GetArrayElementAtIndex(nextIndex)
                            .FindPropertyRelative("choiceBranchIDs")
                            .arraySize;
                        if (choiceAmount == 0)
                        {
                            hasNext = false;
                            break;
                        }
                        nextIndex = sentencesProp.GetArrayElementAtIndex(nextIndex)
                            .FindPropertyRelative("choiceBranchIDs").GetArrayElementAtIndex(0).intValue;


                        break;
                    }
                    case 3:
                        var branchListProp = sentenceProp.FindPropertyRelative("choiceBranchIDs");

                        // --- MAIN OUTER ROW ---
                        var choicesRow = new VisualElement
                        {
                            style =
                            {
                                marginBottom = 12,
                                flexDirection = FlexDirection.Row,
                                justifyContent = Justify.Center // Center the block in the chat timeline
                            }
                        };

                        // --- MAIN COMBINED BUBBLE WRAPPER ---
                        var choicesBubble = new VisualElement
                        {
                            style =
                            {
                                width = Length.Percent(90), // Wide layout to give room to the 3 horizontal panels
                                backgroundColor = new Color(0.18f, 0.18f, 0.25f, 1f), // Deep indigo theme for choices
                                paddingBottom = 8,
                                paddingTop = 8,
                                paddingLeft = 8,
                                paddingRight = 8,
                                borderBottomLeftRadius= 6,
                                borderBottomRightRadius = 6,
                                borderTopRightRadius = 6,
                                borderTopLeftRadius = 6,
                                borderBottomWidth= 1,
                                borderTopWidth = 1,
                                borderLeftWidth = 1,
                                borderRightWidth = 1,
                                borderBottomColor = new Color(0.4f, 0.4f, 0.6f, 1f),
                                borderTopColor = new Color(0.4f, 0.4f, 0.6f, 1f),
                                borderLeftColor = new Color(0.4f, 0.4f, 0.6f, 1f),
                                borderRightColor = new Color(0.4f, 0.4f, 0.6f, 1f)
                            }
                        };
                        
                        var choicesHeader = new VisualElement
                        {
                            style =
                            {
                                flexDirection = FlexDirection.Row,
                                justifyContent = Justify.SpaceBetween,
                                alignItems = Align.Center,
                                marginBottom = 6
                            }
                        };

                        var multiIdLabel =
                            new Label($"Choice Block ID: {sentenceProp.FindPropertyRelative("id").intValue}")
                            {
                                style = { unityFontStyleAndWeight = FontStyle.Bold, color = Color.cyan }
                            };
                        choicesHeader.Add(multiIdLabel);

                        // Global X Button to drop the entire branching block node
                        var globalIndex = nextIndex;
                        var choiceDeleteButton = new Button(() =>
                        {
                         
                                serializedActor.Update();
                                for(var i = sentencesProp.arraySize -1 ; i > globalIndex; i--)
                                {
                                    if (selectedChoices.Contains(i))
                                        selectedChoices.Remove(i);
                                    sentencesProp.DeleteArrayElementAtIndex(i);
                                }
                                sentencesProp.GetArrayElementAtIndex(globalIndex).FindPropertyRelative("choiceBranchIDs").arraySize = 0;
                                currentSentence = sentencesProp.GetArrayElementAtIndex(globalIndex);

                                serializedActor.ApplyModifiedProperties();
                                RefreshSentenceList();
                            
                        })
                        {
                            text = "X",
                            style =
                            {
                                unityFontStyleAndWeight = FontStyle.Bold,
                                backgroundColor = new Color(0.6f, 0.15f, 0.15f), color = Color.white
                            }
                        };
                        choicesHeader.Add(choiceDeleteButton);
                        choicesBubble.Add(choicesHeader);

                        // Spacer
                        var spaceDivider = new VisualElement { style = { height = 8 } };
                        choicesBubble.Add(spaceDivider);

                        // --- HORIZONTAL BUTTON ROW ---
                        var buttonsContainer = new VisualElement
                        {
                            style =
                            {
                                flexDirection = FlexDirection.Row,
                                justifyContent = Justify.SpaceBetween
                            }
                        };

                        var idList = new List<int>();
                        

                        // Generate the 3 horizontal columns
                        for (int c = 0; c < 3; c++)
                        {
                            var choiceIndex = c;
                            var bondedSentenceIndex = branchListProp.GetArrayElementAtIndex(choiceIndex).intValue;
                            idList.Add(bondedSentenceIndex);
                            
                            // Grab the actual sentence property that this button link points to
                            var subSentenceProp = sentencesProp.GetArrayElementAtIndex(bondedSentenceIndex);
                            var subTextProp = subSentenceProp.FindPropertyRelative("text");
                            var subBonusProp = subSentenceProp.FindPropertyRelative("bonus");

                            var highlighted = selectedChoices.Contains(bondedSentenceIndex);
                            // Vertical panel representing one choice route
                            var singleChoiceColumn = new Button(() =>
                            {
                                foreach (var id in idList)
                                {
                                    selectedChoices.Remove(id);
                                }
                                selectedChoices.Add(bondedSentenceIndex);

                                if (sentencesProp.GetArrayElementAtIndex(bondedSentenceIndex)
                                        .FindPropertyRelative("choiceBranchIDs").arraySize == 0)
                                {
                                    serializedActor.Update();
                                    var newIndex = sentencesProp.arraySize;
                                    sentencesProp.InsertArrayElementAtIndex(newIndex);

                                    // Standard auto-increment settings
                                    var newSentence = sentencesProp.GetArrayElementAtIndex(newIndex);

                                    sentencesProp.GetArrayElementAtIndex(bondedSentenceIndex).FindPropertyRelative("choiceBranchIDs").arraySize = 1;
                                    sentencesProp.GetArrayElementAtIndex(bondedSentenceIndex).FindPropertyRelative("choiceBranchIDs").GetArrayElementAtIndex(0).intValue = newIndex;

                                    currentSentence = newSentence;
                                    newSentence.FindPropertyRelative("id").intValue = newIndex;
                                    newSentence.FindPropertyRelative("text").stringValue = "New regular dialogue line...";
                                    newSentence.FindPropertyRelative("leftImage").boolValue = true; // Default left alignment

                                    serializedActor.ApplyModifiedProperties();
                                }
                                
                                RefreshSentenceList();
                            })
                            {
                                style =
                                {
                                    width = Length.Percent(31), // Fits 3 columns comfortably with spacing margins
                                    backgroundColor = highlighted ? new Color(0.40f, 0.40f, 0.50f) :new Color(0.25f, 0.25f, 0.35f, 1f),
                                    paddingBottom = 6,
                                    paddingTop = 6,
                                    paddingLeft = 6,
                                    paddingRight = 6,
                                    borderBottomLeftRadius = 4,
                                    borderBottomRightRadius = 4,
                                    borderTopLeftRadius = 4,
                                    borderTopRightRadius = 4,
                                    
                                }
                            };
                            var size = sentencesProp.GetArrayElementAtIndex(bondedSentenceIndex)
                                .FindPropertyRelative("choiceBranchIDs").arraySize;
                            var pointingText = size == 0 ? " (has no pointedID)" : $" (Points to ID: {sentencesProp.GetArrayElementAtIndex(bondedSentenceIndex).FindPropertyRelative("choiceBranchIDs").GetArrayElementAtIndex(0).intValue})";
                            var choiceMarkerLabel =
                                new Label($"Route {choiceIndex + 1} {pointingText}")
                                {
                                    style =
                                    {
                                        fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold, color = Color.gray,
                                        marginBottom = 4
                                    }
                                };
                            singleChoiceColumn.Add(choiceMarkerLabel);

                            // Editable text input field belonging directly to that target route sentence
                            var routeTextField = new PropertyField(subTextProp, "Option Text");
                            routeTextField.Bind(serializedActor);
                            singleChoiceColumn.Add(routeTextField);
                            
                            var bonusField = new PropertyField(subBonusProp, "Bonus");
                            bonusField.Bind(serializedActor);
                            singleChoiceColumn.Add(bonusField);
                            
                            buttonsContainer.Add(singleChoiceColumn);
                        }

                        choicesBubble.Add(buttonsContainer);
                        choicesRow.Add(choicesBubble);
                        chatScrollView.Add(choicesRow);

                        hasNext = false; //break here unless you select a choice to continue
                        foreach (var id in idList.Where(id => selectedChoices.Contains(id)))
                        {
                            hasNext = true;
                            nextIndex = sentencesProp.GetArrayElementAtIndex(id).FindPropertyRelative("choiceBranchIDs").GetArrayElementAtIndex(0).intValue;
                        }
                        break;
                    default:
                        hasNext = false;
                        break;
                }
            } while (hasNext);
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