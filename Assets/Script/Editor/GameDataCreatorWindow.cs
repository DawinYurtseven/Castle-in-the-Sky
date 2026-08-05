using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class GameDataCreatorWindow : EditorWindow
{
    private GameDatabase targetDatabase;
    private SerializedObject serializedDatabase;

    private RadioButtonGroup categoryToggle;
    private ListView databaseListView;
    private VisualElement inspectorContainer;
    private Button addButton;

    private enum Category { Items, Skills }
    private Category currentCategory = Category.Items;

    [MenuItem("Tools/Game Data Creator")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<GameDataCreatorWindow>();
        wnd.titleContent = new GUIContent("Data Creator");
        wnd.minSize = new Vector2(600, 400);
    }

    public void CreateGUI()
    {
        // Layout Split: Left side = Setup/List, Right side = Dynamic Fields Editor
        var root = rootVisualElement;
        root.style.flexDirection = FlexDirection.Row;

        // --- LEFT COLUMN ---
        var leftPane = new VisualElement
        {
            style =
            {
                width = 250, 
                borderRightWidth = 1, 
                borderRightColor = Color.gray, 
                paddingBottom = 10,
                paddingTop = 10,
                paddingLeft = 10,
                paddingRight = 10,
            }
        };
        root.Add(leftPane);

        // Database file selector field
        var dbField = new ObjectField("Database Asset") { objectType = typeof(GameDatabase) };
        leftPane.Add(dbField);

        // Switcher between Items and Skills
        categoryToggle = new RadioButtonGroup("Category", new List<string> { "Items", "Skills" })
        {
            style =
            {
                marginTop = 5
            }
        };
        leftPane.Add(categoryToggle);

        // The searchable/clickable list of created instances
        databaseListView = new ListView
        {
            reorderable = false,
            showAddRemoveFooter = false,
            fixedItemHeight = 22,
            style =
            {
                flexGrow = 1, 
                marginTop = 10, 
                borderBottomWidth = 1,
                borderLeftWidth = 1,
                borderRightWidth = 1,
                borderTopWidth = 1,
                borderBottomColor = Color.gray,
                borderTopColor = Color.gray,
                borderLeftColor = Color.gray,
                borderRightColor = Color.gray
            }
        };
        leftPane.Add(databaseListView);

        addButton = new Button(ShowTypeCreationMenu) { text = "Add New Subclass Object" };
        addButton.SetEnabled(false);
        leftPane.Add(addButton);

        // --- RIGHT COLUMN ---
        VisualElement rightPane = new VisualElement
        {
            style =
            {
                flexGrow = 1, 
                paddingBottom = 10,
                paddingTop = 10,
                paddingLeft = 10,
                paddingRight = 10
            }
        };
        root.Add(rightPane);
        
        rightPane.Add(new Label("Properties Editor") { style =
        {
            fontSize = 16, 
            marginBottom = 10, 
            unityFontStyleAndWeight = FontStyle.Bold
        } });

        var rightScroll = new ScrollView();
        rightPane.Add(rightScroll);

        inspectorContainer = new VisualElement();
        rightScroll.Add(inspectorContainer);

        // --- LOGIC BINDINGS ---
        dbField.RegisterValueChangedCallback(evt => ConnectToDatabase(evt.newValue as GameDatabase));
        categoryToggle.RegisterValueChangedCallback(evt => { currentCategory = (Category)evt.newValue; RefreshListView(); inspectorContainer.Clear(); });
        databaseListView.selectionChanged += _ => DrawSelectedObjectFields();
    }

    private void ConnectToDatabase(GameDatabase db)
    {
        targetDatabase = db;
        addButton.SetEnabled(targetDatabase);
        
        if (targetDatabase)
        {
            serializedDatabase = new SerializedObject(targetDatabase);
            RefreshListView();
        }
        else
        {
            databaseListView.itemsSource = null;
            databaseListView.Rebuild();
            inspectorContainer.Clear();
        }
    }

    private void RefreshListView()
    {
        if (!targetDatabase) return;
        serializedDatabase?.Update();

        if (currentCategory == Category.Items)
        {
            databaseListView.itemsSource = targetDatabase.allItems;
            databaseListView.makeItem = () => new Label();
            databaseListView.bindItem = (element, index) => 
            {
                if (index >= 0 && index < targetDatabase.allItems.Count)
                {
                    ((Label)element).text = targetDatabase.allItems[index]?.GetType().Name ?? $"[Empty Item {index}]";
                }
            };
        }
        else
        {
            databaseListView.itemsSource = targetDatabase.allSkills;
            databaseListView.makeItem = () => new Label();
            databaseListView.bindItem = (element, index) => 
            {
                if (index >= 0 && index < targetDatabase.allSkills.Count)
                {
                    ((Label)element).text = targetDatabase.allSkills[index]?.GetType().Name ?? $"[Empty Skill {index}]";
                }
            }; 
        }

        databaseListView.Rebuild();
    }

    private void ShowTypeCreationMenu()
    {
        var menu = new GenericMenu();
        var baseType = currentCategory == Category.Items ? typeof(Items) : typeof(Skill);
        
        var derivedTypes = TypeCache.GetTypesDerivedFrom(baseType)
            .Where(t => !t.IsAbstract)
            .ToList();

        foreach (Type type in derivedTypes)
        {
            menu.AddItem(new GUIContent(type.Name), false, () => InstantiateAndAddType(type));
        }
        menu.ShowAsContext();
    }

    private void InstantiateAndAddType(Type type)
    {
        serializedDatabase.Update();
        object newInstance = Activator.CreateInstance(type);

        if (currentCategory == Category.Items)
        {
            targetDatabase.allItems.Add((Items)newInstance);
        }
        else
        {
            targetDatabase.allSkills.Add((Skill)newInstance);
        }

        EditorUtility.SetDirty(targetDatabase);
        serializedDatabase.Update();
        RefreshListView();
        databaseListView.selectedIndex = databaseListView.itemsSource.Count - 1;
    }

    private void DrawSelectedObjectFields()
    {
        inspectorContainer.Clear();
        var index = databaseListView.selectedIndex;
        if (index < 0 || !targetDatabase) return;

        serializedDatabase.Update();
        
        // Find the SerializedProperty matching our selected index inside our collection
        var propertyPath = currentCategory == Category.Items ? "allItems" : "allSkills";
        var listProp = serializedDatabase.FindProperty(propertyPath);
        var elementProp = listProp.GetArrayElementAtIndex(index);

        if (elementProp == null) return;

        // Add a delete button at the top of the details view
        var deleteBtn = new Button(() => DeleteSelectedObject(index)) { text = "Delete This Object", style = { marginBottom = 10, backgroundColor = new Color(0.7f, 0.2f, 0.2f) } };
        inspectorContainer.Add(deleteBtn);

        // Extracting child fields dynamically using the technique inside your SubclassSelectorDrawer
        var endProperty = elementProp.GetEndProperty();
        var iterator = elementProp.Copy();
        var enterChildren = true;

        while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
        {
            enterChildren = false;
            var field = new PropertyField(iterator.Copy());
            
            // This hooks up live modifications directly back to the database array asset
            field.Bind(serializedDatabase);
            
            // Special update hook: if the item name changes text, refresh our list view display label immediately
            if (iterator.name == "itemName" || iterator.name.Contains("Name"))
            {
                field.RegisterValueChangeCallback(_ => RefreshListView());
            }

            inspectorContainer.Add(field);
        }
    }

    private void DeleteSelectedObject(int index)
    {
        serializedDatabase.Update();
        if (currentCategory == Category.Items)
            targetDatabase.allItems.RemoveAt(index);
        else
            targetDatabase.allSkills.RemoveAt(index);

        EditorUtility.SetDirty(targetDatabase);
        serializedDatabase.Update();
        RefreshListView();
        inspectorContainer.Clear();
    }
}