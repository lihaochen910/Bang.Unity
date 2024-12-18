using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Bang.Components;
using Bang.Entities;
using Bang.Interactions;
using Bang.StateMachines;
using Bang.Unity.Utilities;
using UnityEditor;
using UnityEngine;


namespace Bang.Unity.Editor {

public static partial class EntityDrawer {

    public record ComponentInfo {
        public int Index;
        public string Name;
        public Type Type;
    }
    
    private static GUIStyle _foldoutNoFoldStyle;
    public static GUIStyle FoldoutNoFoldStyle
    {
        get
        {
            if ( _foldoutNoFoldStyle == null ) {
                _foldoutNoFoldStyle = new GUIStyle( EditorStyles.boldLabel );
                _foldoutNoFoldStyle.fontSize = 15;
                _foldoutNoFoldStyle.fontStyle = FontStyle.Bold;
                _foldoutNoFoldStyle.imagePosition = ImagePosition.ImageLeft;
                // _foldoutStyle.normal = new GUIStyleState { textColor = Color.cyan };
                // _foldoutStyle.hover = new GUIStyleState { textColor = Color.yellow };
                // _foldoutStyle.focused = new GUIStyleState { textColor = Color.green };
            }

            return _foldoutNoFoldStyle;
        }
    }
    
    private static GUIStyle _foldoutStyle;
    public static GUIStyle FoldoutStyle
    {
        get
        {
            if ( _foldoutStyle == null ) {
                _foldoutStyle = new GUIStyle( EditorStyles.foldout );
                _foldoutStyle.fontSize = 15;
                _foldoutStyle.fontStyle = FontStyle.Bold;
                _foldoutStyle.imagePosition = ImagePosition.ImageLeft;
                // _foldoutStyle.normal = new GUIStyleState { textColor = Color.cyan };
                // _foldoutStyle.hover = new GUIStyleState { textColor = Color.yellow };
                // _foldoutStyle.focused = new GUIStyleState { textColor = Color.green };
            }

            return _foldoutStyle;
        }
    }

    static string _componentNameSearchString;

    public static string ComponentNameSearchString
    {
        get => _componentNameSearchString ??= string.Empty;
        set => _componentNameSearchString = value;
    }
    
    private static readonly Lazy<ImmutableArray<Type>> _componentTypes = new(() =>
    {
        return ReflectionHelper.GetAllImplementationsOf<IComponent>()
                               .Where(t => !Attribute.IsDefined(t, typeof(HideInEditorAttribute))
                                           && !typeof(IMessage).IsAssignableFrom(t)
                                           // && !Attribute.IsDefined(t, typeof(RuntimeOnlyAttribute))
                               )
                               .ToImmutableArray();
    });

    public static ImmutableArray<Type> GetAllComponents() => _componentTypes.Value;
    
    private static readonly Lazy< ImmutableArray< Type > > _iteractions = new(() => {
        return ReflectionHelper.GetAllImplementationsOf< IInteraction >()
                               .ToImmutableArray();
    } );

    public static ImmutableArray< Type > GetAllInteractions() => _iteractions.Value;
    
    private static readonly Lazy< ImmutableArray< Type > > _stateMachines = new(() => {
        return ReflectionHelper.GetAllImplementationsOf< StateMachine >()
                               .Where( t => !Attribute.IsDefined( t, typeof( RuntimeOnlyAttribute ) ) )
                               .ToImmutableArray();
    } );

    public static ImmutableArray< Type > GetAllStateMachines() => _stateMachines.Value;

    #region Searchable
    private static readonly Lazy< ImmutableArray< Type > > _searchableComponentTypes = new(() => {
        IEnumerable< Type > types = GetAllComponents()
                                        .Where( t => !t.IsGenericType );
        return types.ToImmutableArray();
    } );

    public static ImmutableArray< Type > GetSearchableComponentTypes() => _searchableComponentTypes.Value;
    
    private static readonly Lazy< ImmutableArray< Type > > _searchableStateMachines = new(() => {
        var builder = ImmutableArray.CreateBuilder< Type >();
        Type tStateMachine = typeof( StateMachineComponent<> );
        foreach ( var t in GetAllStateMachines() ) {
            builder.Add( tStateMachine.MakeGenericType( t ) );
        }
        return builder.ToImmutable();
    } );

    public static ImmutableArray< Type > GetSearchableStateMachines() => _searchableStateMachines.Value;
    
