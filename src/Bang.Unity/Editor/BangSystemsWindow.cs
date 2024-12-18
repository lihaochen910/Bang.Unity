using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bang.Systems;
using Bang.Unity.Graphics;
using UnityEditor;
using UnityEngine;


namespace Bang.Unity.Editor {

	public class BangSystemsWindow : EditorWindow {

		[MenuItem( "Window/Bang/Bang Systems" )]
		public static BangSystemsWindow Open() {
			var window = GetWindow< BangSystemsWindow >( false, "Bang Systems", true );
			window.titleContent.image = EditorGUIUtility.IconContent( "UnityEditor.HierarchyWindow" ).image;
			window.Show();
			return window;
		}
		
		
		private int _selectedWorldId;
		private readonly List< ISystem > _systems = new ( 0xFF );
		private readonly Dictionary< Type, int > _systemTypeToId = new ( 0xFF );
		private Vector2 _scrollPosition;


		private enum TargetView {
			Update = 0,
			LateUpdate = 1,
			FixedUpdate = 2,
			Reactive = 3,
			Render = 5,
			EarlyStartup = 6,
			Startup = 7,
			Total = 8
		}


		/// <summary>
		/// This is the overall time reported per each system.
		/// </summary>
		private readonly double[] _timePerSystems = new double[( int )TargetView.Total];

		private void OnEnable() {
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}

		private void OnDisable() {
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		}

		private void OnPlayModeStateChanged( PlayModeStateChange playModeStateChange ) {
			if ( playModeStateChange is PlayModeStateChange.EnteredPlayMode ) {
				_selectedWorldId = -1;
			}
			
			Repaint();
		}

