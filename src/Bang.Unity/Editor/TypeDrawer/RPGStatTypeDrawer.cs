using System;
using DigitalRune.Mathematics;
using Pixpil.RPGStatSystem;
using UnityEditor;
using UnityEngine;


namespace Bang.Unity.Editor {

	public class RPGStatTypeDrawer : ITypeDrawer {

		public bool CanHandlesType( Type type ) => type == typeof( RPGStat );

		public object DrawAndGetNewValue( Type memberType, string memberName, object value, object target ) {
			var rpgStat = ( RPGStat )value;
			EditorGUILayout.LabelField( $"{memberName}: {rpgStat.StatValue:0.000}" );
			
			var newStatBaseValue = EditorGUILayout.FloatField( "base", rpgStat.StatBaseValue );
			if ( !Numeric.AreEqual( newStatBaseValue, rpgStat.StatBaseValue ) ) {
				rpgStat.SetBaseValue( newStatBaseValue );
			}
			
			EditorGUILayout.LabelField( $"scale: {rpgStat.StatScaleValue}" );

			return rpgStat;
		}
	}


	public class RPGStatModifiableTypeDrawer : ITypeDrawer {
		
		public bool CanHandlesType( Type type ) => type == typeof( RPGStatModifiable );
		
		public object DrawAndGetNewValue( Type memberType, string memberName, object value, object target ) {
			var rpgStat = ( RPGStatModifiable )value;
			EditorGUILayout.LabelField( $"{memberName}: {rpgStat.StatValue:0.000}" );
			
			var newStatBaseValue = EditorGUILayout.FloatField( "base", rpgStat.StatBaseValue );
			if ( !Numeric.AreEqual( newStatBaseValue, rpgStat.StatBaseValue ) ) {
				rpgStat.SetBaseValue( newStatBaseValue );
			}
			
			EditorGUILayout.LabelField( $"scale: {rpgStat.StatScaleValue}" );
			EditorGUILayout.LabelField( $"modifier: {rpgStat.StatModifierValue}" );
			
			EditorGUI.BeginDisabledGroup( true );
			
			EditorGUILayout.LabelField( $"modifiers: {rpgStat.GetModifierCount()}" );
			EditorGUI.indentLevel++;

			for ( var i = 0; i < rpgStat.StatMods.Count; i++ ) {
				var rpgStatStatMod = rpgStat.StatMods[ i ];
				EditorGUILayout.LabelField( $"#{i}: {rpgStatStatMod.Value} {rpgStatStatMod.GetType().Name}" );
			}
			
			EditorGUI.indentLevel--;
			
			EditorGUI.EndDisabledGroup();

			return rpgStat;
		}
	}


	public class RPGAttributeTypeDrawer : ITypeDrawer {
		
		public bool CanHandlesType( Type type ) => type == typeof( RPGAttribute );
		
		public object DrawAndGetNewValue( Type memberType, string memberName, object value, object target ) {
			var rpgStat = ( RPGAttribute )value;
			EditorGUILayout.LabelField( $"{memberName}: {rpgStat.StatValue:0.000}" );
			
			var newStatBaseValue = EditorGUILayout.FloatField( "base", rpgStat.StatBaseValue );
			if ( !Numeric.AreEqual( newStatBaseValue, rpgStat.StatBaseValue ) ) {
				rpgStat.SetBaseValue( newStatBaseValue );
			}
			
			EditorGUILayout.LabelField( $"scale: {rpgStat.StatScaleValue}" );
			EditorGUILayout.LabelField( $"modifier: {rpgStat.StatModifierValue}" );
			EditorGUILayout.LabelField( $"linker: {rpgStat.StatLinkerValue}" );
			
			// RPGStatModifiable
			EditorGUI.BeginDisabledGroup( true );
			
			EditorGUILayout.LabelField( $"modifiers: {rpgStat.GetModifierCount()}" );
			EditorGUI.indentLevel++;

			for ( var i = 0; i < rpgStat.StatMods.Count; i++ ) {
				var rpgStatStatMod = rpgStat.StatMods[ i ];
				EditorGUILayout.LabelField( $"#{i}: {rpgStatStatMod.Value} {rpgStatStatMod.GetType().Name}" );
			}
			
