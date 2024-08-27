#if !NO_UNITY
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bang.Unity.Serialization.DirectConverters {

	public class Vector4_DirectConverter : fsDirectConverter< Vector4 > {
		protected override fsResult DoSerialize( Vector4 model, Dictionary< string, fsData > serialized ) {
			SerializeMember( serialized, null, "x", model.x );
			SerializeMember( serialized, null, "y", model.y );
			SerializeMember( serialized, null, "z", model.z );
			SerializeMember( serialized, null, "w", model.w );
			return fsResult.Success;
		}

		protected override fsResult DoDeserialize( Dictionary< string, fsData > data, ref Vector4 model ) {
			var t0 = model.x;
			DeserializeMember( data, null, "x", out t0 );
			model.x = t0;

			var t1 = model.y;
			DeserializeMember( data, null, "y", out t1 );
			model.y = t1;

			var t2 = model.z;
			DeserializeMember( data, null, "z", out t2 );
			model.z = t2;
			
			var t3 = model.w;
			DeserializeMember( data, null, "w", out t3 );
			model.w = t3;

			return fsResult.Success;
		}

		public override object CreateInstance( fsData data, Type storageType ) {
			return new Vector4();
		}
	}

}
#endif