using System.Reflection;

namespace Bang.Unity.Serialization
{

    public interface ISerializedMethodBaseInfo : ISerializedReflectedInfo
    {
        MethodBase GetMethodBase();
        bool HasChanged();
    }
}