			EditorGUI.indentLevel--;
			
			EditorGUI.EndDisabledGroup();
			
			// RPGAttribute
			EditorGUI.BeginDisabledGroup( true );
			
			EditorGUILayout.LabelField( $"linkers: {rpgStat.GetLinkerCount()}" );
			if ( rpgStat.StatLinkers != null ) {
				EditorGUI.indentLevel++;

				for ( var i = 0; i < rpgStat.StatLinkers.Count; i++ ) {
					var rpgStatLinker = rpgStat.StatLinkers[ i ];
					EditorGUILayout.LabelField( $"#{i}: {rpgStatLinker.GetValue()} {rpgStatLinker.GetType().Name}" );
				}
				
				EditorGUI.indentLevel--;
			}
			
			EditorGUI.EndDisabledGroup();

			return rpgStat;
		}
	}


	public class RPGVitalTypeDrawer : ITypeDrawer {
		
		public bool CanHandlesType( Type type ) => type == typeof( RPGVital );
		
		public object DrawAndGetNewValue( Type memberType, string memberName, object value, object target ) {
			var rpgStat = ( RPGVital )value;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField( $"{memberName}: [ {rpgStat.StatValueCurrent:0.000} / {rpgStat.StatValue:0.000} ]" );
			if ( GUILayout.Button( "Min", EditorStyles.miniButtonRight, GUILayout.Width( 50 ) ) ) {
				rpgStat.StatValueCurrent = 0f;
			}
			if ( GUILayout.Button( "Max", EditorStyles.miniButtonRight, GUILayout.Width( 50 ) ) ) {
				rpgStat.SetCurrentValueToMax();
			}
			EditorGUILayout.EndHorizontal();
			
			var newStatBaseValue = EditorGUILayout.FloatField( "base", rpgStat.StatBaseValue );
			if ( !Numeric.AreEqual( newStatBaseValue, rpgStat.StatBaseValue ) ) {
				rpgStat.SetBaseValue( newStatBaseValue );
			}
			
			var newStatCurrentValue = EditorGUILayout.FloatField( "current", rpgStat.StatValueCurrent );
			if ( !Numeric.AreEqual( newStatCurrentValue, rpgStat.StatValueCurrent ) ) {
				rpgStat.StatValueCurrent = newStatCurrentValue;
			}
			
			EditorGUILayout.LabelField( $"scale: {rpgStat.StatScaleValue}" );
			EditorGUILayout.LabelField( $"modifier: {rpgStat.StatModifierValue}" );
			EditorGUILayout.LabelField( $"linker: {rpgStat.StatLinkerValue}" );
			
			// RPGStatModifiable
			EditorGUI.BeginDisabledGroup( true );
			
			EditorGUILayout.LabelField( $"modifiers: {rpgStat.GetModifierCount()}" );
			EditorGUI.indentLevel++;

			for ( var i = 0; i < rpgStat.StatMods.Count; i++ ) {
				var rpgStatStatMod = rpgStat.StatMods[ i ];
				EditorGUILayout.LabelField( $"#{i}: {rpgStatStatMod.Value} {rpgStatStatMod.GetType().Name}" );
			}
			
			EditorGUI.indentLevel--;
			
			EditorGUI.EndDisabledGroup();
			
			// RPGAttribute
			EditorGUI.BeginDisabledGroup( true );
			
			EditorGUILayout.LabelField( $"linkers: {rpgStat.GetLinkerCount()}" );
			if ( rpgStat.StatLinkers != null ) {
				EditorGUI.indentLevel++;

				for ( var i = 0; i < rpgStat.StatLinkers.Count; i++ ) {
					var rpgStatLinker = rpgStat.StatLinkers[ i ];
					EditorGUILayout.LabelField( $"#{i}: {rpgStatLinker.GetValue()} {rpgStatLinker.GetType().Name}" );
				}
				
				EditorGUI.indentLevel--;
			}
			
			EditorGUI.EndDisabledGroup();

			return rpgStat;
		}
	}

}
