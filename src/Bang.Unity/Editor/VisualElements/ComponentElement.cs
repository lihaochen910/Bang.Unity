using System;
using System.Reflection;
using System.Linq;
using Bang.Components;
using Bang.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


namespace Bang.Unity.Editor {

	public class ComponentElement : VisualElement
    {
        private IComponent _component;
        private Entity _entity;
        private bool _isExpanded;
        private VisualElement _contentContainer;
        
        public string ComponentName => _component?.GetType().Name.RemoveSuffix("Component") ?? string.Empty;

        public ComponentElement()
        {
            style.marginLeft = 5;
            style.marginRight = 5;
            style.marginTop = 2;
            style.marginBottom = 2;
        }

        public void UpdateComponent(IComponent component, Entity entity)
        {
            _component = component;
            _entity = entity;
            
            Clear();
            
            var header = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            
            // Foldout
            var foldout = new Foldout
            {
                text = ComponentName,
                value = _isExpanded
            };
            foldout.RegisterValueChangedCallback(evt =>
            {
                _isExpanded = evt.newValue;
                UpdateContent();
            });
            
            // Remove button
            var removeButton = new Button(() =>
            {
                if (EditorUtility.DisplayDialog("Remove Component", 
                    $"Remove {ComponentName}?", "Yes", "No"))
                {
                    _entity.RemoveComponent(_component.GetType());
                }
            })
            {
                text = "-",
                style = { width = 20 }
            };

            header.Add(foldout);
            header.Add(removeButton);
            Add(header);

            // Content container
            _contentContainer = new VisualElement
            {
                style =
                {
                    marginLeft = 15,
                    display = _isExpanded ? DisplayStyle.Flex : DisplayStyle.None
                }
            };
            Add(_contentContainer);

            UpdateContent();
        }

        private void UpdateContent()
        {
            if (!_isExpanded)
            {
                _contentContainer.style.display = DisplayStyle.None;
                return;
            }

            _contentContainer.style.display = DisplayStyle.Flex;
            _contentContainer.Clear();

            var memberInfos = _component.GetType().GetPublicMemberInfos();
            foreach (var info in memberInfos)
            {
                if (info.HasAttribute<HideInEditorAttribute>())
                    continue;

                var memberValue = info.GetValue(_component);
                CreateMemberField(info, memberValue);
            }
        }

        private void CreateMemberField(PublicMemberInfo info, object value)
        {
            var memberType = value?.GetType() ?? info.Type;
            
            if (memberType == typeof(int))
            {
                var field = new IntegerField(info.Name) { value = (int)value };
                field.RegisterValueChangedCallback(evt => UpdateComponentValue(info, evt.newValue));
                _contentContainer.Add(field);
            }
            else if (memberType == typeof(float))
            {
                var field = new FloatField(info.Name) { value = (float)value };
                field.RegisterValueChangedCallback(evt => UpdateComponentValue(info, evt.newValue));
                _contentContainer.Add(field);
            }
            else if (memberType == typeof(string))
            {
                var field = new TextField(info.Name) { value = (string)value };
                field.RegisterValueChangedCallback(evt => UpdateComponentValue(info, evt.newValue));
                _contentContainer.Add(field);
            }
            else if (memberType == typeof(bool))
            {
                var field = new Toggle(info.Name) { value = (bool)value };
                field.RegisterValueChangedCallback(evt => UpdateComponentValue(info, evt.newValue));
                _contentContainer.Add(field);
            }
            else if (memberType == typeof(Vector2))
            {
                var field = new Vector2Field(info.Name) { value = (Vector2)value };
                field.RegisterValueChangedCallback(evt => UpdateComponentValue(info, evt.newValue));
                _contentContainer.Add(field);
            }
            else if (memberType == typeof(Vector3))
            {
                var field = new Vector3Field(info.Name) { value = (Vector3)value };
                field.RegisterValueChangedCallback(evt => UpdateComponentValue(info, evt.newValue));
                _contentContainer.Add(field);
            }
            else if (memberType.IsEnum)
            {
                var field = new EnumField(info.Name, (Enum)value);
                field.RegisterValueChangedCallback(evt => UpdateComponentValue(info, evt.newValue));
                _contentContainer.Add(field);
            }
            else
            {
                var label = new Label($"{info.Name}: {value?.ToString() ?? "null"}");
                _contentContainer.Add(label);
            }
        }

