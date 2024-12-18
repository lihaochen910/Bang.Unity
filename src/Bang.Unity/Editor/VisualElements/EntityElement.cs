using System;
using System.Linq;
using Bang.Components;
using Bang.Entities;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace Bang.Unity.Editor {

	public class EntityElement : VisualElement
    {
        private readonly Entity _entity;
        private readonly ListView _componentsListView;
        private readonly Label _entityIdLabel;
        private readonly Button _destroyButton;
        
        public EntityElement(Entity entity)
        {
            _entity = entity;
            
            if (_entity == null)
            {
                Add(new SpaceElement());
                return;
            }

            if (_entity.IsDestroyed)
            {
                var helpBox = new HelpBox("Entity is dead.", HelpBoxMessageType.Warning);
                Add(helpBox);
                Add(new SpaceElement());
                return;
            }

            // Entity ID
            _entityIdLabel = new Label($"EntityId: {_entity.EntityId}")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold }
            };
            Add(_entityIdLabel);

            // Destroy按钮
            var buttonContainer = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            _destroyButton = new Button(() => _entity.Destroy())
            {
                text = "Destroy Entity",
                style =
                {
                    backgroundColor = new StyleColor(Color.red),
                    color = new StyleColor(Color.white)
                }
            };
            buttonContainer.Add(_destroyButton);
            Add(buttonContainer);

            // 组件列表
            var componentsContainer = new VisualElement
            {
                style =
                {
                    backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f, 0.3f)),
                    marginTop = 5,
                    marginBottom = 5,
                    paddingTop = 5,
                    paddingBottom = 5,
                    paddingLeft = 5,
                    paddingRight = 5
                }
            };

            // 组件列表头部
            var headerContainer = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row }
            };
            
            var componentsLabel = new Label($"Components ({_entity.Components.Length})")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold }
            };
            
            var expandAllButton = new Button(() => ExpandAllComponents(true)) { text = "▾" };
            var collapseAllButton = new Button(() => ExpandAllComponents(false)) { text = "▸" };
            
            headerContainer.Add(componentsLabel);
            headerContainer.Add(collapseAllButton);
            headerContainer.Add(expandAllButton);
            
            componentsContainer.Add(headerContainer);

            // 添加组件按钮
            var addComponentContainer = CreateAddComponentMenu();
            componentsContainer.Add(addComponentContainer);

            // 组件搜索
            var searchField = new ToolbarSearchField();
            searchField.RegisterValueChangedCallback(evt =>
            {
                FilterComponents(evt.newValue);
            });
            componentsContainer.Add(searchField);

            // 组件列表
            _componentsListView = new ListView
            {
                makeItem = () => new ComponentElement(),
                bindItem = (element, index) =>
                {
                    var componentElement = element as ComponentElement;
                    var component = _entity.Components[index];
                    componentElement.UpdateComponent(component, _entity);
                },
                itemsSource = _entity.Components.ToList()
            };
            
            componentsContainer.Add(_componentsListView);
            Add(componentsContainer);
        }

        private void ExpandAllComponents(bool expand)
        {
            // 通知所有ComponentElement展开/折叠
            _componentsListView.Query<ComponentElement>()
                .ForEach(element => element.SetExpanded(expand));
        }

        private void FilterComponents(string searchString)
        {
            // 实现组件过滤逻辑
            _componentsListView.Query<ComponentElement>()
                .ForEach(element => element.SetVisible(
                    string.IsNullOrEmpty(searchString) || 
                    element.ComponentName.ToLower().Contains(searchString.ToLower())
                ));
        }

        private VisualElement CreateAddComponentMenu()
        {
            var container = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            
            var popup = new PopupField<Type>("Add Component", 
                GetAvailableComponentTypes(),
                0,
                type => type.Name.RemoveSuffix("Component"),
                type => type.Name.RemoveSuffix("Component"));

            popup.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue != null)
                {
                    var component = Activator.CreateInstance(evt.newValue) as IComponent;
                    _entity.AddComponent(component, evt.newValue);
                    _componentsListView.Rebuild();
                }
            });

            container.Add(popup);
            return container;
        }

        private System.Collections.Generic.List<Type> GetAvailableComponentTypes()
        {
            return EntityDrawer.GetComponentInfos()
                .Where(kv => !_entity.HasComponent(kv.Value.Index) && 
                           !kv.Value.Type.IsInterface && 
                           !kv.Value.Type.IsGenericType)
                .Select(kv => kv.Value.Type)
                .ToList();
        }
    }

}
