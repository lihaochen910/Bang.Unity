using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Gilzoide.EasyProjectSettings;
using UnityEngine;


namespace Bang.Unity {

public static class Game {
	
	/// <summary>
	/// Use this to set whether diagnostics should be pulled.
	/// </summary>
	public static bool DIAGNOSTICS_MODE = true;
	
	
	public static bool IsPaused { get; private set; }
	
	
	/// <summary>
	/// If set, this is the amount of frames we will skip while rendering.
	/// </summary>
	private static int _freezeFrameCount = 0;
	private static int _freezeFrameCountPending = 0;

	/// <summary>
	/// Time since we have been freezing the frames.
	/// </summary>
	private static double _freezeFrameTime = 0;
	
	
	/// <summary>
	/// Whether the player is currently skipping frames (due to cutscene) and ignore
	/// the time while calling update methods.
	/// </summary>
	private static bool _isSkippingDeltaTimeOnUpdate = false;

	/// <summary>
	/// Whether the player is currently skipping frames (due to cutscene) and ignore
	/// the time while calling update methods.
	/// </summary>
	public static bool IsSkippingDeltaTimeOnUpdate => _isSkippingDeltaTimeOnUpdate;
	
	
	/// <summary>
	/// De time difference between current and last update, scaled by pause and other time scaling. Value is reliable only during the Update().
	/// </summary>
	public static float DeltaTime => Time.deltaTime;
	
	
	/// <summary>
	/// De time difference between current and last update. Value is reliable only during the Update().
	/// </summary>
	public static float UnscaledDeltaTime => Time.unscaledDeltaTime;
	
	
	/// <summary>
	/// Gets the fixed delta time in seconds.
	/// </summary>
	public static float FixedDeltaTime => Time.fixedDeltaTime;
	
	
	/// <summary>
	/// Gets the current scaled elapsed time.
	/// </summary>
	public static float Now => Time.time;
	
	
	/// <summary>
	/// Gets the current unscaled elapsed time.
	/// </summary>
	public static float NowUnscaled => Time.unscaledTime;


	public static ulong Frame => _frame;
	private static ulong _frame;
	
	
	/// <summary>
	/// Initialized in <see cref="LoadContent"/>.
	/// </summary>
	private static SceneLoader? _sceneLoader;
	public static SceneLoader SceneLoader => _sceneLoader;

	public static Scene InitialScene {
		get {
			// if ( !Godot.Engine.IsEditorHint() && _gameData is not EditorDataManager ) {
			// 	return new GameScene( Profile.StartingScene );
			// }
			// else {
			// 	return _editorScene ??= new ();
			// }
			// TODO:
			return null;
		}
	}

	public static bool IsDiagnosticEnabled { get; set; } = true;

	/* *** Public instance fields *** */

	public static Scene? ActiveScene => _sceneLoader?.ActiveScene;
	
	
	private const float LONGEST_TIME_RESET = 5f;
	
	public static float UpdateTime { get; private set; }
	public static float LongestUpdateTime { get; private set; }
	private static float _longestUpdateTimeAt;


	#region Architect Prop

	private static bool _isPlayingGame = false;
	
	/// <summary>
	/// Is playing
	/// NOTE: 在IExitSystem回调触发时, 此标志为false
	/// </summary>
	public static bool IsPlayingGame => _isPlayingGame;


	private static StartPlayGameInfo? _queueStartPlayGame = null;
	
	
	/// <summary>
	/// Whether we are waiting for a save complete operation: do not do any update logic.
	/// </summary>
	private static bool _waitForSaveComplete = false;
	
	
	/// <summary>
	/// Whether it initialized loading textures after the content was loaded.
	/// </summary>
	private static bool _initialiazedAfterContentLoaded = false;


	/// <summary>
	/// Always run update before fixed update. Override this for a different behavior.
	/// </summary>
	public static bool AlwaysUpdateBeforeFixed { get; set; } = false;