		private void OnGUI() {
			
			var worlds = ImmutableArray< UnityWorld >.Empty;
			if ( Game.ActiveScene?.World != null ) {
				worlds = worlds.Add( Game.ActiveScene?.World );
			}
			var worldSize = worlds.Length;
			var keys = worlds.Select( x => worlds.IndexOf( x ) ).ToArray();

			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
			{
				if ( worldSize == 0 ) {
					GUILayout.Button( "No World", EditorStyles.toolbarPopup, GUILayout.Width( 100f ) );
				}
				else {
					var displayedOptions = worlds.Select( x => $"World #{worlds.IndexOf( x )}" ).ToArray();
					var id = EditorGUILayout.IntPopup( _selectedWorldId, displayedOptions, keys,
						EditorStyles.toolbarPopup, GUILayout.Width( 100f ) );
					if ( id != _selectedWorldId ) {
						_selectedWorldId = id;
						OnSelectedWorldChanged( worlds[ _selectedWorldId ] );
					}
				}

				GUILayout.FlexibleSpace();
			}

			if ( worlds.Length == 0 ) {
				return;
			}
			
			if ( _selectedWorldId > worlds.Length - 1 || _selectedWorldId < 0 ) {
				_selectedWorldId = 0;
				OnSelectedWorldChanged( null );

				if ( _selectedWorldId <= worlds.Length - 1 &&
					 _selectedWorldId >= 0 ) {
					OnSelectedWorldChanged( worlds[ _selectedWorldId ] );
				}
			}

			var world = worlds[ _selectedWorldId ];

			CalculateAllOverallTime( world );

			_scrollPosition = EditorGUILayout.BeginScrollView(
				_scrollPosition,
				GUILayout.Width( position.width ),
				GUILayout.Height( position.height - 50 )
			);
			
			foreach ( var system in _systems ) {
				EditorGUILayout.BeginHorizontal();
				var isActive = world.IsSystemActive( system.GetType() );
				var newActiveState = EditorGUILayout.Toggle( isActive, GUILayout.Width( 20 ) );
				EditorGUILayout.LabelField(system.GetType().Name);
				// 如果状态发生变化，激活或停用系统
				if ( newActiveState != isActive ) {
					if ( newActiveState ) {
						world.ActivateSystem( system.GetType() ); // 激活系统
					}
					else {
						world.DeactivateSystem( system.GetType() ); // 停用系统
					}
				}
				EditorGUILayout.EndHorizontal();

				// 性能信息
				if ( World.DIAGNOSTICS_MODE ) {
					var systemId = _systemTypeToId[ system.GetType() ];
					EditorGUI.indentLevel++;
					
					if ( system is IEarlyStartupSystem &&
						 world.EarlyStartCounters.TryGetValue( systemId, out var earlyStartupCounter ) ) {
						var overallTime = _timePerSystems[ ( int )TargetView.EarlyStartup ];
						var size = ( float )( earlyStartupCounter.MaximumTime / overallTime * 100 );

						EditorGUILayout.PrefixLabel( $"EarlyStartup: {Math.Round(size)}%" );
						EditorGUI.indentLevel++;
						
						EditorGUILayout.LabelField( $"avg: {PrintTime( earlyStartupCounter.AverageTime )}" );
						EditorGUILayout.LabelField( $"max: {PrintTime( earlyStartupCounter.MaximumTime )}" );
						EditorGUILayout.LabelField( $"avg ent: {PrintTime( earlyStartupCounter.AverageEntities )}" );
						EditorGUI.indentLevel--;
					}
					
					if ( system is IStartupSystem &&
						 world.StartCounters.TryGetValue( systemId, out var startupCounter ) ) {
						var overallTime = _timePerSystems[ ( int )TargetView.Startup ];
						var size = ( float )( startupCounter.MaximumTime / overallTime * 100 );

						EditorGUILayout.PrefixLabel( $"Startup: {Math.Round(size)}%" );
						EditorGUI.indentLevel++;
						
						EditorGUILayout.LabelField( $"avg: {PrintTime( startupCounter.AverageTime )}" );
						EditorGUILayout.LabelField( $"max: {PrintTime( startupCounter.MaximumTime )}" );
						EditorGUILayout.LabelField( $"avg ent: {PrintTime( startupCounter.AverageEntities )}" );
						EditorGUI.indentLevel--;
					}
					
					if ( system is IUpdateSystem &&
						 world.UpdateCounters.TryGetValue( systemId, out var updateCounter ) ) {
						var overallTime = _timePerSystems[ ( int )TargetView.Update ];
						var size = ( float )( updateCounter.MaximumTime / overallTime * 100 );

						EditorGUILayout.PrefixLabel( $"Update: {Math.Round(size)}%" );
						EditorGUI.indentLevel++;
						
						EditorGUILayout.LabelField( $"avg: {PrintTime( updateCounter.AverageTime )}" );
						EditorGUILayout.LabelField( $"max: {PrintTime( updateCounter.MaximumTime )}" );
						EditorGUILayout.LabelField( $"avg ent: {PrintTime( updateCounter.AverageEntities )}" );
						EditorGUI.indentLevel--;
					}
					
					if ( system is IFixedUpdateSystem &&
						 world.FixedUpdateCounters.TryGetValue( systemId, out var fixedUpdateCounter ) ) {
						var overallTime = _timePerSystems[ ( int )TargetView.FixedUpdate ];
						var size = ( float )( fixedUpdateCounter.MaximumTime / overallTime * 100 );

						EditorGUILayout.PrefixLabel( $"FixedUpdate: {Math.Round(size)}%" );
						EditorGUI.indentLevel++;
						
						EditorGUILayout.LabelField( $"avg: {PrintTime( fixedUpdateCounter.AverageTime )}" );
						EditorGUILayout.LabelField( $"max: {PrintTime( fixedUpdateCounter.MaximumTime )}" );
						EditorGUILayout.LabelField( $"avg ent: {PrintTime( fixedUpdateCounter.AverageEntities )}" );
						EditorGUI.indentLevel--;
					}
					
					if ( system is ILateUpdateSystem &&
						 world.LateUpdateCounters.TryGetValue( systemId, out var lateUpdateCounter ) ) {
						var overallTime = _timePerSystems[ ( int )TargetView.LateUpdate ];
						var size = ( float )( lateUpdateCounter.MaximumTime / overallTime * 100 );

						EditorGUILayout.PrefixLabel( $"LateUpdate: {Math.Round(size)}%" );
						EditorGUI.indentLevel++;
						
						EditorGUILayout.LabelField( $"avg: {PrintTime( lateUpdateCounter.AverageTime )}" );
						EditorGUILayout.LabelField( $"max: {PrintTime( lateUpdateCounter.MaximumTime )}" );
						EditorGUILayout.LabelField( $"avg ent: {PrintTime( lateUpdateCounter.AverageEntities )}" );
						EditorGUI.indentLevel--;
					}
					
					if ( system is IDrawSystem &&
						 world.DrawCounters.TryGetValue( systemId, out var drawCounter ) ) {
						var overallTime = _timePerSystems[ ( int )TargetView.Render ];
						var size = ( float )( drawCounter.MaximumTime / overallTime * 100 );

						EditorGUILayout.PrefixLabel( $"Draw: {Math.Round(size)}%" );
						EditorGUI.indentLevel++;
						
						EditorGUILayout.LabelField( $"avg: {PrintTime( drawCounter.AverageTime )}" );
						EditorGUILayout.LabelField( $"max: {PrintTime( drawCounter.MaximumTime )}" );
						EditorGUILayout.LabelField( $"avg ent: {PrintTime( drawCounter.AverageEntities )}" );
						EditorGUI.indentLevel--;
					}
					
					if ( system is IReactiveSystem &&
						 world.ReactiveCounters.TryGetValue( systemId, out var reactiveCounter ) ) {
						var overallTime = _timePerSystems[ ( int )TargetView.Reactive ];
						var size = ( float )( reactiveCounter.MaximumTime / overallTime * 100 );

						EditorGUILayout.PrefixLabel( $"Reactive: {Math.Round(size)}%" );
						EditorGUI.indentLevel++;
						
						EditorGUILayout.LabelField( $"avg: {PrintTime( reactiveCounter.AverageTime )}" );
						EditorGUILayout.LabelField( $"max: {PrintTime( reactiveCounter.MaximumTime )}" );
						EditorGUILayout.LabelField( $"avg ent: {PrintTime( reactiveCounter.AverageEntities )}" );
						EditorGUI.indentLevel--;
					}
					
					EditorGUI.indentLevel--;
				}
			}
			
			EditorGUILayout.EndScrollView();
			
		}

