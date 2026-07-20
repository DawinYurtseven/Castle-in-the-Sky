using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;

[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
public class SubclassSelectorDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var container = new VisualElement { style = { marginBottom = 4 } };

        if (property.propertyType != SerializedPropertyType.ManagedReference)
        {
            container.Add(new Label("Use [SubclassSelector] only on [SerializeReference] fields!"));
            return container;
        }

        var baseType = GetManagedReferenceFieldType(property);
        if (baseType == null) return container;

        // 1. Fetch all pre-configured master objects from your database asset
        var database = FindDatabaseAsset();
        
        // 2. Prepare the dropdown selection choices based on what is actually available
        var choices = new List<string> { "Null (Empty)" };
        var sourcePool = new List<object>();

        if (database)
        {
            if (baseType == typeof(Items) || baseType.IsSubclassOf(typeof(Items)))
            {
                foreach (var item in database.allItems.Where(item => item != null))
                {
                    choices.Add($"{item.GetType().Name} ({item.ItemName})");
                    sourcePool.Add(item);
                }
            }
            else if (baseType == typeof(Skill) || baseType.IsSubclassOf(typeof(Skill)))
            {
                foreach (var skill in database.allSkills.Where(skill => skill != null))
                {
                    // Modify this to match whatever naming property your Skill class uses!
                    choices.Add(skill.GetType().Name); 
                    sourcePool.Add(skill);
                }
            }
        }
        else
        {
            // Fallback to blank system types if no database exists yet
            var inheritedTypes = TypeCache.GetTypesDerivedFrom(baseType).Where(t => !t.IsAbstract).ToList();
            choices.AddRange(inheritedTypes.Select(t => $"{t.Name} (Blank)"));
            sourcePool.AddRange(inheritedTypes);
        }

        // Determine current selection display index
        var selectedIndex = 0;
        if (property.managedReferenceValue != null)
        {
            var currentTypeName = property.managedReferenceValue.GetType().Name;
            // Best effort match by type name
            for (var i = 0; i < sourcePool.Count; i++)
            {
                var target = sourcePool[i];
                var t = target as Type ?? target.GetType();
                if (t.Name != currentTypeName) continue;
                selectedIndex = i + 1; // offset by "Null"
                break;
            }
        }

        var typeDropdown = new DropdownField(property.displayName, choices, selectedIndex);
        container.Add(typeDropdown);

        var fieldsContainer = new VisualElement { style = { paddingLeft = 15 } };
        container.Add(fieldsContainer);

        RefreshChildFields();

        // 3. Handle data copy when selected
        typeDropdown.RegisterValueChangedCallback(_ =>
        {
            var index = typeDropdown.index;

            if (index == 0)
            {
                property.managedReferenceValue = null;
            }
            else
            {
                var source = sourcePool[index - 1];

                if (source is Type typeSource)
                {
                    // Fallback create blank if database didn't have it
                    property.managedReferenceValue = Activator.CreateInstance(typeSource);
                }
                else
                {
                    // CRITICAL: Clone the data from your database instance so it doesn't stay linked to the asset file
                    var json = JsonUtility.ToJson(source);
                    var clone = Activator.CreateInstance(source.GetType());
                    JsonUtility.FromJsonOverwrite(json, clone);
                    
                    property.managedReferenceValue = clone;
                }
            }

            property.serializedObject.ApplyModifiedProperties();
            RefreshChildFields();
        });

        return container;

        void RefreshChildFields()
        {
            fieldsContainer.Clear();
            if (property.managedReferenceValue == null) return;
            var endProperty = property.GetEndProperty();
            var iterator = property.Copy();
            var enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                enterChildren = false; 
                var propField = new PropertyField(iterator.Copy());
                propField.Bind(property.serializedObject);
                fieldsContainer.Add(propField);
            }
        }
    }

    private static GameDatabase FindDatabaseAsset()
    {
        var data = Resources.Load<GameDatabase>("Values");
        return data;
    }

    private static Type GetManagedReferenceFieldType(SerializedProperty property)
    {
        var parts = property.managedReferenceFieldTypename.Split(' ');
        return parts.Length < 2 ? null : Type.GetType($"{parts[1]}, {parts[0]}");
    }
}