	#endregion


	#region Update Prop

	// Update properties.
	private static float _fixedUpdateDelta;
	private static double _targetFixedUpdateTime = 0;

	private static double _scaledElapsedTime = 0;
	private static double _unscaledElapsedTime = 0;
	private static double _absoluteElapsedTime = 0;

	private static double _scaledPreviousElapsedTime = 0;
	private static double _unscaledPreviousElapsedTime = 0;
	
	private static double _lastRenderTime = 0;
	private static double _timeSinceLastDraw = 0;

	private static double _scaledDeltaTime = 0;
	private static double _unscaledDeltaTime = 0;
	
	private static double _previousFrameTime = 0;

	#endregion
	
	
	/// <summary>
	/// Initializes the game by setting up input bindings and configuring initial settings.
	/// Typically overridden by the game implementation.
	/// </summary>
	/// <remarks>
	/// Registers various input buttons for both editor and navigation controls using the MurderInputButtons enumeration.
	/// Configures gamepad axes for UI navigation. Also initializes game assets and refreshes the window size.
	/// Calls the base Initialize method for content loading and initializes the game instance if available.
	/// </remarks>
	public static void Initialize() {
		// Propagate dianostics mode settings.
		World.DIAGNOSTICS_MODE = DIAGNOSTICS_MODE;
		
		LoadContent();
		
#if UNITY_EDITOR
		
#endif
	}


	/// <summary>
	/// Refreshes the game window settings based on the current profile.
	/// </summary>
	/// <remarks>
	/// Refreshes the active scene with new graphics settings, if present.
	/// </remarks>
	public static void RefreshWindow() {
		if ( ProjectSettings.TryLoad< BangAppSettings >( out var bangAppSettings ) ) {
			SetTargetFps( bangAppSettings.TargetFps, bangAppSettings.FixedUpdateFactor );
		}
		else {
			SetTargetFps( 60, 2.0f );
		}
	}


	/// <summary>
	/// Loads game content and initializes it. This includes initializing the sound player, game data, settings, shaders, and initial scene. Also asynchronously loads the initial scene.
	/// </summary>
	private static void LoadContent() {
		
		// _gameData.Initialize( resourcesBinPath );
		
		// Load assets, textures, content, etc
		// _gameData.LoadContent();

		ImmutableArray<(Type system, bool isActive)> systemsToStart = ImmutableArray< (Type system, bool isActive) >.Empty;
		
		if ( ProjectSettings.TryLoad< BangAppSettings >( out var bangAppSettings ) ) {
			if ( bangAppSettings.MainFeatures != null ) {
				systemsToStart = bangAppSettings.FetchAllSystems();
			}
		}
		
		// Window setup goes here
		RefreshWindow();
		
		// Initialize the initial scene.
		_sceneLoader = new SceneLoader( new GameScene( systemsToStart ), IsDiagnosticEnabled );

		_ = LoadSceneAsync( true );

		if ( _sceneLoader.ActiveScene != null ) {
			_isPlayingGame = true;
		}
	}

	private static bool _isForeground;
	public static bool IsForeground => _isForeground;
	