        private void UpdateComponentValue(PublicMemberInfo info, object newValue)
        {
            info.SetValue(_component, newValue);
            _entity.ReplaceComponent(_component, _component.GetType());
        }
        
        public void UpdateComponent(object component, EntityInstance entityInstance)
        {
            _component = component as IComponent;
            _entity = null; // 编辑时没有实际Entity
            
            Clear();
            
            var header = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            
            // Foldout
            var foldout = new Foldout
            {
                text = ComponentName,
                value = _isExpanded
            };
            foldout.RegisterValueChangedCallback(evt =>
            {
                _isExpanded = evt.newValue;
                UpdateContentForInstance(entityInstance);
            });

            header.Add(foldout);
            Add(header);

            // Content container
            _contentContainer = new VisualElement
            {
                style =
                {
                    marginLeft = 15,
                    display = _isExpanded ? DisplayStyle.Flex : DisplayStyle.None
                }
            };
            Add(_contentContainer);

            UpdateContentForInstance(entityInstance);
        }

        private void UpdateContentForInstance(EntityInstance entityInstance)
        {
            if (!_isExpanded)
            {
                _contentContainer.style.display = DisplayStyle.None;
                return;
            }

            _contentContainer.style.display = DisplayStyle.Flex;
            _contentContainer.Clear();

            var memberInfos = _component.GetType().GetPublicMemberInfos();
            foreach (var info in memberInfos)
            {
                if (info.HasAttribute<HideInEditorAttribute>())
                    continue;

                var memberValue = info.GetValue(_component);
                CreateMemberFieldForInstance(info, memberValue, entityInstance);
            }
        }

        private void CreateMemberFieldForInstance(PublicMemberInfo info, object value, EntityInstance entityInstance)
        {
            var memberType = value?.GetType() ?? info.Type;
            
            if (memberType == typeof(int))
            {
                var field = new IntegerField(info.Name) { value = (int)value };
                field.RegisterValueChangedCallback(evt => UpdateComponentValueForInstance(info, evt.newValue, entityInstance));
                _contentContainer.Add(field);
            }
            else if (memberType == typeof(float))
            {
                var field = new FloatField(info.Name) { value = (float)value };
                field.RegisterValueChangedCallback(evt => UpdateComponentValueForInstance(info, evt.newValue, entityInstance));
                _contentContainer.Add(field);
            }
            else if (memberType == typeof(string))
            {
                var field = new TextField(info.Name) { value = (string)value };
                field.RegisterValueChangedCallback(evt => UpdateComponentValueForInstance(info, evt.newValue, entityInstance));
                _contentContainer.Add(field);
            }
            else if (memberType == typeof(bool))
            {
                var field = new Toggle(info.Name) { value = (bool)value };
                field.RegisterValueChangedCallback(evt => UpdateComponentValueForInstance(info, evt.newValue, entityInstance));
                _contentContainer.Add(field);
            }
            else if (memberType == typeof(Vector2))
            {
                var field = new Vector2Field(info.Name) { value = (Vector2)value };
                field.RegisterValueChangedCallback(evt => UpdateComponentValueForInstance(info, evt.newValue, entityInstance));
                _contentContainer.Add(field);
            }
            else if (memberType == typeof(Vector3))
            {
                var field = new Vector3Field(info.Name) { value = (Vector3)value };
                field.RegisterValueChangedCallback(evt => UpdateComponentValueForInstance(info, evt.newValue, entityInstance));
                _contentContainer.Add(field);
            }
            else if (memberType.IsEnum)
            {
                var field = new EnumField(info.Name, (Enum)value);
                field.RegisterValueChangedCallback(evt => UpdateComponentValueForInstance(info, evt.newValue, entityInstance));
                _contentContainer.Add(field);
            }
            else
            {
                var label = new Label($"{info.Name}: {value?.ToString() ?? "null"}");
                _contentContainer.Add(label);
            }
        }

        private void UpdateComponentValueForInstance(PublicMemberInfo info, object newValue, EntityInstance entityInstance)
        {
            info.SetValue(_component, newValue);
            
            // 通知EntityInstance更新
            if (entityInstance != null)
            {
                var componentIndex = entityInstance.Components.IndexOf(_component);
                if (componentIndex >= 0)
                {
                    entityInstance.Components[componentIndex] = _component;
                }
            }
        }

        public void SetExpanded(bool expanded)
        {
            _isExpanded = expanded;
            UpdateContent();
        }

        public void SetVisible(bool visible)
        {
            style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
    }

}
