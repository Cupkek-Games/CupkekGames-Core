#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.Core.Editor
{
    /// <summary>
    /// A searchable dropdown for selecting types, similar to SerializeReference picker.
    /// Uses Unity's AdvancedDropdown for better UX with many types.
    /// </summary>
    public class TypeAdvancedDropdown : AdvancedDropdown
    {
        public event Action<Type> OnTypeSelected;

        private readonly Type _baseType;
        private readonly bool _includeAbstract;
        private readonly bool _includeInterfaces;
        private readonly bool _includeNone;
        private Dictionary<int, Type> _idToType = new();

        public TypeAdvancedDropdown(
            Type baseType,
            AdvancedDropdownState state,
            bool includeAbstract = false,
            bool includeInterfaces = true,
            bool includeNone = true) : base(state)
        {
            _baseType = baseType;
            _includeAbstract = includeAbstract;
            _includeInterfaces = includeInterfaces;
            _includeNone = includeNone;

            minimumSize = new Vector2(250, 300);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem(_baseType?.Name ?? "Select Type");
            _idToType.Clear();

            // Add "None" option
            if (_includeNone)
            {
                var noneItem = new AdvancedDropdownItem("(Any)");
                _idToType[noneItem.id] = null;
                root.AddChild(noneItem);
                root.AddSeparator();
            }

            // Get all types
            var types = DiscoverTypes().ToList();

            // Group by namespace
            var grouped = types
                .GroupBy(t => SimplifyNamespace(t.Namespace ?? "Global"))
                .OrderBy(g => g.Key);

            foreach (var group in grouped)
            {
                var typesInGroup = group.OrderBy(t => t.Name).ToList();

                // If only one type in namespace, add directly
                if (typesInGroup.Count == 1)
                {
                    var type = typesInGroup[0];
                    var item = CreateTypeItem(type);
                    _idToType[item.id] = type;
                    root.AddChild(item);
                }
                else
                {
                    // Create namespace folder
                    var folderItem = new AdvancedDropdownItem(group.Key);

                    foreach (var type in typesInGroup)
                    {
                        var item = CreateTypeItem(type);
                        _idToType[item.id] = type;
                        folderItem.AddChild(item);
                    }

                    root.AddChild(folderItem);
                }
            }

            return root;
        }

        private AdvancedDropdownItem CreateTypeItem(Type type)
        {
            string prefix = "";
            if (type.IsInterface)
                prefix = "[I] ";
            else if (type.IsAbstract)
                prefix = "[A] ";

            return new AdvancedDropdownItem($"{prefix}{type.Name}");
        }

        private string SimplifyNamespace(string ns)
        {
            if (string.IsNullOrEmpty(ns) || ns == "Global")
                return "Global";

            // Shorten common namespaces
            return ns
                .Replace("CupkekGames.", "CG.")
                .Replace("UnityEngine.", "UE.")
                .Replace("UnityEditor.", "UEd.");
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (_idToType.TryGetValue(item.id, out var type))
            {
                OnTypeSelected?.Invoke(type);
            }
        }

        private IEnumerable<Type> DiscoverTypes()
        {
            var results = new HashSet<Type>();

            if (_baseType == null)
                return results;

            if (_baseType.IsInterface)
            {
                var implementers = TypeCache.GetTypesDerivedFrom(_baseType);
                foreach (var type in implementers)
                {
                    if (ShouldIncludeType(type))
                        results.Add(type);
                }

                if (_includeInterfaces)
                    results.Add(_baseType);
            }
            else
            {
                var derived = TypeCache.GetTypesDerivedFrom(_baseType);
                foreach (var type in derived)
                {
                    if (ShouldIncludeType(type))
                        results.Add(type);
                }

                if (!_baseType.IsAbstract || _includeAbstract)
                    results.Add(_baseType);
            }

            return results;
        }

        private bool ShouldIncludeType(Type type)
        {
            if (type.IsGenericTypeDefinition)
                return false;

            if (type.IsInterface)
                return _includeInterfaces;

            if (type.IsAbstract)
                return _includeAbstract;

            return true;
        }
    }

    /// <summary>
    /// A button-style field that opens a searchable type picker dropdown.
    /// Similar to SerializeReference picker UI.
    /// </summary>
    public class TypePickerField : VisualElement
    {
        public event Action<Type> OnTypeSelected;

        private readonly Type _baseType;
        private readonly bool _includeAbstract;
        private readonly bool _includeInterfaces;
        private readonly Button _button;
        private readonly AdvancedDropdownState _dropdownState;
        private Type _selectedType;

        /// <summary>
        /// Creates a type picker for types assignable to the specified base type.
        /// </summary>
        public TypePickerField(
            Type baseType,
            string label = null,
            bool includeAbstract = false,
            bool includeInterfaces = true)
        {
            _baseType = baseType;
            _includeAbstract = includeAbstract;
            _includeInterfaces = includeInterfaces;
            _dropdownState = new AdvancedDropdownState();

            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            style.flexGrow = 1;

            if (!string.IsNullOrEmpty(label))
            {
                var labelElement = new Label(label);
                labelElement.style.marginRight = 4;
                Add(labelElement);
            }

            _button = new Button(ShowDropdown);
            _button.text = "(Any)";
            _button.style.flexGrow = 1;
            _button.style.unityTextAlign = TextAnchor.MiddleLeft;
            Add(_button);
        }

        /// <summary>
        /// Gets the currently selected type.
        /// </summary>
        public Type SelectedType => _selectedType;

        /// <summary>
        /// Sets the selected type by full name.
        /// </summary>
        public void SetSelectedType(string fullTypeName)
        {
            if (string.IsNullOrEmpty(fullTypeName))
            {
                _selectedType = null;
                _button.text = "(Any)";
                return;
            }

            var type = TypePickerUtility.FindTypeByName(fullTypeName);
            SetSelectedType(type);
        }

        /// <summary>
        /// Sets the selected type directly.
        /// </summary>
        public void SetSelectedType(Type type)
        {
            _selectedType = type;
            _button.text = FormatTypeName(type);
        }

        private void ShowDropdown()
        {
            var dropdown = new TypeAdvancedDropdown(
                _baseType,
                _dropdownState,
                _includeAbstract,
                _includeInterfaces,
                true
            );

            dropdown.OnTypeSelected += type =>
            {
                _selectedType = type;
                _button.text = FormatTypeName(type);
                OnTypeSelected?.Invoke(type);
            };

            var buttonRect = _button.worldBound;
            dropdown.Show(buttonRect);
        }

        private string FormatTypeName(Type type)
        {
            if (type == null)
                return "(Any)";

            string prefix = "";
            if (type.IsInterface)
                prefix = "[I] ";
            else if (type.IsAbstract)
                prefix = "[A] ";

            return $"{prefix}{type.Name}";
        }
    }

    /// <summary>
    /// Static utility methods for type-related operations.
    /// </summary>
    public static class TypePickerUtility
    {
        /// <summary>
        /// Gets all types that implement a specific interface.
        /// </summary>
        public static IEnumerable<Type> GetImplementingTypes<TInterface>() where TInterface : class
        {
            return GetImplementingTypes(typeof(TInterface));
        }

        /// <summary>
        /// Gets all types that implement a specific interface.
        /// </summary>
        public static IEnumerable<Type> GetImplementingTypes(Type interfaceType)
        {
            if (!interfaceType.IsInterface)
                throw new ArgumentException($"{interfaceType.Name} is not an interface", nameof(interfaceType));

            return TypeCache.GetTypesDerivedFrom(interfaceType)
                .Where(t => !t.IsAbstract && !t.IsInterface && !t.IsGenericTypeDefinition);
        }

        /// <summary>
        /// Gets all concrete types derived from a base type.
        /// </summary>
        public static IEnumerable<Type> GetDerivedTypes<TBase>() where TBase : class
        {
            return GetDerivedTypes(typeof(TBase));
        }

        /// <summary>
        /// Gets all concrete types derived from a base type.
        /// </summary>
        public static IEnumerable<Type> GetDerivedTypes(Type baseType)
        {
            return TypeCache.GetTypesDerivedFrom(baseType)
                .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition);
        }

        /// <summary>
        /// Finds a type by its full name across all loaded assemblies.
        /// </summary>
        public static Type FindTypeByName(string fullTypeName)
        {
            if (string.IsNullOrEmpty(fullTypeName))
                return null;

            // Try direct lookup first
            var type = Type.GetType(fullTypeName);
            if (type != null)
                return type;

            // Search all assemblies
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(fullTypeName);
                if (type != null)
                    return type;
            }

            return null;
        }
    }
}
#endif
