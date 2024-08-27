using System.Collections.Generic;

namespace Bang.Unity.Serialization
{
    [System.Serializable]
    ///<summary>A pair of JSON and UnityObject references</summary>
    sealed public class SerializationPair
    {
        public string _json;
        public List<UnityEngine.Object> _references;
        public SerializationPair() { _references = new List<UnityEngine.Object>(); }
    }
}