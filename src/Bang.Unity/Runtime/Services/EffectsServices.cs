using Bang.Entities;
using Bang.Unity.Effects;
using UnityEngine;


namespace Bang.Unity.Services {

	public static class EffectsServices {

		/// <summary>
		/// Add an entity which will apply a "fade-in" effect. Darkening the screen to black.
		/// </summary>
		public static void FadeIn( World world, float time, Color color, bool destroyAfterFinished = true ) {
			if ( Game.IsSkippingDeltaTimeOnUpdate ) {
				return;
			}

			Entity e;
			if ( world.TryGetUniqueEntity< FadeScreenWithSolidColorComponent >() is {} fadeScreenEntity ) {
				e = fadeScreenEntity;
			}
			else {
				e = world.AddEntity();
			}
			
			e.SetFadeScreenWithSolidColor( new( color, FadeType.In, time, destroyAfterFinished ) );
		}

		/// <summary>
		/// Add an entity which will apply a "fade-out" effect. Clearing the screen.
		/// </summary>
		public static void FadeOut( World world, float duration, Color color, bool destroyAfterFinished = true ) {
			if ( Game.IsSkippingDeltaTimeOnUpdate ) {
				return;
			}

			Entity e;
			if ( world.TryGetUniqueEntity< FadeScreenWithSolidColorComponent >() is {} fadeScreenEntity ) {
				e = fadeScreenEntity;
			}
			else {
				e = world.AddEntity();
			}

			// if ( bufferDrawFrames > 0 ) {
			// 	// With buffer frames we must wait until we get Game.Now otherwise we will get an value
			// 	// specially at lower frame rates
			// 	e.SetFadeScreen( new(FadeType.Out, delay, duration, color, string.Empty, 0, bufferDrawFrames) );
			// }
			// else {
			// 	e.SetFadeScreen( new(FadeType.Out, Game.NowUnscaled + delay, duration, color, string.Empty, 0,
			// 		bufferDrawFrames) );
			// }
			
			e.SetFadeScreenWithSolidColor( new( color, FadeType.Out, duration, destroyAfterFinished ) );
		}

	}

}
