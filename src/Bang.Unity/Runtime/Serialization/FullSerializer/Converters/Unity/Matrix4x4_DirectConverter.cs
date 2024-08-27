#if !NO_UNITY
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bang.Unity.Serialization.DirectConverters {

	public class Matrix4x4_DirectConverter : fsDirectConverter< Matrix4x4 > {
		protected override fsResult DoSerialize( Matrix4x4 model, Dictionary< string, fsData > serialized ) {
			SerializeMember( serialized, null, "m00", model.m00 );
			SerializeMember( serialized, null, "m10", model.m10 );
			SerializeMember( serialized, null, "m20", model.m20 );
			SerializeMember( serialized, null, "m30", model.m30 );
			SerializeMember( serialized, null, "m01", model.m01 );
			SerializeMember( serialized, null, "m11", model.m11 );
			SerializeMember( serialized, null, "m21", model.m21 );
			SerializeMember( serialized, null, "m31", model.m31 );
			SerializeMember( serialized, null, "m02", model.m02 );
			SerializeMember( serialized, null, "m12", model.m12 );
			SerializeMember( serialized, null, "m22", model.m22 );
			SerializeMember( serialized, null, "m32", model.m32 );
			SerializeMember( serialized, null, "m03", model.m03 );
			SerializeMember( serialized, null, "m13", model.m13 );
			SerializeMember( serialized, null, "m23", model.m23 );
			SerializeMember( serialized, null, "m33", model.m33 );
			return fsResult.Success;
		}

		protected override fsResult DoDeserialize( Dictionary< string, fsData > data, ref Matrix4x4 model ) {
			DeserializeMember( data, null, "m00", out float v );
			model.m00 = v;
			DeserializeMember( data, null, "m10", out v );
			model.m10 = v;
			DeserializeMember( data, null, "m20", out v );
			model.m20 = v;
			DeserializeMember( data, null, "m30", out v );
			model.m30 = v;
			
			DeserializeMember( data, null, "m01", out v );
			model.m01 = v;
			DeserializeMember( data, null, "m11", out v );
			model.m11 = v;
			DeserializeMember( data, null, "m21", out v );
			model.m21 = v;
			DeserializeMember( data, null, "m31", out v );
			model.m31 = v;
			
			DeserializeMember( data, null, "m02", out v );
			model.m02 = v;
			DeserializeMember( data, null, "m12", out v );
			model.m12 = v;
			DeserializeMember( data, null, "m22", out v );
			model.m22 = v;
			DeserializeMember( data, null, "m32", out v );
			model.m32 = v;
			
			DeserializeMember( data, null, "m03", out v );
			model.m03 = v;
			DeserializeMember( data, null, "m13", out v );
			model.m13 = v;
			DeserializeMember( data, null, "m23", out v );
			model.m23 = v;
			DeserializeMember( data, null, "m33", out v );
			model.m33 = v;
			
			return fsResult.Success;
		}

		public override object CreateInstance( fsData data, Type storageType ) {
			return Matrix4x4.identity;
		}
	}

}
#endif