		private void OnSelectedWorldChanged( World world ) {
			_systems.Clear();
			if ( world is null ) {
				return;
			}
			
			foreach ( var system in world.IdToSystem.Values ) {
				_systems.Add( system );
			}
			
			_systemTypeToId.Clear();
			foreach ( var kv in world.IdToSystem ) {
				_systemTypeToId.Add( kv.Value.GetType(), kv.Key );
			}

			_systems.Sort( ( a, b ) => a.GetType().Name.CompareTo( b.GetType().Name ) );
		}

		private void CalculateAllOverallTime( UnityWorld world ) {
			_timePerSystems[ ( int )TargetView.Update ] = world.UpdateCounters.Sum( k => k.Value.MaximumTime );
			_timePerSystems[ ( int )TargetView.LateUpdate ] = world.LateUpdateCounters.Sum( k => k.Value.MaximumTime );
			_timePerSystems[ ( int )TargetView.FixedUpdate ] = world.FixedUpdateCounters.Sum( k => k.Value.MaximumTime );
			_timePerSystems[ ( int )TargetView.Reactive ] = world.ReactiveCounters.Sum( k => k.Value.MaximumTime );
			// _timePerSystems[ ( int )TargetView.PreRender ] = world.PreRenderCounters.Sum( k => k.Value.MaximumTime );
			_timePerSystems[ ( int )TargetView.Render ] = world.DrawCounters.Sum( k => k.Value.MaximumTime );
			// _timePerSystems[ ( int )TargetView.GuiRender ] = world.GuiCounters.Sum( k => k.Value.MaximumTime );
			_timePerSystems[ ( int )TargetView.EarlyStartup ] = world.EarlyStartCounters.Sum( k => k.Value.MaximumTime );
			_timePerSystems[ ( int )TargetView.Startup ] = world.StartCounters.Sum( k => k.Value.MaximumTime );
		}

		private static string PrintTime( double microsseconds ) => ( microsseconds / 1000f ).ToString( "0.00" );
	}

}