	public static void Update( TimeSpan gameTime ) {

		if ( _waitForSaveComplete && !CanResumeAfterSaveComplete() ) {
			UpdateUnscaledDeltaTime( gameTime.TotalSeconds );
			_targetFixedUpdateTime = _unscaledElapsedTime;

			// Don't do any logic operation yet, we are waiting for the save to complete.
			return;
		}

		if ( ActiveScene is not null && ActiveScene.Loaded && !_initialiazedAfterContentLoaded ) {
			// _gameData.AfterContentLoadedFromMainThread();
			_initialiazedAfterContentLoaded = true;
		}

		UpdateImpl( gameTime );
            
		// Absolute time is ALWAYS updated.
		_absoluteElapsedTime += gameTime.TotalSeconds;

		while (_isSkippingDeltaTimeOnUpdate)
		{
			UpdateImpl(gameTime);

			ActiveScene?.OnBeforeDraw();
		}

		// UNCHECKED: Architect Update goes here?
		// var isEditor = Engine.IsEditorHint() || _gameData is EditorDataManager;
		// if ( isEditor ) {
		// 	if ( _queueStartPlayGame is {} info ) {
		// 		_queueStartPlayGame = null;
		// 		PlayGame( in info );
		// 	}
		// }
		//
		// if ( isEditor ) {
		// 	if ( !DisplayServer.WindowIsFocused() ) {
		// 		_isForeground = true;
		// 	}
		// 	else if ( _isForeground ) {
		// 		// Window is now active and was previously on foreground on the last frame.
		// 		EditorData.ReloadOnWindowForeground();
		// 		
		// 		// Logger.Info( "ReloadOnWindowForeground!" );
		//
		// 		_isForeground = false;
		// 	}
		// }

		_frame++;
	}


	/// <summary>
	/// Implements core update logic, including frame freezing, world transitions, input handling, and time scaling.
	/// </summary>
	private static void UpdateImpl( in TimeSpan gameTime) {
		
		// If this is set, the game has been frozen for some frames.
		// We will simply wait until this returns properly.
		if (_freezeFrameCount > 0)
		{
			_freezeFrameTime += ( float )gameTime.TotalSeconds;

			if (_freezeFrameTime >= _fixedUpdateDelta)
			{
				_freezeFrameCount--;
				_freezeFrameTime = 0;
			}
			return;
		}
		
		DoPendingExitGame();
		DoPendingWorldTransition();
		
		// Time.Update( ( float )gameTime.TotalSeconds );
		
		var startTime = gameTime.TotalSeconds;
		var calculatedDelta = startTime - _previousFrameTime;

		_previousFrameTime = startTime;

		double deltaTime;
		if (_isSkippingDeltaTimeOnUpdate)
		{
			// TODO: TargetElapsedTime
			// deltaTime = TargetElapsedTime.TotalSeconds;
			deltaTime = TimeSpan.Zero.TotalSeconds;
		}
		else
		{
			// TimeTrackerDiagnoostics.Update(calculatedDelta);
			deltaTime = Math.Clamp(calculatedDelta, 0, FixedDeltaTime * 2);
		}

		if (_freezeFrameCountPending > 0)
		{
			deltaTime -= _freezeFrameCountPending * FixedDeltaTime;
			_freezeFrameCountPending = 0;
		}

		UpdateUnscaledDeltaTime(deltaTime);

		double scaledDeltaTime = deltaTime * Time.timeScale;

		if (IsPaused)
		{
			// Make sure we don't update the scaled delta time.
			scaledDeltaTime = 0;
		}

		UpdateScaledDeltaTime(scaledDeltaTime);
		UpdateInputAndScene();
		
		// Check for fixed updates as well! TODO: Do we need to recover from lost frames?
		// See https://github.com/amzeratul/halley/blob/41cd76c927ce59cfcc400f8cdf5f1465e167341a/src/engine/core/src/game/main_loop.cpp
		int maxRecoverFrames = 3;
		while (_unscaledElapsedTime >= _targetFixedUpdateTime)
		{
			// ActiveScene.FixedUpdate();

			// Update previous time after fixed update
			_unscaledPreviousElapsedTime = _unscaledElapsedTime;
			_scaledPreviousElapsedTime = _scaledElapsedTime;

			_targetFixedUpdateTime += _fixedUpdateDelta / Time.timeScale;

			if (maxRecoverFrames-- == 0)
			{
				_targetFixedUpdateTime = (float)_unscaledElapsedTime; // Just slow down the game at this point, sorry.
			}
			else if (AlwaysUpdateBeforeFixed)
			{
				// Update must always run before FixedUpdate
				// Since we are running update again we must reset delta time to zero (no time passed since that update)
				_scaledDeltaTime = 0;
				_unscaledDeltaTime = 0;
				UpdateInputAndScene();
			}
		}
		
		UpdateTime = (float)(gameTime.TotalSeconds - startTime);

		if (Now > _longestUpdateTimeAt + LONGEST_TIME_RESET)
		{
			_longestUpdateTimeAt = Now;
			LongestUpdateTime = 0.0f;
		}

		if (UpdateTime > LongestUpdateTime)
		{
			_longestUpdateTimeAt = Now;
			LongestUpdateTime = UpdateTime;
		}
	}


