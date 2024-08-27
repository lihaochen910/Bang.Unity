using System.Reflection;

namespace Bang.Unity.Serialization
{

    ///<summary>Interface between Serialized_X_Info</summary>
    public interface ISerializedReflectedInfo : UnityEngine.ISerializationCallbackReceiver
    {
        MemberInfo AsMemberInfo();
        string AsString();
    }
}