    private static readonly Lazy< ImmutableArray< Type > > _searchableInteractions = new(() => {
        var builder = ImmutableArray.CreateBuilder< Type >();
        Type tInteraction = typeof( InteractiveComponent<> );
        foreach ( var t in GetAllInteractions() ) {
            builder.Add( tInteraction.MakeGenericType( t ) );
        }
        return builder.ToImmutable();
    } );

    public static ImmutableArray< Type > GetSearchableInteractions() => _searchableInteractions.Value;
    
    #endregion
    

    private static readonly Lazy< ComponentsLookup > _componentLookup = new ( World.FindLookupImplementation );
    public static ComponentsLookup GetComponentsLookup() => _componentLookup.Value;
    
    private static readonly Lazy<Dictionary< Type, bool >> _unfoldedComponents = new(() => {
        var unfoldedComponents = new Dictionary< Type, bool >( GetAllComponents().Length + GetAllStateMachines().Length + GetAllInteractions().Length );
        foreach ( var tComponent in GetAllComponents() ) {
            unfoldedComponents[ tComponent ] = true;
        }
        Type tStateMachineMeta = typeof( StateMachineComponent<> );
        foreach ( var tStateMachine in GetAllStateMachines() ) {
            unfoldedComponents[ tStateMachineMeta.MakeGenericType( tStateMachine ) ] = true;
        }
        Type tInteractionMeta = typeof( InteractiveComponent<> );
        foreach ( var tInteraction in GetAllInteractions() ) {
            unfoldedComponents[ tInteractionMeta.MakeGenericType( tInteraction ) ] = true;
        }
        return unfoldedComponents;
    });
    
    private static readonly Lazy<Dictionary< Type, string >> _componentMemberSearch = new(() => {
        var componentMemberSearch = new Dictionary< Type, string >( GetAllComponents().Length + GetAllStateMachines().Length + GetAllInteractions().Length );
        foreach ( var tComponent in GetAllComponents() ) {
            componentMemberSearch[ tComponent ] = string.Empty;
        }
        Type tStateMachineMeta = typeof( StateMachineComponent<> );
        foreach ( var tStateMachine in GetAllStateMachines() ) {
            componentMemberSearch[ tStateMachineMeta.MakeGenericType( tStateMachine ) ] = string.Empty;
        }
        Type tInteractionMeta = typeof( InteractiveComponent<> );
        foreach ( var tInteraction in GetAllInteractions() ) {
            componentMemberSearch[ tInteractionMeta.MakeGenericType( tInteraction ) ] = string.Empty;
        }
        return componentMemberSearch;
    });
    
    private static readonly Lazy<Dictionary< Type, ComponentInfo >> _componentInfos = new(() => {
        var componentLookup = GetComponentsLookup();
        var infos = new Dictionary< Type, ComponentInfo >( GetAllComponents().Length + GetAllStateMachines().Length + GetAllInteractions().Length );
        foreach ( var tComponent in GetAllComponents() ) {
            infos.Add( tComponent, new ComponentInfo
            {
                Index = componentLookup.Id( tComponent ),
                Name = tComponent.Name.RemoveSuffix( "Component" ),
                Type = tComponent
            } );
        }
        foreach ( var tStateMachine in GetAllStateMachines() ) {
            infos.Add( tStateMachine, new ComponentInfo
            {
                Index = componentLookup.Id( tStateMachine ),
                Name = tStateMachine.Name,// tStateMachine.GetGenericArguments()[ 0 ].Name.RemoveSuffix( "Component" ),
                Type = tStateMachine
            } );
        }
        if ( !GetAllInteractions().IsDefaultOrEmpty ) {
            
            foreach ( var tInteraction in GetAllInteractions() ) {
                infos.Add( tInteraction, new ComponentInfo
                {
                    Index = componentLookup.Id( tInteraction ),
                    // Name = tInteraction.GetGenericArguments()[ 0 ].Name,
                    Name = tInteraction.Name,
                    Type = tInteraction
                } );
            }
        }
        return infos;
    });

    public static readonly IDefaultInstanceCreator[] DefaultInstanceCreators;
    public static readonly ITypeDrawer[] TypeDrawers;
    public static readonly IComponentDrawer[] ComponentDrawers;
    
    static EntityDrawer()
    {
        DefaultInstanceCreators = AppDomain.CurrentDomain.GetInstancesOf<IDefaultInstanceCreator>().ToArray();
        TypeDrawers = AppDomain.CurrentDomain.GetInstancesOf<ITypeDrawer>().ToArray();
        ComponentDrawers = AppDomain.CurrentDomain.GetInstancesOf<IComponentDrawer>().ToArray();
    }

