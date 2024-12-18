// using System;
// using Unity.Profiling;
// using Unity.Profiling.Editor;
// using UnityEditor;
// using UnityEngine;
// using UnityEngine.Serialization;
//
//
// namespace Bang.Unity.Editor {
//
// 	[ProfilerModuleMetadata("Bang Systems")] 
// 	public class BangSystemsDiagnosticsEditor : ProfilerModule {
//
// 		internal static class Styles {
//
// 			public static readonly GUIContent ProfilerRecordOff = EditorGUIUtility.TrIconContent( "Record Off", "Record profiling information" );
// 			public static readonly GUIContent ProfilerRecordOn = EditorGUIUtility.TrIconContent( "Record On", "Record profiling information" );
//
// 		}
//
// 		// [MenuItem( "Window/Bang/Bang Diagnostics" )]
// 		// public static BangSystemsDiagnosticsEditor Open() {
// 		// 	var window = GetWindow< BangSystemsDiagnosticsEditor >( false, "Bang Diagnostics", true );
// 		// 	window.titleContent.image = EditorGUIUtility.IconContent( "UnityEditor.HierarchyWindow" ).image;
// 		// 	window.Show();
// 		// 	return window;
// 		// }
// 		
// 		
// 		// For keeping correct "Recording" state on window maximizing
// 		[SerializeField]
// 		private bool _recording;
//
// 		static readonly ProfilerCounterDescriptor[] k_Counters = new ProfilerCounterDescriptor[]
// 		{
// 			// new ProfilerCounterDescriptor(GameStatistics.TankTrailParticleCountName, ProfilerCategory.Scripts),
// 			// new ProfilerCounterDescriptor(GameStatistics.ShellExplosionParticleCountName, ProfilerCategory.Scripts),
// 			// new ProfilerCounterDescriptor(GameStatistics.TankExplosionParticleCountName, ProfilerCategory.Scripts)
// 		};
// 		
// 		public BangSystemsDiagnosticsEditor() : base( k_Counters, defaultChartType: ProfilerModuleChartType.StackedTimeArea ) {
// 			_recording = true;
// 		}
//
// 		private void OnGUI() {
// 			DrawMainToolbar();
// 		}
//
// 		private float DrawMainToolbar() {
// 			GUILayout.BeginHorizontal( EditorStyles.toolbar );
// 			
// 			// Record
// 			var profilerEnabled = GUILayout.Toggle( _recording,
// 				_recording ? Styles.ProfilerRecordOn : Styles.ProfilerRecordOff, EditorStyles.toolbarButton );
// 			if ( profilerEnabled != _recording ) {
// 				SetRecordingEnabled( profilerEnabled );
// 			}
//
// 			GUILayout.EndHorizontal();
//
// 			return EditorStyles.toolbar.fixedHeight;
// 		}
//
// 		internal void SetRecordingEnabled( bool profilerEnabled ) {
// 			_recording = profilerEnabled;
//
// 			// SessionState.SetBool(kProfilerEnabledSessionKey, profilerEnabled);
// 			// if (ProfilerUserSettings.rememberLastRecordState)
// 			// 	EditorPrefs.SetBool(kProfilerEnabledSessionKey, profilerEnabled);
// 			//
// 			// if (profilerEnabled)
// 			// 	ProfilerWindowAnalytics.StartCapture();
// 			//
// 			// recordingStateChanged?.Invoke(m_Recording);
//
// 			// Repaint();
// 		}
//
// 	}
// 	
// }
