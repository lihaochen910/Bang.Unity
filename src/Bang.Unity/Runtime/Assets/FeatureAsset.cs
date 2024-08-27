using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bang.Unity.Utilities;
using UnityEngine;


namespace Bang.Unity {
    
    [Serializable]
    public abstract class SerializableTuple<T1, T2> : Tuple<T1, T2> {
        
        [SerializeField]
        private T1 value1;

        [SerializeField]
        private T2 value2;

        public SerializableTuple(T1 item1, T2 item2): base(item1, item2) {
            value1 = item1;
            value2 = item2;
        }

        public new T1 Item1 => value1;
        public new T2 Item2 => value2;
    }


    [Serializable]
    public class TypeBooleanTuple : SerializableTuple< SerializableSystemType, bool > {
        public TypeBooleanTuple( SerializableSystemType item1, bool item2 ) : base( item1, item2 ) { }
    }


    [Serializable]
    public class FeatureBooleanTuple : SerializableTuple< FeatureAsset, bool > {
        public FeatureBooleanTuple( FeatureAsset item1, bool item2 ) : base( item1, item2 ) { }
    }

    
    [CreateAssetMenu(menuName = "Bang/FeatureAsset")]
    public class FeatureAsset : ScriptableObject {
        
        /// <summary>
        /// Whether this should always be added when running with diagnostics (e.g. editor).
        /// This will NOT be serialized into the world.
        /// </summary>
        public bool IsDiagnostics = false;

        /// <summary>
        /// Map of all the systems and whether they are active or not.
        /// </summary>
        [SerializeField]
        public List<TypeBooleanTuple> Systems = new ();

        [SerializeField]
        public List<FeatureBooleanTuple> Features = new ();

        public bool HasSystems
        {
            get
            {
                if (Systems.Count > 0)
                    return true;

                foreach (var feature in Features)
                {
                    if (!feature.Item2)
                        continue;

                    if (feature.Item1.HasSystems)
                        return true;
                }

                return false;
            }
        }

        public void SetSystems(IList<TypeBooleanTuple> newList)
        {
            Systems = newList.ToList();
        }

        public void SetFeatures(IList<FeatureBooleanTuple> newSystemsList)
        {
            Features = newSystemsList.ToList();
        }

        public ImmutableArray<(Type systemType, bool isActive)> FetchAllSystems(bool enabled)
        {
            var builder = ImmutableArray.CreateBuilder<(Type systemType, bool isActive)>();
        
            foreach (var system in Systems)
            {
                builder.Add((system.Item1, system.Item2 && enabled));
            }
        
            foreach (var data in Features)
            {
                FeatureAsset? asset = data.Item1;
                if (asset is null)
                {
                    // Debug.LogWarning($"Skipping feature asset of {guid.feature} for {this}.");
                    continue;
                }
        
                builder.AddRange(asset.FetchAllSystems(data.Item2 && enabled));
            }
        
            return builder.ToImmutable();
        }
	    
    }

}
