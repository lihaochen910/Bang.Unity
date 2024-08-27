using System;
using System.Collections;
using System.Reflection;
using Bang.Interactions;


namespace Bang.Unity.Serialization.Converters {

	public class BangInteractiveComponent_Converter : fsConverter {
		
		public override bool CanProcess(Type type) {
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof( InteractiveComponent<> );
		}

		public override bool RequestCycleSupport(Type type) => true;

		public override bool RequestInheritanceSupport(Type type) => true;
		
		public override object CreateInstance(fsData data, Type storageType) {
			return Activator.CreateInstance( storageType );
		}
		
		public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType) {
            serialized = fsData.CreateDictionary();
            var result = fsResult.Success;

			var obj = instance.GetType().GetField( "_interaction", BindingFlags.NonPublic | BindingFlags.Instance )
					.GetValue( instance );

            fsMetaType metaType = fsMetaType.Get(obj.GetType());

            //Dont do this for UnityObject. While there is fsUnityObjectConverter, this converter is also used as override,
            //when serializing a UnityObject directly.
            object defaultInstance = null;
            if ( fsGlobalConfig.SerializeDefaultValues == false && !( instance is UnityEngine.Object ) ) {
                defaultInstance = metaType.GetDefaultInstance();
            }

            for ( var i = 0; i < metaType.Properties.Length; ++i ) {
                fsMetaProperty property = metaType.Properties[i];

                if ( property.WriteOnly ) {
                    continue;
                }

                if ( property.AsReference && Serializer.IgnoreSerializeCycleReferences ) {
                    continue;
                }

                var propertyValue = property.Read(obj);

                // auto instance?
                if ( propertyValue == null && property.AutoInstance ) {
                    propertyValue = fsMetaType.Get(property.StorageType).CreateInstance();
                    property.Write(obj, propertyValue);
                } else if ( fsGlobalConfig.SerializeDefaultValues == false && defaultInstance != null ) {
                    if ( Equals(propertyValue, property.Read(defaultInstance)) ) {
                        continue;
                    }
                }

                fsData serializedData;
                var itemResult = Serializer.TrySerialize(property.StorageType, propertyValue, out serializedData);
                result.AddMessages(itemResult);
                if ( itemResult.Failed ) {
                    continue;
                }

                serialized.AsDictionary[property.JsonName] = serializedData;
            }

            return result;
        }
		
		public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType) {
			var result = fsResult.Success;

			// Verify that we actually have an Object
			if ( ( result += CheckType(data, fsDataType.Object) ).Failed ) {
				return result;
			}

			if ( data.AsDictionary.Count == 0 ) {
				return fsResult.Success;
			}
			
			var obj = instance.GetType().GetField( "_interaction", BindingFlags.NonPublic | BindingFlags.Instance )
								  .GetValue( instance );

			fsMetaType metaType = fsMetaType.Get(storageType.GetGenericArguments()[ 0 ]);

			for ( var i = 0; i < metaType.Properties.Length; ++i ) {
				fsMetaProperty property = metaType.Properties[i];

				if ( property.ReadOnly ) {
					continue;
				}

				fsData propertyData;
				if ( data.AsDictionary.TryGetValue(property.JsonName, out propertyData) ) {
					object deserializedValue = null;

					//This does not work well with no serializing default values -> Find a workaround.
					if ( fsGlobalConfig.SerializeDefaultValues ) {
						if ( metaType.DeserializeOverwriteRequest || typeof(ICollection).IsAssignableFrom(storageType) ) {
							deserializedValue = property.Read(obj);
						}
					}

					var itemResult = Serializer.TryDeserialize(propertyData, property.StorageType, ref deserializedValue, null);
					result.AddMessages(itemResult);
					if ( itemResult.Failed ) continue;

					property.Write(obj, deserializedValue);
				}
			}
			
			instance.GetType().GetField( "_interaction", BindingFlags.NonPublic | BindingFlags.Instance )
					.SetValue( instance, obj );

			return result;
		}
	}

}