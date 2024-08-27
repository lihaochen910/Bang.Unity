// using System.Collections.Generic;
// using UnityEditor;
// using UnityEngine;
// using UnityEngine.UIElements;
//
//
// namespace Bang.Unity.Editor;
//
// public class BangAppSettingsProvider : SettingsProvider {
// 	
// 	private static class Styles
// 	{
// 		public static readonly GUIStyle VerticalStyle;
//
// 		static Styles()
// 		{
// 			VerticalStyle = new GUIStyle(EditorStyles.inspectorFullWidthMargins);
// 			VerticalStyle.margin = new RectOffset(10, 10, 10, 10);
// 		}
// 	}
//
//
// 	public BangAppSettingsProvider( string path, SettingsScope scopes, IEnumerable< string > keywords = null )
// 		: base( path, scopes, keywords ) {}
//
// 	[SettingsProvider]
// 	public static SettingsProvider Create()
// 		=> new BangAppSettingsProvider("Project/BangApp", SettingsScope.Project, new []{ "Analyzer", "C#", "csproj", "Project" } );
//
// 	public override void OnActivate(string searchContext, VisualElement rootElement)
// 	{
// 		Initialize();
// 		base.OnActivate(searchContext, rootElement);
// 	}
//
// 	public override void OnGUI(string searchContext)
// 	{
// 		using (new EditorGUILayout.VerticalScope(Styles.VerticalStyle))
// 		{
// 			var settings = BangAppSettings.Instance;
//
// 			EditorGUILayout.BeginHorizontal();
// 			EditorGUILayout.LabelField( "Main Features" );
// 			settings.MainFeatures = EditorGUILayout.TextField( settings.MainFeatures );
// 			EditorGUILayout.EndHorizontal();
// 			
// 		}
//
// 		if (GUI.changed)
// 		{
// 			BangAppSettings.Instance.Save();
// 		}
// 	}
//
// 	private void Initialize()
// 	{
// 		
// 	}
// }
