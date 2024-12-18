using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using PlayerLoopType = UnityEngine.PlayerLoop;


namespace Bang.Unity {

    public static class BangLoopRunners {
        public struct BangInitialization {}
        public struct BangEarlyUpdate {}
        public struct BangFixedUpdate {}
        public struct BangPreUpdate {}
        public struct BangUpdate {}
        public struct BangPreLateUpdate {}
        public struct BangPostLateUpdate {}
        public struct BangTimeUpdate {}
    }


    // public enum PlayerLoopTiming {
	   //  Initialization = 0,
	   //  EarlyUpdate = 1,
	   //  FixedUpdate = 2,
	   //  PreUpdate = 3,
	   //  Update = 4,
	   //  PreLateUpdate = 5,
	   //  PostLateUpdate = 6,
	   //  TimeUpdate = 7
    // }


    internal static class PlayerLoopHelper {
	    
	    public static event Action OnInitialization;
        public static event Action OnEarlyUpdate;
        public static event Action OnFixedUpdate;
        public static event Action OnPreUpdate;
        public static event Action OnUpdate;
        public static event Action OnPreLateUpdate;
        public static event Action OnPostLateUpdate;
        public static event Action OnTimeUpdate;

        private static bool _initialized;
        private static bool _eventsInitialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void RuntimeInit() {
    #if !UNITY_EDITOR
            Debug.Log( "[Bang.Unity] PlayerLoopHelper::RuntimeInit()" );
    #endif
            
            Game.Initialize();
            
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    #endif
            
            if (!_eventsInitialized)
            {
                // // Initialize JobArchChunkHandle
                // OnUpdate += JobArchChunkHandle.CheckHandles;

                // Initialize Apps
                // OnInitialization += Game.Initialize;
                // OnEarlyUpdate += SystemRunner.EarlyUpdate.Run;
                OnFixedUpdate += () => Game.FixedUpdate( TimeSpan.FromSeconds( Time.fixedDeltaTime ) );
                // OnPreUpdate += SystemRunner.PreUpdate.Run;
                OnUpdate += () => Game.Update( TimeSpan.FromSeconds( Time.deltaTime ) );
                // OnPreLateUpdate += SystemRunner.PreLateUpdate.Run;
                OnPostLateUpdate += Game.LateUpdate;
                // OnTimeUpdate += SystemRunner.TimeUpdate.Run;

                _eventsInitialized = true;
            }
            
    #if UNITY_EDITOR
            var domainReloadDisabled = UnityEditor.EditorSettings.enterPlayModeOptionsEnabled &&
                                       UnityEditor.EditorSettings.enterPlayModeOptions.HasFlag( UnityEditor.EnterPlayModeOptions.DisableDomainReload );
            if ( !domainReloadDisabled && _initialized ) {
                return;
            }
    #else
            if ( _initialized ) {
                return;
            }
    #endif
            
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            Initialize( ref playerLoop );
        }


#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void OnProjectLoadedInEditor() {
            // Debug.Log( "[InitializeOnLoadMethod] called." );
            if ( Game.ActiveScene is not EditorScene ) {
                Game.InitializeEditor();
            }
        }
#endif
        
        
        private static void Initialize( ref PlayerLoopSystem playerLoop ) {
            _initialized = true;
            var newLoop = playerLoop.subSystemList.ToArray();

            InsertLoop( newLoop, typeof( PlayerLoopType.Initialization ), typeof( BangLoopRunners.BangInitialization ),
                static () => OnInitialization?.Invoke() );
            InsertLoop( newLoop, typeof( PlayerLoopType.EarlyUpdate ), typeof( BangLoopRunners.BangEarlyUpdate ),
                static () => OnEarlyUpdate?.Invoke() );
            InsertLoop( newLoop, typeof( PlayerLoopType.FixedUpdate ), typeof( BangLoopRunners.BangFixedUpdate ),
                static () => OnFixedUpdate?.Invoke() );
            InsertLoop( newLoop, typeof( PlayerLoopType.PreUpdate ), typeof( BangLoopRunners.BangPreUpdate ),
                static () => OnPreUpdate?.Invoke() );
            InsertLoop( newLoop, typeof( PlayerLoopType.Update ), typeof( BangLoopRunners.BangUpdate ),
                static () => OnUpdate?.Invoke() );
            InsertLoop( newLoop, typeof( PlayerLoopType.PreLateUpdate ), typeof( BangLoopRunners.BangPreLateUpdate ),
                static () => OnPreLateUpdate?.Invoke() );
            InsertLoop( newLoop, typeof( PlayerLoopType.PostLateUpdate ), typeof( BangLoopRunners.BangPostLateUpdate ),
                static () => OnPostLateUpdate?.Invoke() );
            InsertLoop( newLoop, typeof( PlayerLoopType.TimeUpdate ), typeof( BangLoopRunners.BangTimeUpdate ),
                static () => OnTimeUpdate?.Invoke() );

            playerLoop.subSystemList = newLoop;
            PlayerLoop.SetPlayerLoop( playerLoop );
        }

        static void InsertLoop( PlayerLoopSystem[] loopSystems, Type loopType, Type loopRunnerType, PlayerLoopSystem.UpdateFunction updateDelegate ) {
            var i = FindLoopSystemIndex( loopSystems, loopType );
            ref var loop = ref loopSystems[ i ];
            loop.subSystemList = InsertRunner( loop.subSystemList, loopRunnerType, updateDelegate );
        }

        static int FindLoopSystemIndex( PlayerLoopSystem[] playerLoopList, Type systemType ) {
            for ( int i = 0; i < playerLoopList.Length; i++ ) {
                if ( playerLoopList[ i ].type == systemType ) {
                    return i;
                }
            }

            throw new Exception( "Target PlayerLoopSystem does not found. Type:" + systemType.FullName );
        }

        static PlayerLoopSystem[] InsertRunner( PlayerLoopSystem[] subSystemList, Type loopRunnerType, PlayerLoopSystem.UpdateFunction updateDelegate ) {
            var source = subSystemList.Where( x => x.type != loopRunnerType ).ToArray();
            var dest = new PlayerLoopSystem[source.Length + 1];
            Array.Copy( source, 0, dest, 1, source.Length );
            dest[ 0 ] = new PlayerLoopSystem { type = loopRunnerType, updateDelegate = updateDelegate };
            return dest;
        }

#if UNITY_EDITOR
        private static void OnPlayModeStateChanged( PlayModeStateChange playModeStateChange ) {
            if ( playModeStateChange is PlayModeStateChange.ExitingPlayMode ) {
                // Debug.Log( $"PlayerLoopHelper::OnPlayModeStateChanged {playModeStateChange}" );
                Game.ExitGame();
                Game.SceneLoader.SwitchScene( null );
                Game.SwitchToEditorScene();
                
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            }

            // // TODO: Create EditorModeWorld => FetchEditorSystems => Start!
            // if ( playModeStateChange is PlayModeStateChange.EnteredEditMode ) {
            //     Game.SwitchToEditorScene();
            // }
            //
            // // TODO: Clean EditorModeWorld
            // if ( playModeStateChange is PlayModeStateChange.ExitingEditMode ) {
            //     
            // }
            
        }
    #endif
    }

}
