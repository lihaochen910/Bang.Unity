using System;
using UnityEngine;


namespace Bang.Unity.Conversion {

	public enum ConversionMode : byte {
		ConvertAndDestroy,
		SyncWithEntity
	}


	[Serializable]
	public class EntityConversionOptions {
		
		public static readonly EntityConversionOptions Default = new();

		[SerializeField]
		private ConversionMode _conversionMode = ConversionMode.SyncWithEntity;
		
		[SerializeField]
		private bool _useDisabledComponent = false;

		public ConversionMode ConversionMode {
			get => _conversionMode;
			set => _conversionMode = value;
		}

		public bool UseDisabledComponent
		{
			get => _useDisabledComponent;
			set => _useDisabledComponent = value;
		}
	}
	
}