    static Dictionary< Type, bool > GetUnfoldedComponents()
    {
        // if (!ContextToUnfoldedComponents.TryGetValue(DefaultContext, out var unfoldedComponents))
        // {
        //     unfoldedComponents = new bool[entity.World.ComponentsLookup.GetAllComponentIndexUnderInterface( typeof( IComponent ) ).Count()];
        //     for (var i = 0; i < unfoldedComponents.Length; i++)
        //         unfoldedComponents[i] = true;
        //
        //     ContextToUnfoldedComponents.Add(DefaultContext, unfoldedComponents);
        // }
        //
        // return unfoldedComponents;
        return _unfoldedComponents.Value;
    }

    static Dictionary< Type, string > GetComponentMemberSearch()
    {
        // if (!ContextToComponentMemberSearch.TryGetValue(DefaultContext, out var componentMemberSearch))
        // {
        //     componentMemberSearch = new string[entity.World.ComponentsLookup.GetAllComponentIndexUnderInterface( typeof( IComponent ) ).Count()];
        //     for (var i = 0; i < componentMemberSearch.Length; i++)
        //         componentMemberSearch[i] = string.Empty;
        //
        //     ContextToComponentMemberSearch.Add(DefaultContext, componentMemberSearch);
        // }
        //
        // return componentMemberSearch;
        return _componentMemberSearch.Value;
    }

    internal static Dictionary< Type, ComponentInfo > GetComponentInfos()
    {
        // if (!ContextToComponentInfos.TryGetValue(DefaultContext, out var infos)) {
        //     var allComponents = entity.World.ComponentsLookup.GetAllComponentIndexUnderInterface( typeof( IComponent ) );
        //     var infosList = new List<ComponentInfo>(allComponents.Count());
        //     foreach ( var componentIndexer in allComponents ) {
        //         if ( componentIndexer.Item1.IsInterface ) {
        //             continue;
        //         }
        //         
        //         infosList.Add(new ComponentInfo
        //         {
        //             Index = componentIndexer.Item2,
        //             Name = componentIndexer.Item1.Name.RemoveSuffix( "Component" ),
        //             Type = componentIndexer.Item1
        //         });
        //     }
        //
        //     infos = infosList.ToArray();
        //     ContextToComponentInfos.Add(DefaultContext, infos);
        // }
        //
        // return infos;
        return _componentInfos.Value;
    }

    static IComponentDrawer GetComponentDrawer(Type type)
    {
        foreach (var drawer in ComponentDrawers)
            if (drawer.CanHandlesType(type))
                return drawer;

        return null;
    }

    public static ITypeDrawer GetTypeDrawer(Type type)
    {
        foreach (var drawer in TypeDrawers)
            if (drawer.CanHandlesType(type))
                return drawer;

        return null;
    }
    
    private static IEnumerable<Type> GetAllTypes(this IEnumerable<Assembly> assemblies)
    {
        var types = new List<Type>();
        foreach (var assembly in assemblies)
        {
            try
            {
                types.AddRange(assembly.GetTypes());
            }
            catch (ReflectionTypeLoadException exception)
            {
                types.AddRange(exception.Types.Where(type => type != null));
            }
        }

        return types;
    }
    
    private static IEnumerable<Type> GetAllTypes(this AppDomain appDomain)
    {
        return appDomain.GetAssemblies().GetAllTypes();
    }
    
    private static IEnumerable<Type> GetNonAbstractTypes<T>(this AppDomain appDomain)
    {
        return appDomain.GetAllTypes().GetNonAbstractTypes<T>();
    }
    
    private static IEnumerable<T> GetInstancesOf<T>(this AppDomain appDomain)
    {
        return appDomain.GetNonAbstractTypes<T>().GetInstancesOf<T>();
    }
    
    private static IEnumerable<T> GetInstancesOf<T>(this IEnumerable<Type> types)
    {
        return from type in types.GetNonAbstractTypes<T>()
               select (T)Activator.CreateInstance(type);
    }
    
    private static IEnumerable<Type> GetNonAbstractTypes<T>(this IEnumerable<Type> types)
    {
        return from type in types
               where !type.IsAbstract
               where type.ImplementsInterface<T>()
               select type;
    }
    
}

}
