#if !NO_UNITY
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bang.Unity.Serialization.DirectConverters {

	public class Color_DirectConverter : fsDirectConverter< Color > {
		protected override fsResult DoSerialize( Color model, Dictionary< string, fsData > serialized ) {
			SerializeMember( serialized, null, "r", model.r );
			SerializeMember( serialized, null, "g", model.g );
			SerializeMember( serialized, null, "b", model.b );
			SerializeMember( serialized, null, "a", model.a );
			return fsResult.Success;
		}

		protected override fsResult DoDeserialize( Dictionary< string, fsData > data, ref Color model ) {
			var t0 = model.r;
			DeserializeMember( data, null, "r", out t0 );
			model.r = t0;

			var t1 = model.g;
			DeserializeMember( data, null, "g", out t1 );
			model.g = t1;

			var t2 = model.b;
			DeserializeMember( data, null, "b", out t2 );
			model.b = t2;
			
			var t3 = model.a;
			DeserializeMember( data, null, "a", out t3 );
			model.a = t3;

			return fsResult.Success;
		}

		public override object CreateInstance( fsData data, Type storageType ) {
			return new Color();
		}
	}

}
#endif