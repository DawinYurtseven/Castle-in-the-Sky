using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
public class SubclassSelectorDrawer : PropertyDrawer
{
   
    //AI code that I need to analyse later but it works so no complaining
    
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // Container element that holds the dropdown and child fields
        VisualElement container = new VisualElement
        {
            style =
            {
                marginBottom = 4
            }
        };

        if (property.propertyType != SerializedPropertyType.ManagedReference)
        {
            container.Add(new Label("Use [SubclassSelector] only on [SerializeReference] fields!"));
            return container;
        }

        // 1. Get the base class type from the field ('Skill')
        Type baseType = GetManagedReferenceFieldType(property);
        if (baseType == null) return container;

        // 2. Efficiently grab all non-abstract types that inherit from 'Skill'
        List<Type> inheritedTypes = TypeCache.GetTypesDerivedFrom(baseType)
            .Where(t => !t.IsAbstract)
            .ToList();

        // 3. Prepare the dropdown selection choices
        List<string> choices = new List<string> { "Null (Empty)" };
        choices.AddRange(inheritedTypes.Select(t => t.Name));

        // Determine current selection index
        string currentTypeName = property.managedReferenceValue?.GetType().Name;
        int selectedIndex = string.IsNullOrEmpty(currentTypeName) ? 0 : choices.IndexOf(currentTypeName);
        if (selectedIndex == -1) selectedIndex = 0;

        // 4. Create the Dropdown field
        DropdownField typeDropdown = new DropdownField(property.displayName, choices, selectedIndex);
        container.Add(typeDropdown);

        // 5. Container to hold all fields belonging to GrandSlash, HealAll, etc.
        VisualElement fieldsContainer = new VisualElement
        {
            style =
            {
                paddingLeft = 15 // Indent child properties
            }
        };
        container.Add(fieldsContainer);

        // Initialize child fields for the first load
        RefreshChildFields();

        // 6. Handle type assignment when the user picks a different class from the dropdown
        typeDropdown.RegisterValueChangedCallback(_ =>
        {
            int index = typeDropdown.index;

            if (index == 0)
            {
                property.managedReferenceValue = null;
            }
            else
            {
                // Target index minus the offset of the "Null" option
                Type selectedType = inheritedTypes[index - 1];
                property.managedReferenceValue = Activator.CreateInstance(selectedType);
            }

            property.serializedObject.ApplyModifiedProperties();
            
            // Re-render fields for the newly chosen instance without rebuilding the whole inspector
            RefreshChildFields();
        });

        return container;

        // Helper method to draw child properties dynamically when type changes
        void RefreshChildFields()
        {
            fieldsContainer.Clear();
            if (property.managedReferenceValue != null)
            {
                // Iterates through child properties automatically without writing boilerplate
                SerializedProperty endProperty = property.GetEndProperty();
                SerializedProperty iterator = property.Copy();
                bool enterChildren = true;

                while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
                {
                    enterChildren = false; // Do not deeply nest internally hidden tracking data
                    var propField = new PropertyField(iterator.Copy());
                    propField.Bind(property.serializedObject);
                    fieldsContainer.Add(propField);
                }
            }
        }
    }

    private Type GetManagedReferenceFieldType(SerializedProperty property)
    {
        // Extracts the full assembly name and type name string from the managed reference data
        string[] parts = property.managedReferenceFieldTypename.Split(' ');
        if (parts.Length < 2) return null;
        return Type.GetType($"{parts[1]}, {parts[0]}");
    }
}
