using Bang.Components;
using UnityEngine;


namespace Bang.Unity.Effects {
	
	public enum FadeType {
		In,
		Out,
		OutBuffer,
		Flash
	}

	[Unique]
	[DoNotPersistOnSave]
	public readonly struct FadeScreenWithSolidColorComponent : IComponent {

		// public readonly Color CurrentScreenOverlayColor;
		// public readonly Color TargetScreenOverlayColor;
		// public readonly Color DeltaColor;
		// public readonly float FadeDuration;
		public readonly Color Color;
		public readonly FadeType FadeType;
		public readonly float Duration;
		public readonly bool DestroyAfterFinished;

		public FadeScreenWithSolidColorComponent( Color color, FadeType fade, float duration, bool destroyAfterFinished ) {
			Color = color;
			FadeType = fade;
			Duration = duration;
			DestroyAfterFinished = destroyAfterFinished;
		}

		// public FadeScreenWithSolidColorComponent( Color targetScreenOverlayColor, float fadeDuration, bool destroyAfterFinished ) {
		// 	CurrentScreenOverlayColor = Color.black;
		// 	TargetScreenOverlayColor = targetScreenOverlayColor;
		// 	DeltaColor = Color.clear;
		// 	FadeDuration = fadeDuration;
		// 	DestroyAfterFinished = destroyAfterFinished;
		// }
		//
		// public FadeScreenWithSolidColorComponent( Color currentScreenOverlayColor, Color targetScreenOverlayColor, Color deltaColor, float fadeDuration, bool destroyAfterFinished ) {
		// 	CurrentScreenOverlayColor = currentScreenOverlayColor;
		// 	TargetScreenOverlayColor = targetScreenOverlayColor;
		// 	DeltaColor = deltaColor;
		// 	FadeDuration = fadeDuration;
		// 	DestroyAfterFinished = destroyAfterFinished;
		// }
	}

}
