using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Bang.Components;
using Bang.Entities;
using Bang.Interactions;
using Bang.StateMachines;
using Bang.Unity.Conversion;
using Bang.Unity.Utilities;
using GameCreator.Editor.Common;
using UnityEditor;
using UnityEngine;
using CopyPasteUtils = Bang.Unity.Editor.Utilities.CopyPasteUtils;


namespace Bang.Unity.Editor {

public static partial class EntityDrawer {

    public static void DrawEntity( Entity entity ) {
        if ( entity is null ) {
            EditorGUILayout.Space();
            return;
        }

        if ( entity.IsDestroyed ) {
            EditorGUILayout.HelpBox( "Entity is dead.", MessageType.Warning );
            EditorGUILayout.Space();
            return;
        }
        
        EditorGUILayout.PrefixLabel( $"EntityId: {entity.EntityId}" );

        EditorGUILayout.BeginHorizontal();

        var bgColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.red;
        // if ( entity.TryGetGameObjectReference() is { GameObject: not null } gameObjectReference &&
        //      GUILayout.Button( "Destroy GameObject And Entity" ) ) {
        //     Object.Destroy( gameObjectReference.GameObject );
        // }

        if ( GUILayout.Button( "Destroy Entity" ) ) {
            entity.Destroy();
        }

        GUI.backgroundColor = bgColor;

        EditorGUILayout.EndHorizontal();

        DrawComponents( entity );

        EditorGUILayout.Space();

        // TODO: Retained by
        // EditorGUILayout.LabelField($"Retained by ({entity.RetainCount})", EditorStyles.boldLabel);
        //
        // if (entity.Aerc is SafeAERC safeAerc)
        // {
        //     EditorLayout.BeginVerticalBox();
        //     {
        //         foreach (var owner in safeAerc.Owners.OrderBy(o => o.GetType().Name))
        //         {
        //             EditorGUILayout.BeginHorizontal();
        //             {
        //                 EditorGUILayout.LabelField(owner.ToString());
        //                 if (MiniButton("Release"))
        //                     entity.Release(owner);
        //
        //                 EditorGUILayout.EndHorizontal();
        //             }
        //         }
        //     }
        //     EditorLayout.EndVerticalBox();
        // }
    }

