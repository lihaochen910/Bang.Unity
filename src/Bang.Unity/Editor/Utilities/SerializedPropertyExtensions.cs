using System;
using System.Reflection;
using UnityEditor;


namespace Bang.Unity.Editor.Utilities {

	public static class SerializedPropertyExtensions {
		
		private const BindingFlags AllBindingFlags = (BindingFlags)(-1);

		/// <summary>
		/// Returns attributes of type <typeparamref name="TAttribute"/> on <paramref name="serializedProperty"/>.
		/// </summary>
		public static TAttribute[] GetAttributes<TAttribute>(this SerializedProperty serializedProperty, bool inherit)
			where TAttribute : Attribute
		{
			if (serializedProperty == null)
			{
				throw new ArgumentNullException(nameof(serializedProperty));
			}

			var targetObjectType = serializedProperty.serializedObject.targetObject.GetType();
			if (targetObjectType == null)
			{
				throw new ArgumentException($"Could not find the {nameof(targetObjectType)} of {nameof(serializedProperty)}");
			}

			foreach (var pathSegment in serializedProperty.propertyPath.Split('.'))
			{
				var fieldInfo = targetObjectType.GetField(pathSegment, AllBindingFlags);
				if (fieldInfo != null)
				{
					return (TAttribute[])fieldInfo.GetCustomAttributes<TAttribute>(inherit);
				}

				var propertyInfo = targetObjectType.GetProperty(pathSegment, AllBindingFlags);
				if (propertyInfo != null)
				{
					return (TAttribute[])propertyInfo.GetCustomAttributes<TAttribute>(inherit);
				}
			}

			throw new ArgumentException($"Could not find the field or property of {nameof(serializedProperty)}");
		}
		
		
		/// <summary>
		/// Returns attributes of type <typeparamref name="TAttribute"/> on <paramref name="serializedProperty"/>.
		/// </summary>
		public static TAttribute GetAttribute<TAttribute>(this SerializedProperty serializedProperty, bool inherit) where TAttribute : Attribute
		{
			if (serializedProperty == null)
			{
				throw new ArgumentNullException(nameof(serializedProperty));
			}

			var targetObjectType = serializedProperty.serializedObject.targetObject.GetType();
			if (targetObjectType == null)
			{
				throw new ArgumentException($"Could not find the {nameof(targetObjectType)} of {nameof(serializedProperty)}");
			}

			foreach (var pathSegment in serializedProperty.propertyPath.Split('.'))
			{
				var fieldInfo = targetObjectType.GetField(pathSegment, AllBindingFlags);
				if (fieldInfo != null)
				{
					return fieldInfo.GetCustomAttribute<TAttribute>(inherit);
				}

				var propertyInfo = targetObjectType.GetProperty(pathSegment, AllBindingFlags);
				if (propertyInfo != null)
				{
					return propertyInfo.GetCustomAttribute<TAttribute>(inherit);
				}
			}

			// throw new ArgumentException($"Could not find the field or property of {nameof(serializedProperty)}");
			return null;
		}
		
	}

}
