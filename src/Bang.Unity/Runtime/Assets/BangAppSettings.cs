using System;
using System.Collections.Immutable;
using UnityEngine;
using Gilzoide.EasyProjectSettings;


namespace Bang.Unity {

#if UNITY_EDITOR
	[ProjectSettings("Assets/Resources/BangApp", SettingsPath = "Project/BangApp", Label = "BangApp Settings")]
#else
	[ProjectSettings("Assets/Resources/BangApp", SettingsPath = "Project/BangApp", Label = "BangApp Settings")]
#endif
	public class BangAppSettings : ScriptableObject {
		
		// private const string SettingsPath = "ProjectSettings/BangAppSettings.json";
		//
		// private static BangAppSettings _instance;
		// public static BangAppSettings Instance
		// {
		// 	get
		// 	{
		// 		if (_instance != null)
		// 			return _instance;
		//
		// 		_instance = LoadOrNew();
		//
		// 		return _instance;
		// 	}
		// }

		#region Settings
		public int TargetFps = 60;
		public float FixedUpdateFactor = 2;
		public bool IsVSyncEnabled = false;
		
		public FeatureAsset MainFeatures;
		#endregion
		
		// private static BangAppSettings LoadOrNew()
		// {
		// 	if (File.Exists(SettingsPath))
		// 	{
		// 		var instance = CreateInstance<BangAppSettings>();
		// 		
		// 		JsonUtility.FromJsonOverwrite(File.ReadAllText(SettingsPath), instance);
		// 		return instance;
		// 	}
		// 	else
		// 	{
		// 		var instance = CreateInstance<BangAppSettings>();
		// 		return instance;
		// 	}
		// }
		//
		// public void Save()
		// {
		// 	File.WriteAllText(SettingsPath, JsonUtility.ToJson(_instance));
		// }

		public ImmutableArray< (Type system, bool isActive) > FetchAllSystems() {
			
			var systems = ImmutableArray.CreateBuilder< (Type, bool) >();

			// First, let's add our own systems - easy!
			// systems.AddRange(_systems);

			// Now, let's fetch each of our features...
			if ( MainFeatures != null ) {
				systems.AddRange( MainFeatures.FetchAllSystems( enabled: true ) );
			}

			return systems.ToImmutable();
		}
	}
	
}
