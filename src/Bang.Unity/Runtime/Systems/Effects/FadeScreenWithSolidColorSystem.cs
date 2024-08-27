using System;
using System.Collections.Immutable;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using Bang.Unity.Graphics;
using UnityEngine;
using Object = UnityEngine.Object;


namespace Bang.Unity.Effects {
	
	[Watch( typeof( FadeScreenWithSolidColorComponent ) )]
	[Filter( typeof( FadeScreenWithSolidColorComponent ) )]
	public class FadeScreenWithSolidColorSystem : IStartupSystem, IExitSystem, IReactiveSystem, IUpdateSystem, IGuiSystem {

		private GUIStyle _backgroundStyle;
		private Texture2D _fadeTexture;
		private const int FadeGUIDepth = -1000;
		
		private float _fadeInTime = -1;
		private float _fadeOutTime = -1;

		private Color _color;
		private float _duration;

		private float _currentAlpha = 0;

		// private static Color FullyTransparrentColor = new ( 0, 0, 0, 0 );
		
		public void Start( Context context ) {
			_fadeTexture = new Texture2D( 1, 1 );
			_backgroundStyle = new GUIStyle();
			_backgroundStyle.normal.background = _fadeTexture;
		}

		public void Exit( Context context ) {
			_backgroundStyle.normal.background = null;
			Object.Destroy( _fadeTexture );
		}
		
		public void Update( Context context ) {
			// if ( context.World.TryGetUniqueEntityFadeScreenWithSolidColor() is not {} fadeScreenWithSolidColorEntity ) {
			// 	return;
			// }
			
			// // if the current color of the screen is not equal to the desired color: keep fading!
			// if ( fadeScreenWithSolidColor.CurrentScreenOverlayColor !=
			// 	 fadeScreenWithSolidColor.TargetScreenOverlayColor ) {
			// 	
			// 	// if the difference between the current alpha and the desired alpha is smaller than delta-alpha * deltaTime, then we're pretty much done fading:
			// 	if ( Mathf.Abs( fadeScreenWithSolidColor.CurrentScreenOverlayColor.a -
			// 					fadeScreenWithSolidColor.TargetScreenOverlayColor.a ) <
			// 		 Mathf.Abs( fadeScreenWithSolidColor.DeltaColor.a ) * Game.DeltaTime ) {
			// 		var currentColor = fadeScreenWithSolidColor.TargetScreenOverlayColor;
			// 		var deltaColor = new Color( 0, 0, 0, 0 );
			// 		UpdateFadeTexture( fadeScreenWithSolidColor.CurrentScreenOverlayColor );
			// 		fadeScreenWithSolidColorEntity.SetFadeScreenWithSolidColor( currentColor, fadeScreenWithSolidColor.TargetScreenOverlayColor, deltaColor, fadeScreenWithSolidColor.FadeDuration, fadeScreenWithSolidColor.DestroyAfterFinished );
			// 	}
			// 	else {
			// 		// fade!
			// 		var currentColor = fadeScreenWithSolidColor.CurrentScreenOverlayColor +
			// 						   fadeScreenWithSolidColor.DeltaColor * Game.DeltaTime;
			// 		UpdateFadeTexture( currentColor );
			// 		fadeScreenWithSolidColorEntity.SetFadeScreenWithSolidColor( currentColor, fadeScreenWithSolidColor.TargetScreenOverlayColor, fadeScreenWithSolidColor.DeltaColor, fadeScreenWithSolidColor.FadeDuration, fadeScreenWithSolidColor.DestroyAfterFinished );
			// 	}
			// }
			
			if ( _fadeInTime != -1f || _fadeOutTime != -1f ) {
				if ( _fadeInTime != -1f ) {
					_currentAlpha = Math.Min( Game.NowUnscaled - _fadeInTime, _duration ) / _duration;
					UpdateFadeTexture( _color * _currentAlpha );

					if ( _currentAlpha == 1f ) {
						_fadeInTime = -1f;
						
						// var fadeScreenWithSolidColor = fadeScreenWithSolidColorEntity.GetFadeScreenWithSolidColor();
						// if ( fadeScreenWithSolidColor.DestroyAfterFinished ) {
						// 	fadeScreenWithSolidColorEntity.Destroy();
						// }
					}
				}

				if ( _fadeOutTime != -1f ) {
					_currentAlpha = 1f - Math.Min( Game.NowUnscaled - _fadeOutTime, _duration ) / _duration;
					UpdateFadeTexture( _color * _currentAlpha );

					if ( _currentAlpha == 0 ) {
						_fadeOutTime = -1f;
						
						// var fadeScreenWithSolidColor = fadeScreenWithSolidColorEntity.GetFadeScreenWithSolidColor();
						// if ( fadeScreenWithSolidColor.DestroyAfterFinished ) {
						// 	fadeScreenWithSolidColorEntity.Destroy();
						// }
					}
				}

				if ( _currentAlpha == 0 ) {
					return;
				}
			}
		}
		
		public void DrawGui( Context context ) {
			if ( context.World.TryGetUniqueFadeScreenWithSolidColor() is {} ) {
				// only draw the texture when the alpha value is greater than 0:
				if ( _currentAlpha > 0f ) {
					GUI.depth = FadeGUIDepth;
					GUI.Label( new Rect( -10, -10, Screen.width + 10, Screen.height + 10 ), _fadeTexture, _backgroundStyle );
				}
			}
		}

		public void OnAdded( World world, ImmutableArray< Entity > entities ) => OnModified( world, entities );

		public void OnRemoved( World world, ImmutableArray< Entity > entities ) {}

		public void OnModified( World world, ImmutableArray< Entity > entities ) {
			_fadeInTime = -1;
			_fadeOutTime = -1;

			foreach ( Entity e in entities ) {
				FadeScreenWithSolidColorComponent f = e.GetFadeScreenWithSolidColor();
				switch ( f.FadeType ) {
					case FadeType.In:
						// GameLogger.Log("Received fade in.");

						_fadeInTime = Game.NowUnscaled;
						break;

					case FadeType.Out:
						// GameLogger.Log("Received fade out.");

						_fadeOutTime = Game.NowUnscaled;
						break;
					
					// case FadeType.Flash:
					// 	current = 1 - Math.Abs( ratio - 0.5f ) * 2;
					// 	break;
				}

				_color = f.Color;
				_duration = f.Duration;
			}
		}

		private void UpdateFadeTexture( Color newScreenOverlayColor ) {
			_fadeTexture.SetPixel( 0, 0, newScreenOverlayColor );
			_fadeTexture.Apply();
		}
	}

}
