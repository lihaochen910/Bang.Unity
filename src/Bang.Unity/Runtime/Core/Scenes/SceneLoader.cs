using System;
using System.Collections.Generic;
using System.Collections.Immutable;


namespace Bang.Unity {

    public class SceneLoader {
        
        /// <summary>
        /// Whether it will load a scene and a load context that has advanced diagnostic
        /// features enabled (which also implies in more processing overhead).
        /// </summary>
        protected readonly bool IsDiagnosticEnabled;

        private Scene? _activeScene;

        public Scene ActiveScene => _activeScene!;

        private readonly Dictionary< Guid, GameScene > _gameScenes = new();

        private readonly Dictionary< Type, Scene > _genericScenes = new();

        public SceneLoader( Scene scene, bool isDiagnosticEnabled ) {
            IsDiagnosticEnabled = isDiagnosticEnabled;

            SetScene( scene );
        }

        public bool ReplaceWorldOnCurrentScene( UnityWorld world, bool disposeWorld ) {
            if ( _activeScene is not GameScene gameScene ) {
                return false;
            }

            return gameScene.ReplaceWorld( world, disposeWorld );
        }

        // public Scene SwitchScene( Guid worldGuid ) {
        //     
        //     throw new NotImplementedException();
        //     
        //     // This is causing *issues* in expectation because we are simply loading the 
        //     // same world (rather than a deserialized version of it) and just calling start again.
        //     // It's not really working out, so I'll comment for now.
        //     //if (_activeScene is GameScene gameScene && gameScene.WorldGuid == worldGuid)
        //     //{
        //     //    _activeScene.Reload();
        //     //    return;
        //     //}
        //
        //     if ( _gameScenes.TryGetValue( worldGuid, out GameScene? scene ) ) {
        //         // Scene was already loaded, just change the active scene.
        //         scene.Reload();
        //
        //         SetScene( scene );
        //         return scene;
        //     }
        //
        //     scene = new GameScene( ImmutableArray< (Type system, bool isActive) >.Empty );
        //     CacheAndSetScene( scene );
        //     return scene;
        // }

        /// <summary>
        /// Switch to a scene of type <typeparamref name="T"/>.
        /// </summary>
        public void SwitchScene< T >() where T : Scene, new() {
            if ( _genericScenes.TryGetValue( typeof( T ), out Scene? scene ) ) {
                // Scene was already loaded, just change the active scene.
                scene.Reload();

                SetScene( scene );
                return;
            }

            CacheAndSetScene( new T() );
        }

        /// <summary>
        /// Switch to <paramref name="scene"/>.
        /// </summary>
        public void SwitchScene( Scene scene ) => SetScene( scene );

        /// <summary>
        /// Initialize current active scene.
        /// </summary>
        public void Initialize() {
            if ( _activeScene is null ) {
                return;
            }

            // RenderContextFlags flags = RenderContextFlags.CustomShaders;
            // if (IsDiagnosticEnabled)
            // {
            //     flags |= RenderContextFlags.Debug;
            // }

            _activeScene.Initialize();
        }

        /// <summary>
        /// Load the content of the current active scene.
        /// </summary>
        public void LoadContent() {
            if ( _activeScene is null ) {
                return;
            }

            // using PerfTimeRecorder recorder = new("Loading Scene Content");
            _activeScene.LoadContent();
        }

        private void CacheAndSetScene( Scene scene ) {
            if ( scene is GameScene gameScene ) {
                _gameScenes.Add( gameScene.WorldGuid, gameScene );
            }
            else {
                _genericScenes.Add( scene.GetType(), scene );
            }

            SetScene( scene );
        }
        
        private void SetScene( Scene scene ) {
            _activeScene?.Unload();
            _activeScene = scene;

            Initialize();
        }

    }

}