	/// <summary>
	/// Updates player input and the active scene.
	/// </summary>
	private static void UpdateInputAndScene() {
#if DEBUG
		// Logger.Verify( ActiveScene is not null );
#endif

		// _playerInput.Update();
#if DEBUG
		ActiveScene?.Update();
		ActiveScene?.OnBeforeDraw();
#else
		ActiveScene.Update();
		ActiveScene.OnBeforeDraw();
#endif
	}


	public static void FixedUpdate( TimeSpan gameTime ) {
#if DEBUG
		// Logger.Verify( ActiveScene is not null );
#endif
		
		// Time.FixedUpdate( ( float )gameTime.TotalSeconds );
		
#if DEBUG
		ActiveScene?.FixedUpdate();
#else
		ActiveScene.FixedUpdate();
#endif
	}
	
	
	public static void LateUpdate() {
#if DEBUG
		// Logger.Verify( ActiveScene is not null );
#endif
#if DEBUG
		ActiveScene?.LateUpdate();
#else
		ActiveScene.LateUpdate();
#endif
	}


	public static void Draw() {
		ActiveScene?.Draw();
	}


	public static void DrawImGui() {
#if DEBUG
		// Logger.Verify( ActiveScene is not null );
#endif
		
#if DEBUG
		ActiveScene?.DrawGui();
#else
		ActiveScene.DrawGui();
#endif
	}


	public static void ExitGame() {
		if ( _isPlayingGame ) {
			_isPlayingGame = false;
		}
	}


	/// <summary>
	/// This will pause the game.
	/// </summary>
	public static void Pause() {
		IsPaused = true;
	}


	/// <summary>
	/// This will resume the game.
	/// </summary>
	public static void Resume() {
		// _slowDownScale = 1;
		IsPaused = false;
	}


	private static void UpdateUnscaledDeltaTime( double deltaTime ) {
		_unscaledElapsedTime += deltaTime;
		// _unscaledDeltaTime = deltaTime;
	}

	private static void UpdateScaledDeltaTime( double deltaTime ) {
		_scaledElapsedTime += deltaTime;
		// _scaledDeltaTime = deltaTime;
	}

	private static void SetTargetFps( int fps, float fixedUpdateFactor ) {
		_fixedUpdateDelta = 1f / ( fps / fixedUpdateFactor );
	}
	
	
	private static async Task LoadSceneAsync( bool waitForAllContent ) {
		// Logger.Verify( _sceneLoader is not null );

		// if ( waitForAllContent && _gameData.LoadContentProgress is not null ) {
		// 	await _gameData.LoadContentProgress;
		// }

		// if ( _game is not null ) {
		// 	await _game.LoadContentAsync();
		// 	_game.OnSceneTransition();
		// }

#if RELEASE
		if ( !PlayInEditor
#if GODOT
			 && !Engine.IsEditorHint()
#endif
		   ) {
			EntityBuilder.PushComponentCopyMode( EntityBuilder.ComponentCopyMode.ShallowCopy );
		}
#endif

		_sceneLoader.LoadContent();
		
#if RELEASE
		if ( !PlayInEditor
#if GODOT
			 && !Engine.IsEditorHint()
#endif
		   ) {
			EntityBuilder.PopComponentCopyMode();
		}
#endif
	}