    public static void DrawMultipleEntities( Entity[] entities ) {
        EditorGUILayout.Space();
        var entity0 = entities[ 0 ];
        if ( !entity0.IsDestroyed ) {
            EditorGUILayout.BeginHorizontal();
            {
                var newComponentType = DrawAddComponentMenu( entity0 );
                if ( newComponentType != null ) {
                    entity0.AddComponent( Activator.CreateInstance( newComponentType ) as IComponent, newComponentType );
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
        
        var bgColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.red;

        if ( GUILayout.Button( "Destroy selected entities" ) )
            foreach ( var entity in entities )
                entity.Destroy();

        GUI.backgroundColor = bgColor;

        EditorGUILayout.Space();

        foreach ( var entity in entities ) {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField( entity.ToString() );

                bgColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.red;

                if ( MiniButton( "Destroy Entity" ) ) {
                    entity.Destroy();
                }

                GUI.backgroundColor = bgColor;
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    public static void DrawComponents( Entity entity ) {
        var unfoldedComponents = GetUnfoldedComponents();
        var componentMemberSearch = GetComponentMemberSearch();

        var components = entity.Components;
        EditorGUILayout.BeginVertical( GUI.skin.box, Array.Empty< GUILayoutOption >() );
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField( $"Components ({components.Length})", EditorStyles.boldLabel );
                if ( MiniButtonLeft( "▸" ) ) {
                    foreach ( var keyValuePair in unfoldedComponents ) {
                        unfoldedComponents[ keyValuePair.Key ] = false;
                    }
                }

                if ( MiniButtonRight( "▾" ) ) {
                    foreach ( var keyValuePair in unfoldedComponents ) {
                        unfoldedComponents[ keyValuePair.Key ] = true;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            var newComponentType = DrawAddComponentMenu( entity );
            if ( newComponentType != null ) {
                entity.AddComponent( Activator.CreateInstance( newComponentType ) as IComponent, newComponentType );
            }

            EditorGUILayout.Space();

            ComponentNameSearchString = SearchTextField( ComponentNameSearchString );

            EditorGUILayout.Space();

            var orderedComponents = OrderComponents( components );
            for ( var i = 0; i < orderedComponents.Length; i++ ) {
                var bgColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.green;
                
                DrawComponent( unfoldedComponents, componentMemberSearch, entity, orderedComponents[ i ] );

                GUI.backgroundColor = bgColor;
            }
        }
        EditorGUILayout.EndVertical();
    }

    public static void DrawComponent( Dictionary< Type, bool > unfoldedComponents, Dictionary< Type, string > componentMemberSearch, Entity entity, IComponent component ) {
        var componentType = component.GetType();
        var componentName = componentType.Name.RemoveSuffix( "Component" );

        var isStateMachineComponent = false;
        var isInteractiveComponent = false;
        if ( componentType.IsGenericType && componentType.GetGenericTypeDefinition() == typeof( StateMachineComponent<> ) ) {
            componentName = componentType.GetGenericArguments()[ 0 ].Name;
            isStateMachineComponent = true;
        }
        else if ( componentType.IsGenericType && componentType.GetGenericTypeDefinition() == typeof( InteractiveComponent<> ) ) {
            componentName = componentType.GetGenericArguments()[ 0 ].Name;
            isInteractiveComponent = true;
        }
        
        if ( MatchesSearchString( componentName.ToLower(), ComponentNameSearchString.ToLower() ) ) {
            EditorGUILayout.BeginVertical();
            {
                // if (!Attribute.IsDefined(componentType, typeof(DontDrawComponentAttribute)))
                {
                    var memberInfos = componentType.GetPublicMemberInfos();
                    EditorGUILayout.BeginHorizontal();
                    {
                        if ( memberInfos.Length == 0 && ( !isStateMachineComponent && !isInteractiveComponent ) ) {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField( new GUIContent( EditorGUIUtility.IconContent( "sv_icon_dot3_sml" ).image ), EditorStyles.boldLabel, GUILayout.MaxWidth( 10f ) );
                            EditorGUILayout.LabelField( componentName, EditorStyles.boldLabel );
                            EditorGUILayout.EndHorizontal();
                        }
                        else {
                            if ( unfoldedComponents.ContainsKey( componentType ) ) {
                                unfoldedComponents[ componentType ] = Foldout( unfoldedComponents[ componentType ], componentName, Styles.BooScriptIconTexture, FoldoutStyle );
                                if ( unfoldedComponents[ componentType ] ) {
                                    componentMemberSearch[ componentType ] = memberInfos.Length > 5
                                        ? SearchTextField( componentMemberSearch[ componentType ] )
                                        : string.Empty;
                                }
                            }
                        }

                        if ( MiniButton( "-" ) ) {
                            entity.RemoveComponent( componentType );
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    if ( unfoldedComponents.ContainsKey( componentType ) &&
                         unfoldedComponents[ componentType ] ) {
                        // var newComponent = entity.CreateComponent(index, componentType);
                        // component.CopyPublicMemberValues(newComponent);

                        var changed = false;
                        var componentDrawer = GetComponentDrawer( componentType );
                        if ( componentDrawer != null ) {
                            EditorGUI.BeginChangeCheck();
                            {
                                componentDrawer.DrawComponent( component );
                            }
                            changed = EditorGUI.EndChangeCheck();
                        }
                        else {
                            foreach ( var info in memberInfos ) {
                                if ( info.HasAttribute< HideInEditorAttribute >() ) {
                                    continue;
                                }
                                
                                if ( MatchesSearchString( info.Name.ToLower(), componentMemberSearch[ componentType ].ToLower() ) ) {
                                    var memberValue = info.GetValue( component );
                                    var memberType = memberValue == null ? info.Type : memberValue.GetType();
                                    if ( DrawObjectMember( memberType, info.Name, memberValue, component, info.SetValue ) ) {
                                        changed = true;
                                    }
                                }
                            }
                        }

                        if ( changed ) {
                            entity.ReplaceComponent( component, component.GetType() );
                        }
                        // else
                        //     entity.GetComponentPool(index).Push(newComponent);
                    }
                }

                // else
                // {
                //     EditorGUILayout.LabelField(componentName, "[DontDrawComponent]", EditorStyles.boldLabel);
                // }
            }
            EditorGUILayout.EndVertical();
        }
    }

    private static List< IComponent > _orderListCache = new ();
    private static ImmutableArray< IComponent >.Builder _orderedComponentsCache = ImmutableArray.CreateBuilder< IComponent >();
    private static ImmutableArray< IComponent > OrderComponents( IList< IComponent > components ) {
        var builder = _orderedComponentsCache;
        builder.Clear();

        _orderListCache.Clear();
        _orderListCache.AddRange( components.Where( c => c.IsFlagLikeComponent() ) );
        _orderListCache.Sort( ( a, b ) => string.Compare( a.GetType().Name, b.GetType().Name ) );
        _orderListCache.Reverse();

        // Order by alphabetical order.
        builder.AddRange( components.OrderBy( c => c.GetType().Name ) );
        
        foreach ( var flagLikeComponent in _orderListCache ) {
            builder.Remove( flagLikeComponent );
            builder.Insert( 0, flagLikeComponent );
        }

        // Place "EntityName" as the first component.
        if ( builder.FirstOrDefault( c => c is GameObjectReferenceComponent ) is {} gameObjectRef ) {
            builder.Remove( gameObjectRef );
            builder.Insert( 0, gameObjectRef );
        }
        if ( builder.FirstOrDefault( c => c is EntityNameComponent ) is {} entityName ) {
            builder.Remove( entityName );
            builder.Insert( 0, entityName );
        }

        return builder.ToImmutable();
    }

    private static readonly Func< Type, uint > SizeOfType = ( Func< Type, uint > )Delegate.CreateDelegate(
        typeof( Func< Type, uint > ),
        typeof( Marshal ).GetMethod( "SizeOfType", BindingFlags.NonPublic | BindingFlags.Static ) );

    // private static Lazy< GenericMenu > _genericMenu = new ( () => new GenericMenu() );
    private static Rect _genericMenuRect = default;

    private static Lazy< GUIContent > _removeComponentGUIContent = new ( () => new GUIContent() { text = "Remove Component" } );
    private static Lazy< GUIContent > _copyComponentGUIContent = new ( () => new GUIContent() { text = "Copy Component" } );
    private static Lazy< GUIContent > _pasteComponentGUIContent = new ( () => new GUIContent() { text = "Paste Component" } );

    public static bool DrawComponents( List< IComponent > components ) {
        var anyComponentChanged = false;
        var unfoldedComponents = GetUnfoldedComponents();
        var componentMemberSearch = GetComponentMemberSearch();

        int CaculateTotalSizeInBytes() {
            var total = 0;
            foreach ( var component in components ) {
                total += ( int )SizeOfType( component.GetType() );
            }
            return total;
        }

        EditorGUILayout.BeginVertical( GUI.skin.box, Array.Empty< GUILayoutOption >() );
        {
            // EditorGUI.BeginDisabledGroup( true );
            // var entitySizeInKB = ( int )SizeOfType( typeof( Entity ) ) / 1000f;
            // EditorGUILayout.LabelField( $"Entity ({entitySizeInKB:0.000} kb)", EditorStyles.boldLabel );
            // EditorGUI.EndDisabledGroup();
            
            var componentsHeaderRect = EditorGUILayout.BeginHorizontal();
            {
                var sizeInBytes = CaculateTotalSizeInBytes();
                var sizeInKB = sizeInBytes / 1000f;
                EditorGUILayout.LabelField( $"Components ({components.Count}) ({sizeInKB:0.000} kb)", EditorStyles.boldLabel );
                if ( MiniButtonLeft( "▸" ) ) {
                    foreach ( var key in unfoldedComponents.Keys.ToArray() ) {
                        unfoldedComponents[ key ] = false;
                    }
                }

                if ( MiniButtonRight( "▾" ) ) {
                    foreach ( var key in unfoldedComponents.Keys.ToArray() ) {
                        unfoldedComponents[ key ] = true;
                    }
                }
                componentsHeaderRect.height += GUILayoutUtility.GetLastRect().height;
            }
            EditorGUILayout.EndHorizontal();
            
            var e = Event.current;
            if ( e.type == EventType.MouseDown && e.button == 1 ) {
                if ( componentsHeaderRect.Contains( e.mousePosition ) ) {
                    if ( CopyPasteUtils.SourceObject is IComponent ) {
                        var menu = new GenericMenu();
                                    
                        _pasteComponentGUIContent.Value.text = $"Paste {CopyPasteUtils.SourceType.Name}";
                        menu.AddItem( _pasteComponentGUIContent.Value, false, () => {
                            anyComponentChanged |= DoPasteComponent( components );
                        } );
                        
                        _genericMenuRect.x = e.mousePosition.x;
                        _genericMenuRect.y = componentsHeaderRect.yMin;
                        _genericMenuRect.width = 0f;
                        _genericMenuRect.height = 0f;
                        menu.DropDown( _genericMenuRect );
                        e.Use();
                    }
                }
            }

            EditorGUILayout.Space();

            var typeToAdd = DrawAddComponentMenu( components );
            if ( typeToAdd != null ) {
                if ( typeToAdd.IsSubclassOf( typeof( StateMachine ) ) ) {
                    Type tStateMachine = typeof( StateMachineComponent<> );
                    components.Add( Activator.CreateInstance( tStateMachine.MakeGenericType( typeToAdd ) ) as IComponent );
                }
                else if ( typeToAdd.GetInterfaces().Contains( typeof( IInteraction ) ) ) {
                    Type tInteraction = typeof( InteractiveComponent<> );
                    components.Add( Activator.CreateInstance( tInteraction.MakeGenericType( typeToAdd ) ) as IComponent );
                }
                else {
                    components.Add( Activator.CreateInstance( typeToAdd ) as IComponent );
                }
                anyComponentChanged = true;
            }

            if ( MiniButton( "Add Component(new)" ) ) {
                TypeSelectorFancyPopup.Open( "Add Component", typeof( IComponent ), componentType => {
                    var typeToAdd = componentType;
                    if ( typeToAdd != null ) {
                        if ( typeToAdd.IsSubclassOf( typeof( StateMachine ) ) ) {
                            Type tStateMachine = typeof( StateMachineComponent<> );
                            components.Add( Activator.CreateInstance( tStateMachine.MakeGenericType( typeToAdd ) ) as IComponent );
                        }
                        else if ( typeToAdd.GetInterfaces().Contains( typeof( IInteraction ) ) ) {
                            Type tInteraction = typeof( InteractiveComponent<> );
                            components.Add( Activator.CreateInstance( tInteraction.MakeGenericType( typeToAdd ) ) as IComponent );
                        }
                        else {
                            components.Add( Activator.CreateInstance( typeToAdd ) as IComponent );
                        }
                        anyComponentChanged = true;
                    }
                } );
            }

            EditorGUILayout.Space();

            ComponentNameSearchString = SearchTextField( ComponentNameSearchString );

            EditorGUILayout.Space();

            var orderedComponents = OrderComponents( components );
            for ( var i = 0; i < orderedComponents.Length; i++ ) {
                anyComponentChanged |= DrawComponent( unfoldedComponents, componentMemberSearch, components, orderedComponents[ i ] );
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                
                const float padding = 2f;
                const float thickness = 0.5f;
                
                Rect r = EditorGUILayout.GetControlRect( GUILayout.Height( padding + thickness ) );
                r.height = thickness;
                r.y += padding / 2;
                r.x -= 2;
                r.width += 6;
                EditorGUI.DrawRect( r, Color.black );
                
                // EditorGUILayout.Space();
            }
        }
        EditorGUILayout.EndVertical();

        return anyComponentChanged;
    }

    private static bool DrawComponent( Dictionary< Type, bool > unfoldedComponents, Dictionary< Type, string > componentMemberSearch,
                                       List< IComponent > components, IComponent component ) {
        var componentChanged = false;
        var componentType = component.GetType();
        var componentName = componentType.Name.RemoveSuffix( "Component" );
        
        var isStateMachineComponent = false;
        var isInteractiveComponent = false;
        if ( componentType.IsGenericType && componentType.GetGenericTypeDefinition() == typeof( StateMachineComponent<> ) ) {
            componentName = componentType.GetGenericArguments()[ 0 ].Name;
            isStateMachineComponent = true;
        }
        else if ( componentType.IsGenericType && componentType.GetGenericTypeDefinition() == typeof( InteractiveComponent<> ) ) {
            componentName = componentType.GetGenericArguments()[ 0 ].Name;
            isInteractiveComponent = true;
        }
        
        if ( MatchesSearchString( componentName.ToLower(), ComponentNameSearchString.ToLower() ) ) {
            EditorGUILayout.BeginVertical();
            {
                // if (!Attribute.IsDefined(componentType, typeof(DontDrawComponentAttribute)))
                {
                    var memberInfos = componentType.GetPublicMemberInfos();
                    EditorGUILayout.BeginHorizontal();
                    {
                        Rect componentNameRect = default;
                        
                        // flag component
                        if ( memberInfos.Length == 0 && ( !isStateMachineComponent && !isInteractiveComponent ) ) {
                            componentNameRect = EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField( new GUIContent( EditorGUIUtility.IconContent( "sv_icon_dot3_sml" ).image ), EditorStyles.boldLabel, GUILayout.MaxWidth( 10f ) );
                            componentNameRect.height += GUILayoutUtility.GetLastRect().height;
                            EditorGUILayout.LabelField( new GUIContent( componentName, componentType.FullName ), FoldoutNoFoldStyle );
                            componentNameRect.height += GUILayoutUtility.GetLastRect().height;
                            EditorGUILayout.EndHorizontal();
                        }
                        else {
                            componentNameRect = EditorGUILayout.BeginHorizontal();
                            {
                                GUILayout.Space( 11 );
                                unfoldedComponents[ componentType ] = EditorGUILayout.Foldout(
                                    unfoldedComponents[ componentType ],
                                    new GUIContent( componentName, Styles.BooScriptIconTexture,
                                        componentType.FullName ), toggleOnLabelClick: true, FoldoutStyle );
                                componentNameRect.height += GUILayoutUtility.GetLastRect().height;
                            }
                            EditorGUILayout.EndHorizontal();
                            if ( unfoldedComponents[ componentType ] ) {
                                componentMemberSearch[ componentType ] = memberInfos.Length > 5
                                    ? SearchTextField( componentMemberSearch[ componentType ] )
                                    : string.Empty;
                            }
                        }
                        
                        // Handle events
                        var e = Event.current;
                        if ( e.type == EventType.MouseDown && e.button == 1 ) {
                            
                            if ( componentNameRect.Contains( e.mousePosition ) ) {

                                var menu = new GenericMenu();
                                // _removeComponentGUIContent.Value.text = $"Remove {componentType.Name}";
                                menu.AddItem( _removeComponentGUIContent.Value, false, () => {
                                    components.Remove( component );
                                    componentChanged = true;
                                } );
                                menu.AddSeparator( null );

                                _copyComponentGUIContent.Value.text = $"Copy {componentName}";
                                menu.AddItem( _copyComponentGUIContent.Value, false, () => {
                                    CopyPasteUtils.SoftCopy( component, componentType );
                                    // Debug.Log( $"SoftCopy: {componentType.Name}" );
                                    // CopyPasteUtils.Duplicate( component );
                                    // Debug.Log( CopyPasteUtils.SourceType.Name );
                                    // Debug.Log( CopyPasteUtils.SourceObjectJson );
                                } );

                                if ( CopyPasteUtils.SourceObject is IComponent ) {
                                    // _pasteComponentGUIContent.Value.text = "Paste Component";
                                    // menu.AddDisabledItem( _pasteComponentGUIContent.Value );
                                    
                                    _pasteComponentGUIContent.Value.text = $"Paste {CopyPasteUtils.SourceType.Name}";
                                    menu.AddItem( _pasteComponentGUIContent.Value, false, () => {
                                        componentChanged = DoPasteComponent( components );
                                    } );
                                }

                                // _genericMenuRect.x = componentNameRect.x;
                                _genericMenuRect.x = e.mousePosition.x;
                                // _genericMenuRect.y = componentNameRect.yMax;
                                _genericMenuRect.y = componentNameRect.yMin;
                                _genericMenuRect.width = 0f;
                                _genericMenuRect.height = 0f;
                                menu.DropDown( _genericMenuRect );
                                e.Use();
                            }
                        }

                        if ( MiniButton( "-" ) ) {
                            components.Remove( component );
                            componentChanged = true;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    if ( unfoldedComponents[ componentType ] ) {
                        EditorGUI.indentLevel++;
                        // var newComponent = entity.CreateComponent(index, componentType);
                        // component.CopyPublicMemberValues(newComponent);
                        
                        EditorGUI.BeginDisabledGroup( true );
                        var sizeInBytes = SizeOfType( componentType );
                        if ( sizeInBytes < 100 ) {
                            EditorGUILayout.LabelField( $"{SizeOfType( componentType )} bytes " );
                        }
                        else {
                            var sizeInKB = sizeInBytes / 1000f;
                            EditorGUILayout.LabelField( $"{sizeInKB:0.000} kb" );
                        }
                        EditorGUI.EndDisabledGroup();

                        var changed = false;
                        var componentDrawer = GetComponentDrawer( componentType );
                        if ( componentDrawer != null ) {
                            EditorGUI.BeginChangeCheck();
                            {
                                componentDrawer.DrawComponent( component );
                            }
                            changed = EditorGUI.EndChangeCheck();
                        }
                        else {
                            foreach ( var info in memberInfos ) {
                                if ( info.HasAttribute< HideInEditorAttribute >() ) {
                                    continue;
                                }
                                
                                if ( MatchesSearchString( info.Name.ToLower(), componentMemberSearch[ componentType ].ToLower() ) ) {
                                    var memberValue = info.GetValue( component );
                                    var memberType = ( memberValue == null || Nullable.GetUnderlyingType( info.Type ) != null ) ? info.Type : memberValue.GetType();
                                    if ( DrawObjectMember( memberType, info.Name, memberValue, component,info.SetValue ) ) {
                                        changed = true;
                                    }
                                }
                            }
                        }

                        if ( changed ) {
                            componentChanged = true;
                        }
                        
                        EditorGUI.indentLevel--;
                    }
                }

                // else
                // {
                //     EditorGUILayout.LabelField(componentName, "[DontDrawComponent]", EditorStyles.boldLabel);
                // }
            }
            EditorGUILayout.EndVertical();
        }

        return componentChanged;
    }

    private static bool DoPasteComponent( List< IComponent > components ) {
        var changed = false;
        var clonedComponent = CopyPasteUtils.SourceObjectCopy as IComponent;
        // Debug.Log( $"clonedComponent = {clonedComponent} {clonedComponent is IComponent}" );
        if ( clonedComponent is IComponent ) {
            var sameTypeToRemove = -1;
            foreach ( var comp in components ) {
                if ( comp.GetType() == CopyPasteUtils.SourceType ) {
                    sameTypeToRemove = components.IndexOf( comp );
                    break;
                }
            }
            if ( sameTypeToRemove >= 0 ) {
                components.RemoveAt( sameTypeToRemove );
                changed = true;
                // Debug.Log( $"remove same: {CopyPasteUtils.SourceType.Name}" );
            }
            components.Add( clonedComponent );
            changed = true;
                                            
            // Debug.Log( $"cloned: {clonedComponent}" );
        }

        return changed;
    }
    
    public static bool DrawObjectMember( Type memberType, string memberName, object value, object target, Action< object, object > setValue ) {
        if ( value == null && memberType != typeof( System.Type ) ) {
            EditorGUI.BeginChangeCheck();
            {
                var isUnityObject = memberType == typeof( UnityEngine.Object ) ||
                                    memberType.IsSubclassOf( typeof( UnityEngine.Object ) );
                EditorGUILayout.BeginHorizontal();
                {
                    if ( isUnityObject )
                        setValue( target, EditorGUILayout.ObjectField( memberName, ( UnityEngine.Object )value, memberType, true ) );
                    else
                        EditorGUILayout.LabelField( memberName, "null" );

                    // 非抽象/接口类实例创建
                    if ( !memberType.IsAbstract && !memberType.IsInterface ) {
                        if ( MiniButton( $"new {memberType.ToCompilableString().TypeName()}" ) ) {
                            if ( CreateDefault( memberType, out var defaultValue ) ) {
                                setValue( target, defaultValue );
                            }
                        }
                    }
                    else {
                        using ( var check = new EditorGUI.ChangeCheckScope() ) {
				
                            var systemTypes = SystemTypeDrawer.GetAllImplementationsOfCached( memberType );
                            var systemNames = systemTypes
                                              .Select( t => t.FullName.Replace( '.', '/' ) )
                                              .ToArray();

                            var selectTypeIndex = -1;
                            selectTypeIndex = EditorGUILayout.Popup( selectTypeIndex, systemNames, EditorStyles.popup );
                            if ( check.changed ) {
                                if ( CreateDefault( systemTypes[ selectTypeIndex ], out var defaultValue ) ) {
                                    setValue( target, defaultValue );
                                }
                            }
				
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            return EditorGUI.EndChangeCheck();
        }

        if ( !memberType.IsValueType || Nullable.GetUnderlyingType( memberType ) != null ) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
        }

        EditorGUI.BeginChangeCheck();
        {
            var typeDrawer = GetTypeDrawer( memberType );
            if ( typeDrawer != null ) {
                var newValue = typeDrawer.DrawAndGetNewValue( memberType, memberName, value, target );
                setValue( target, newValue );
            }
            else {
                var targetType = target.GetType();
                var shouldDraw = !targetType.ImplementsInterface< IComponent >();
                if ( shouldDraw ) {
                    EditorGUILayout.LabelField( memberName, value.ToString() );

                    var indent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel += 1;

                    EditorGUILayout.BeginVertical();
                    {
                        foreach ( var info in memberType.GetPublicMemberInfos() ) {
                            var mValue = info.GetValue( value );
                            var mType = mValue == null ? info.Type : mValue.GetType();
                            DrawObjectMember( mType, info.Name, mValue, value, info.SetValue );
                            if ( memberType.IsValueType ) {
                                setValue( target, value );
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();

                    EditorGUI.indentLevel = indent;
                }
                else {
                    DrawUnsupportedType( memberType, memberName, value, target );
                }
            }

            if ( !memberType.IsValueType || Nullable.GetUnderlyingType( memberType ) != null ) {
                EditorGUILayout.EndVertical();
                if ( MiniButton( "×" ) ) {
                    setValue( target, null );
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        return EditorGUI.EndChangeCheck();
    }

    public static bool CreateDefault( Type type, out object defaultValue ) {
        
        foreach ( var creator in DefaultInstanceCreators ) {
            if ( creator.CanHandlesType( type ) ) {
                defaultValue = creator.CreateDefault( type );
                return true;
            }
        }

        if ( type == typeof( UnityEngine.Object ) || type.IsSubclassOf( typeof( UnityEngine.Object ) ) ) {
            defaultValue = null;
            return true;
        }
        
        try {
            defaultValue = Activator.CreateInstance( type );
            return true;
        }
        catch ( Exception ) {}

        var typeName = type.ToCompilableString();
        if ( EditorUtility.DisplayDialog(
                "No IDefaultInstanceCreator found",
                "There's no IDefaultInstanceCreator implementation to handle the type '" + typeName + "'.\n" +
                "Providing an IDefaultInstanceCreator enables you to create instances for that type.\n\n" +
                "Do you want to generate an IDefaultInstanceCreator implementation for '" + typeName + "'?\n",
                "Generate",
                "Cancel"
            ) ) {
            // GenerateIDefaultInstanceCreator(typeName);
        }

        defaultValue = null;
        return false;
    }

    static Type DrawAddComponentMenu( Entity entity ) {
        var componentInfos = GetComponentInfos()
                             .Where( kv => !entity.HasComponent( kv.Value.Index ) && !kv.Value.Type.IsInterface && !kv.Value.Type.IsGenericType )
                             .ToArray();
        var componentNames = componentInfos
                             .Select( kv => {
                                 if ( kv.Key.RTGetAttribute< CreateComponentMenuAttribute >( false ) is {} createComponentMenuAttribute ) {
                                     return createComponentMenuAttribute.MenuName;
                                 }
                                 
                                 return kv.Key.FullName.Replace( '.', '/' );
                             } )
                             .ToArray();
        var index = EditorGUILayout.Popup( "Add Component", -1, componentNames );
        return index >= 0
            ? componentInfos[ index ].Value.Type
            : null;
    }

    static Type DrawAddComponentMenu( List< IComponent > components ) {
        var componentInfos = GetComponentInfos()
                             .Where( kv => {
                                 if ( kv.Value.Type.IsInterface || kv.Value.Type.IsGenericType ) {
                                     return false;
                                 }

                                 if ( components != null ) {
                                     foreach ( var component in components ) {
                                         if ( component.GetType() == kv.Value.Type ) {
                                             return false;
                                         }
                                     }
                                 }

                                 return true;
                             } )
                             .ToArray();
        var componentNames = componentInfos
                             .Select( kv => {
                                 if ( kv.Key.RTGetAttribute< CreateComponentMenuAttribute >( false ) is {} createComponentMenuAttribute ) {
                                     return createComponentMenuAttribute.MenuName;
                                 }
                                 
                                 return kv.Key.FullName.Replace( '.', '/' );
                             } )
                             .ToArray();
        var guiContentColor = GUI.contentColor;
        GUI.contentColor = Color.cyan;
        var index = EditorGUILayout.Popup( "Add Component", -1, componentNames );
        GUI.contentColor = guiContentColor;
        return index >= 0
            ? componentInfos[ index ].Value.Type
            : null;
    }

    private static readonly SystemObjectDrawer DefaultObjectDrawer = new ();
    private static readonly Dictionary< int, bool > DefaultObjectDrawerLevelFoldState = new ( 0xf );

    static void DrawUnsupportedType( Type memberType, string memberName, object value, object target ) {
        if ( !DefaultObjectDrawerLevelFoldState.ContainsKey( EditorGUI.indentLevel ) ) {
            DefaultObjectDrawerLevelFoldState.Add( EditorGUI.indentLevel, false );
        }
        
        // EditorGUILayout.LabelField( memberName, $"{memberName} (instance of {value.GetType().Name})" );
        DefaultObjectDrawerLevelFoldState[ EditorGUI.indentLevel ] =
            // Foldout( DefaultObjectDrawerLevelFoldState[ EditorGUI.indentLevel ], $"{memberName} (instance of {value.GetType().Name})" );
            Foldout( DefaultObjectDrawerLevelFoldState[ EditorGUI.indentLevel ], $"{memberName} ({value.GetType().Name})" );

        if ( DefaultObjectDrawerLevelFoldState[ EditorGUI.indentLevel ] ) {
            EditorGUI.indentLevel++;
            DefaultObjectDrawer.DrawAndGetNewValue( memberType, memberName, value, target );
            EditorGUI.indentLevel--;
        }
        
        // if ( EditorGUI.indentLevel < 0xf ) {
        //     DefaultObjectDrawer.DrawAndGetNewValue( memberType, memberName, value, target );
        // }
        // else {
        //     EditorGUI.indentLevel++;
        //     EditorGUILayout.SelectableLabel( "too deep to show." );
        //     EditorGUI.indentLevel--;
        // }
        
        // EditorGUILayout.BeginHorizontal();
        // {
        //     EditorGUILayout.LabelField( memberName, value.ToString() );
        //     if ( MiniButton( "Missing ITypeDrawer" ) ) {
        //         var typeName = memberType.ToCompilableString();
        //         if ( EditorUtility.DisplayDialog(
        //                 "No ITypeDrawer found",
        //                 "There's no ITypeDrawer implementation to handle the type '" + typeName + "'.\n" +
        //                 "Providing an ITypeDrawer enables you draw instances for that type.\n\n" +
        //                 "Do you want to generate an ITypeDrawer implementation for '" + typeName + "'?\n",
        //                 "Generate",
        //                 "Cancel"
        //             ) ) {
        //             // GenerateITypeDrawer(typeName);
        //         }
        //     }
        // }
        // EditorGUILayout.EndHorizontal();
    }

    private static readonly Dictionary<string, string> BuiltInTypesToString = new () {
        { "System.Boolean", "bool" },
        { "System.Byte", "byte" },
        { "System.SByte", "sbyte" },
        { "System.Char", "char" },
        { "System.Decimal", "decimal" },
        { "System.Double", "double" },
        { "System.Single", "float" },
        { "System.Int32", "int" },
        { "System.UInt32", "uint" },
        { "System.Int64", "long" },
        { "System.UInt64", "ulong" },
        { "System.Object", "object" },
        { "System.Int16", "short" },
        { "System.UInt16", "ushort" },
        { "System.String", "string" },
        { "System.Void", "void" }
    };
    
    internal static string ToCompilableString(this Type type)
    {
        if (BuiltInTypesToString.TryGetValue(type.FullName, out var value))
        {
            return value;
        }

        if (type.IsGenericType)
        {
            IEnumerable<string> values = from argType in type.GetGenericArguments()
                                         select argType.ToCompilableString();
            return type.FullName.Split('`')[0] + "<" + string.Join(", ", values) + ">";
        }

        if (type.IsArray)
        {
            return type.GetElementType().ToCompilableString() + "[" + new string(',', type.GetArrayRank() - 1) + "]";
        }

        if (type.IsNested)
        {
            return type.FullName.Replace('+', '.');
        }

        return type.FullName;
    }
    
    private static bool ImplementsInterface<T>(this Type type)
    {
        if (!type.IsInterface)
        {
            return type.GetInterface(typeof(T).FullName) != null;
        }

        return false;
    }
    
    internal static string TypeName(this string fullTypeName)
    {
        int num = fullTypeName.LastIndexOf(".", StringComparison.Ordinal) + 1;
        return fullTypeName.Substring(num, fullTypeName.Length - num);
    }
    
    internal static string RemoveSuffix(this string str, string suffix)
    {
        return str.EndsWith(suffix, StringComparison.Ordinal)
            ? str.Substring(0, str.Length - suffix.Length)
            : str;
    }
    
    public static bool Foldout(bool foldout, string content, int leftMargin = 11)
    {
        return Foldout(foldout, content, null, EditorStyles.foldout, leftMargin);
    }

    public static bool Foldout(bool foldout, string content, Texture texture, GUIStyle style, int leftMargin = 11)
    {
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Space((float)leftMargin);
            foldout = EditorGUILayout.Foldout(foldout, texture != null ? new GUIContent( content, texture ) : new GUIContent( content ), style);
        }
        EditorGUILayout.EndHorizontal();
        return foldout;
    }
    
    public static string SearchTextField(string searchString)
    {
        bool changed = GUI.changed;
        GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
        {
            searchString = GUILayout.TextField(searchString, Styles.ToolbarSearchTextField);
            if (GUILayout.Button(string.Empty, Styles.ToolbarSearchCancelButtonStyle))
            {
                searchString = string.Empty;
            }
        }
        GUILayout.EndHorizontal();
        GUI.changed = changed;
        return searchString;
    }

    public static bool MatchesSearchString( string str, string search ) {
        string[] array = search.Split( new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
        if ( array.Length != 0 ) {
            return array.Any( str.Contains );
        }

        return true;
    }

    public static bool MiniButton(string c)
    {
        return MiniButton(c, EditorStyles.miniButton);
    }

    public static bool MiniButtonLeft(string c)
    {
        return MiniButton(c, EditorStyles.miniButtonLeft);
    }

    public static bool MiniButtonMid(string c)
    {
        return MiniButton(c, EditorStyles.miniButtonMid);
    }

    public static bool MiniButtonRight(string c)
    {
        return MiniButton(c, EditorStyles.miniButtonRight);
    }

    private static bool MiniButton( string c, GUIStyle style ) {
        GUILayoutOption[] array = ( GUILayoutOption[] )( ( c.Length != 1 )
            ? ( ( Array )Array.Empty< GUILayoutOption >() )
            : ( ( Array )new GUILayoutOption[1] { GUILayout.Width( 19f ) } ) );
        bool num = GUILayout.Button( c, style, array );
        if ( num ) {
            GUI.FocusControl( null );
        }

        return num;
    }

}

}
