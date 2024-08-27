using System.Threading;


namespace Bang.Unity.Utilities {

///<summary>Simple Thread helper for both runtime and editor</summary>
public static class Threader {

#if UNITY_EDITOR

	//this is to be able to call isPlaying in other threads
	[UnityEditor.InitializeOnLoadMethod]
#if UNITY_2019_3_OR_NEWER

	//the 2nd attribute is used for 'no domain reload'
	[UnityEngine.RuntimeInitializeOnLoadMethod( UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration )]
#endif
	static void Init() {
		applicationIsPlaying = UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
		UnityEditor.EditorApplication.playModeStateChanged -= PlayModeChanged;
		UnityEditor.EditorApplication.playModeStateChanged += PlayModeChanged;
	}

	static void PlayModeChanged( UnityEditor.PlayModeStateChange state ) {
		if ( state == UnityEditor.PlayModeStateChange.EnteredPlayMode ) {
			applicationIsPlaying = true;
		}

		if ( state == UnityEditor.PlayModeStateChange.ExitingPlayMode ) {
			applicationIsPlaying = false;
		}
	}

#else

        static Threader() { applicationIsPlaying = true; }
#endif


	public static bool applicationIsPlaying { get; private set; }
	public static bool isMainThread => Thread.CurrentThread.ManagedThreadId == 1;
	
}

}