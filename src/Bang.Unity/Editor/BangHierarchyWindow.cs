using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bang.Contexts;
using Bang.Systems;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;


namespace Bang.Unity.Editor {

    public class BangHierarchyWindow : EditorWindow {

	    [MenuItem( "Window/Bang/Bang Hierarchy" )]
	    public static BangHierarchyWindow Open() {
		    var window = GetWindow< BangHierarchyWindow >( false, "Bang Hierarchy", true );
		    window.titleContent.image = EditorGUIUtility.IconContent( "UnityEditor.HierarchyWindow" ).image;
		    window.Show();
		    return window;
	    }


        [Filter]
        private class TestPlaceHolderSystem : IStartupSystem {
            public void Start( Context context ) {}
        }

        [MenuItem( "Tools/Create 10w Entities" )]
        public static void TestCreateEntities() {
            var diagnosticsMode = World.DIAGNOSTICS_MODE;
            World.DIAGNOSTICS_MODE = true;
            List<(ISystem, bool)> systemInstances = new ( 1 );
            systemInstances.Add( ( new TestPlaceHolderSystem(), true ) );
            using var world = new UnityWorld( systemInstances );
            for ( var i = 0; i < 100_000; i++ ) {
                _ = world.AddEntity();
            }
            World.DIAGNOSTICS_MODE = diagnosticsMode;
            Debug.Log( "Done." );
        }
	    
	    private int _selectedWorldId;
        private HierarchyTreeView _treeView;
        private TreeViewState _treeViewState;

        void OnEnable()
        {
            _treeViewState = new TreeViewState();
            _treeView = new HierarchyTreeView(_treeViewState);

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        void OnDisable()
        {
            _treeView.Dispose();
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            _treeView.SetSelection(Array.Empty<int>());
            Repaint();
        }

        void OnGUI()
        {
            // var worlds = World.Worlds.Where(x => x != null).ToDictionary(x => x.Id, x => x);
            var worlds = ImmutableArray< World >.Empty;
            if ( Game.ActiveScene?.World != null ) {
                worlds = worlds.Add( Game.ActiveScene.World );
            }
            var worldSize = worlds.Length;
            var keys = worlds.Select( x => worlds.IndexOf( x ) ).ToArray();

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (worldSize == 0)
                {
                    GUILayout.Button("No World", EditorStyles.toolbarPopup, GUILayout.Width(100f));
                }
                else
                {
                    var displayedOptions = worlds.Select(x => $"World #{worlds.IndexOf( x )}").ToArray();
                    var id = EditorGUILayout.IntPopup(_selectedWorldId, displayedOptions, keys, EditorStyles.toolbarPopup, GUILayout.Width(100f));
                    if (id != _selectedWorldId)
                    {
                        _treeView.SetSelection(Array.Empty<int>());
                        _selectedWorldId = id;
                    }
                }

                GUILayout.FlexibleSpace();
            }

            if ( worlds.Length == 0 ) {
                return;
            }
            if ( _selectedWorldId > worlds.Length - 1 || _selectedWorldId < 0 ) {
                _selectedWorldId = 0;
            }

            _treeView.SetWorld(worlds[_selectedWorldId]);
            var treeViewRect = EditorGUILayout.GetControlRect(false, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            _treeView.OnGUI(treeViewRect);
        }

    }

}
