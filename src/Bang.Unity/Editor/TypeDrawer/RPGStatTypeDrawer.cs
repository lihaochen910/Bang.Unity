using System;
using System.Linq;
using Bang.Unity.Utilities;
using DigitalRune.Mathematics;
using DigitalRune.Linq;
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


	public class RPGStatModifierTypeDrawer : ITypeDrawer {

		private static Lazy< Type[] > Impls = new ( () =>
			ReflectionHelper.GetAllImplementationsOf( typeof( RPGStatModifier ) ).ToArray() );

		private float _testStatValue;
		
		public bool CanHandlesType( Type type ) => type == typeof( RPGStatModifier ) || type.IsSubclassOf( typeof( RPGStatModifier ) );

		public object DrawAndGetNewValue( Type memberType, string memberName, object value, object target ) {
			var statModifier = ( RPGStatModifier )value;
			
			EditorGUI.BeginDisabledGroup( true );
			
			EditorGUILayout.LabelField( $"{statModifier.GetType().Name}" );
			
			EditorGUI.EndDisabledGroup();
			
			var selectedIndex = Impls.Value.IndexOf( t => t == value.GetType() );
			var typeNames = Impls.Value
								 .Select( t => $"{t.Name} ({t.Namespace})" )
								 .ToArray();
			var index = EditorGUILayout.Popup( "ModifierType", selectedIndex, typeNames );
			if ( index >= 0 && Impls.Value[ index ] != statModifier.GetType() ) {
				var newStatModifier = Activator.CreateInstance( Impls.Value[ index ], 0f ) as RPGStatModifier;
				statModifier = newStatModifier;
			}
			
			var newStacksValue = EditorGUILayout.Toggle( "stacks", statModifier.Stacks );
			if ( newStacksValue != statModifier.Stacks ) {
				statModifier.Stacks = newStacksValue;
			}
			
			var newStatBaseValue = EditorGUILayout.FloatField( "value", statModifier.Value );
			if ( !Numeric.AreEqual( newStatBaseValue, statModifier.Value ) ) {
				statModifier.Value = newStatBaseValue;
			}
			
			EditorGUI.BeginDisabledGroup( true );
			
			EditorGUILayout.LabelField( $"order: {statModifier.Order}" );
			
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.BeginVertical();
			
			_testStatValue = EditorGUILayout.FloatField( "test", _testStatValue );
			EditorGUI.indentLevel++;
			EditorGUILayout.LabelField( $"result: {statModifier.ApplyModifier( _testStatValue, statModifier.Value ):0.00}" );
			EditorGUI.indentLevel--;
			
			EditorGUILayout.EndVertical();
			
			return statModifier;
		}
	}

}