	#region Scenes

	private static Guid? _pendingWorldTransition = default;

    private static UnityWorld? _pendingWorld = default;
    private static bool _disposePendingWorld = true;

    private static bool _pendingExit = false;

	public static bool QueueWorldTransition( Guid world ) {
		if ( _pendingWorldTransition.HasValue ) {
			// Logger.Verify( _pendingWorldTransition.Value == world, "Queue another world transition mid-update?" );
			return false;
		}

		_pendingWorldTransition = world;
		return true;
	}

	/// <summary>
	/// This is called when replacing the world for a current scene.
	/// Happened when transition from two different scenes (already loaded) as a world.
	/// </summary>
	public static bool QueueReplaceWorldOnCurrentScene( UnityWorld world, bool disposeWorld ) {
		if ( _pendingWorldTransition.HasValue ) {
			// Logger.Error( "Queue another world transition mid-update?" );
			return false;
		}

		_pendingWorld = world;
		_disposePendingWorld = disposeWorld;

		return true;
	}

	private static void DoPendingWorldTransition() {
		if ( _pendingWorld is not null ) {
			_sceneLoader?.ReplaceWorldOnCurrentScene( _pendingWorld, _disposePendingWorld );

			_pendingWorld = null;
			_disposePendingWorld = true;

			return;
		}

		if ( !_pendingWorldTransition.HasValue ) {
			return;
		}

		// TODO: Cross fade? Review this flag here!
		// SoundPlayer.Stop(fadeOut: true);

		// Logger.Verify( _sceneLoader is not null );

		// _game?.OnSceneTransition();

		// Unpause on each world transition.
		Resume();

		_sceneLoader.SwitchScene( _pendingWorldTransition.Value );
		_pendingWorldTransition = default;

		// TODO: Fancier loading bar.
		LoadSceneAsync( waitForAllContent: true ).Wait();
	}

	/// <summary>
	/// This queues such that the game exit at the end of the update.
	/// We wait until the end of the update to avoid any access to a world that has been disposed on cleanup.
	/// </summary>
	public static void QueueExitGame() {
		_pendingExit = true;
	}

	public static void DoPendingExitGame() {
		if ( !_pendingExit ) {
			return;
		}

		ExitGame();

		_pendingExit = false;
	}

	#endregion


	#region Architect

	public static void QuitToEditor() {
		// Microsoft.Xna.Framework.Media.MediaPlayer.Stop();
		Resume();

		// SoundPlayer.Stop(fadeOut: false, out _);

		// Logger.Verify( _sceneLoader is not null );

		_isPlayingGame = false;

		if ( ActiveScene is GameScene gameScene ) {
			gameScene.Stop();
			
			// Logger.Verify( _editorScene is not null );

			// _sceneLoader.SwitchScene( _editorScene );

			// Manually set things up in the editor scene.
			// _editorScene.Reload();

			// Open GameScene's WorldAsset.
			// _editorScene.OpenAssetEditor(Data.TryGetAsset<WorldAsset>(gameScene.WorldGuid)!, false);

			// RefreshWindow();
		}

		// Here, let's mock what a real "quit" would do.
		// Manually unload and load all saves.
		// Data.UnloadAllSaves();
		// Data.LoadAllSaves();

		// ( _gameData as EditorDataManager )?.RefreshAfterSave();

		// _playerInput.ClearBinds(MurderInputButtons.PlayGame);
	}

	/// <summary>
	/// Queues an operation for start playing a game. This will be queued and executed in the next
	/// update call.
	/// </summary>
	public static void QueueStartPlayingGame( bool quickplay, Guid? startingScene = null, bool isSimulation = false ) {
		StartPlayGameInfo info = new ( quickplay, isSimulation, startingScene );
		_queueStartPlayGame = info;
	}

