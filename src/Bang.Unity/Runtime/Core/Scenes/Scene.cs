using System;
using System.Threading.Tasks;


namespace Bang.Unity {

public abstract class Scene : IDisposable
{
    /// <summary>
    /// Context renderer unique to this scene.
    /// </summary>
    // public RenderContext? RenderContext { get; private set; }

    public bool Loaded { get; private set; }

    public abstract UnityWorld? World { get; }

    protected bool _calledStart = false;

    /// <summary>
    /// Used to track events when the window (UI) refreshes.
    /// </summary>
    private Action? _onRefreshWindow = default;
    
    public virtual void Initialize() {
        // RenderContext = Game.CreateRenderContext();
        // RenderContext.Initialize();
    }

    public void LoadContent() {
        LoadContentImpl();

        _calledStart = false;
        Loaded = true;
    }

    public virtual void LoadContentImpl() {}

    /// <summary>
    /// Reload the active scene.
    /// </summary>
    public void Reload() {
        _calledStart = false;

        ReloadImpl();
    }

    public void Resume() {}

    /// <summary>
    /// Rests the current scene temporarily.
    /// </summary>
    public void Suspend() {}

    public virtual void ReloadImpl() {}
    public virtual void ResumeImpl() {}
    public virtual void SuspendImpl() {}
    public virtual Task UnloadAsyncImpl() => Task.CompletedTask;


    /// <summary>
    /// Refresh the window size, updating the camera and render context.
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    public virtual void RefreshWindow() {
        // Logger.Verify( RenderContext is not null, "RenderContext should not be null at this point." );

        // Point windowSize = new Point(graphics.Viewport.Width, graphics.Viewport.Height);
        bool changed = false;

        // switch (Game.Profile.ResizeMode)
        // {
        //     case GameProfile.WindowResizeMode.None:
        //         // Ignore the window size, use the game size from settings.
        //         changed = RenderContext.RefreshWindow(graphics, new Point(settings.GameWidth, settings.GameHeight), new Vector2(settings.GameScale, settings.GameScale));
        //         break;
        //     case GameProfile.WindowResizeMode.Stretch:
        //         // Stretch everything, ignoring aspect ratio.
        //         Vector2 stretchedScale = new Vector2(windowSize.X / (float)settings.GameWidth, windowSize.Y / (float)settings.GameHeight);
        //         changed = RenderContext.RefreshWindow(graphics, windowSize, stretchedScale);
        //         break;
        //     case GameProfile.WindowResizeMode.Letterbox:
        //         // Letterbox the game, keeping aspect ratio with some allowance.
        //         (Point letterboxSize, float letterboxScale) = Calculator.LetterboxSize(
        //             windowSize, 
        //             new Point(settings.GameWidth, settings.GameHeight),
        //             settings.PositiveApectRatioAllowance,
        //             settings.NegativeApectRatioAllowance, 
        //             settings.SnapToInteger);
        //         changed = RenderContext.RefreshWindow(graphics, letterboxSize, new Vector2(letterboxScale));
        //         break;
        //     case GameProfile.WindowResizeMode.Crop:
        //         // Maintain the scale, expanding or shrinking the camera to fit the window.
        //         Vector2 scale = new Vector2(settings.GameScale, settings.GameScale);
        //         changed = RenderContext.RefreshWindow(graphics, windowSize, scale);
        //         break;
        //     default:
        //         throw new Exception($"Invalid window resize mode ({Game.Profile.ResizeMode}).");
        // }

        if ( changed ) {
            _onRefreshWindow?.Invoke();
        }
    }

    public virtual void Start() {
        RefreshWindow();

        World?.Start();
        _calledStart = true;
    }

    public virtual void Update() {
        if ( !_calledStart ) {
            Start();
        }

        if ( World != null ) {
            World.Update();
        }
    }

    public virtual void LateUpdate() {
        if ( World != null ) {
            World.LateUpdate();
        }
    }

    public virtual void FixedUpdate() {
        if ( !_calledStart ) {
            Start();
        }

        World?.FixedUpdate();
    }

    public virtual void OnBeforeDraw() {
        if ( World is null ) {
            return;
        }

        if ( !_calledStart ) {
            return;
        }

        // World?.PreDraw();
    }

    public virtual void Draw() {
        if ( World is null ) {
            return;
        }

        if ( !_calledStart ) {
            return;
        }

        // Logger.Verify( RenderContext is not null );

        OnBeforeDraw();

        // RenderContext.Begin();
        // World?.Draw( RenderContext );
        // RenderContext.End();
    }

    /// <summary>
    /// Scenes that would like to implement a Gui should use this method.
    /// </summary>
    public virtual void DrawGui() {
        // Logger.Verify( RenderContext is not null );

        if ( !_calledStart ) {
            return;
        }

        // World?.DrawGui( RenderContext );
    }

    /// <summary>
    /// This will trigger UI refresh operations.
    /// </summary>
    public void AddOnWindowRefresh( Action notification ) {
        _onRefreshWindow += notification;
    }

    /// <summary>
    /// This will reset all watchers of trackers.
    /// </summary>
    public void ResetWindowRefreshEvents() {
        _onRefreshWindow = null;
    }

    public void Unload() {
        // RenderContext?.Unload();
        ResetWindowRefreshEvents();

        _ = UnloadAsyncImpl();
    }

    public void Dispose() {
        // RenderContext?.Dispose();
    }

}

}
