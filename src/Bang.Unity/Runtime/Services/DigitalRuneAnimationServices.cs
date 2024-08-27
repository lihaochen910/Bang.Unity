using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DigitalRune;
using DigitalRune.Animation;
using DigitalRune.Animation.Transitions;
using DigitalRune.Threading;
using UnityEngine;


namespace Bang.Unity.Services {

	public static class DigitalRuneAnimationServices {

		public static bool MultiThreaded => _multiThreaded;
		private static bool _multiThreaded;
		
		private static Action _updateAnimation;
		private static Task _updateAnimationTask;
		
		// The size of the current time step.
		private static TimeSpan _deltaTime;

		
		#region Internal

		internal static AnimationManager AnimationManager => _animationManager;
		private static AnimationManager _animationManager;
		
		
		internal static void Initialize( bool enableMultiThread ) {
			_multiThreaded = enableMultiThread;
			_animationManager = new AnimationManager();
			
			// Initialize delegates for running tasks in parallel.
			// (Creating delegates allocates memory, therefore we do this only once and
			// cache the delegates.)
			_updateAnimation = () => _animationManager.Update( _deltaTime );
		}


		internal static void Tick( float deltaTime ) {
	#if DEBUG
			Debug.Assert( _animationManager is not null, "DRAnimationServices is not initialized correctly!" );
	#endif
			
			_deltaTime = TimeSpan.FromSeconds( deltaTime );

			if ( _multiThreaded ) {
				// In a parallel game loop animation, physics and particles are started at
				// the end of the Update method. The services are now running in parallel. 
				// --> Wait for services to finish.
				_updateAnimationTask.Wait();

				// Now, nothing is running in parallel anymore and we can apply the animations.
				// (This means the animation values are written to the objects and properties
				// that are being animated.)
				_animationManager.ApplyAnimations();
			}
			else {
				// Update animations.
				// (The animation results are stored internally but not yet applied).
				// _profiler.Start("AnimationManger.Update          ");
				_animationManager.Update( _deltaTime );

				// _profiler.Stop();

				// Apply animations.
				// (The animation results are written to the objects and properties that 
				// are being animated. ApplyAnimations() must be called at a point where 
				// it is thread-safe to change the animated objects and properties.)
				// _profiler.Start("AnimationManager.ApplyAnimations");
				_animationManager.ApplyAnimations();

				// _profiler.Stop();
			}

			if ( _multiThreaded ) {
				// Start animation, physics and particle simulation. They will be executed 
				// parallel to the graphics rendering in Draw().
				_updateAnimationTask = Parallel.Start( _updateAnimation );
			}
		}
		
		
		internal static void Uninitialize() {
			_animationManager.SafeDispose();
			_animationManager = null;

			_updateAnimation = null;
			_updateAnimationTask = default;
			_multiThreaded = false;
		}
		
		#endregion


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static AnimationController StartAnimation( ITimeline animation, IAnimatableProperty targetProperty ) {
			return StartAnimation( animation, targetProperty, null );
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static AnimationController StartAnimation( ITimeline animation, IAnimatableProperty targetProperty, AnimationTransition transition ) {
#if DEBUG
			if ( _animationManager is null ) {
				Debug.LogError( "AnimationManager not initialized." );
				return default;
			}
#endif
			return _animationManager.StartAnimation( animation, targetProperty, transition );
		}
		
		
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static AnimationController StartAnimation( ITimeline animation, IAnimatableObject targetObject ) {
			return StartAnimation( animation, targetObject, null );
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static AnimationController StartAnimation( ITimeline animation, IAnimatableObject targetObject, AnimationTransition transition ) {
#if DEBUG
			if ( _animationManager is null ) {
				Debug.LogError( "AnimationManager not initialized." );
				return default;
			}
#endif
			return _animationManager.StartAnimation( animation, targetObject, transition );
		}
		
		
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static AnimationController StartAnimation( ITimeline animation, IEnumerable<IAnimatableObject> targetObjects ) {
			return StartAnimation( animation, targetObjects, null );
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static AnimationController StartAnimation( ITimeline animation, IEnumerable<IAnimatableObject> targetObjects, AnimationTransition transition ) {
#if DEBUG
			if ( _animationManager is null ) {
				Debug.LogError( "AnimationManager not initialized." );
				return default;
			}
#endif
			return _animationManager.StartAnimation( animation, targetObjects, transition );
		}
	}

}