	private static void PlayGame( in StartPlayGameInfo info ) {
		// Guid actualStartingScene = info.StartingScene ?? Profile.StartingScene;

		// Data.ResetActiveSave();

		// WorldAsset? world = actualStartingScene != Guid.Empty
		// 	? Data.TryGetAsset< WorldAsset >( actualStartingScene )
		// 	: null;

		// if ( !info.IsQuickplay && world is null ) {
		// 	Logger.Error( "Unable to start the game, please specify a valid starting scene on \"Game Profile\"." );
		// 	return;
		// }

		// if ( world is { HasSystems: false } ) {
		// 	// Logger.Error( $"Unable to start the game, '{world.Name}' has no systems. Add at least one system to the world." );
		// 	return;
		// }

		Resume();
		
		// Logger.Verify( _sceneLoader is not null );

		// SaveWindowPosition();
		_isPlayingGame = true;

		ActiveScene?.RefreshWindow();

		// Data.InitializeAssets();

		bool shouldLoad = true;
		if ( info.IsQuickplay ) {
			shouldLoad = SwitchToQuickPlayScene();
		}
		else {
			// _sceneLoader.SwitchScene( actualStartingScene );
		}

		if ( shouldLoad ) {
			// Make sure we load the save before playing the game.
			// Data.LoadSaveAsCurrentSave(slot: -1);

			if ( ActiveScene is GameScene gameScene ) {
				gameScene.IsSimulate = info.IsSimulation;
			}

			LoadSceneAsync( waitForAllContent: true ).Wait();
		}
		else {
			_isPlayingGame = false;
		}

		// _playerInput.Consume(MurderInputButtons.PlayGame);
		//
		// _playerInput.Bind(MurderInputButtons.PlayGame, (input) =>
		// {
		//     _playerInput.Consume(MurderInputButtons.PlayGame);
		//     QuitToEditor();
		// });
	}

	private static bool SwitchToQuickPlayScene() {
		// Logger.Verify( _sceneLoader is not null );

		// Handle awkward quick save loading.
		// if ( Data.TryGetActiveSaveData() is null ) {
		// 	if ( !Data.LoadSaveAsCurrentSave( slot: -1 ) ) {
		// 		Logger.Warning( "Quick play currently only works on a loaded save." );
		// 		return false;
		// 	}
		// }

		// TODO:
		// if ( EditorSettings.QuickStartScene == Guid.Empty ) {
		// 	Logger.Warning( "Set a Quick Start Scene on Editor Settings first!" );
		// 	return false;
		// }

		// _sceneLoader.SwitchScene( EditorSettings.QuickStartScene );
		return true;
	}

	#endregion
	
	
	#region Save

	/// <summary>
	/// Sets the flag to indicate that the game should wait for the save operation to complete.
	/// </summary>
	public static void SetWaitForSaveComplete() {
		_waitForSaveComplete = true;
	}


	/// <summary>
	/// Determines if the game can resume after a save operation is complete. Returns true if there's no active save data or the save operation has finished.
	/// </summary>
	/// <returns>True if the game can resume, false otherwise.</returns>
	public static bool CanResumeAfterSaveComplete() {
		// _waitForSaveComplete = Data.WaitPendingSaveTrackerOperation;
		_waitForSaveComplete = false;
		return !_waitForSaveComplete;
	}

	#endregion
	
	
	public readonly struct StartPlayGameInfo {
		
		public readonly bool IsQuickplay { get; }
		
		public readonly bool IsSimulation { get; }

		public readonly Guid? StartingScene { get; }

		// public StartPlayGameInfo() {
		// 	IsQuickplay = false;
		// 	IsSimulation = false;
		// 	StartingScene = null;
		// }

		public StartPlayGameInfo( bool isQuickplay, bool isSimulation, Guid? startingScene ) {
			IsQuickplay = isQuickplay;
			IsSimulation = isSimulation;
			StartingScene = startingScene;
		}
	}
}

}
