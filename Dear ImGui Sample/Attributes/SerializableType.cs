using System.Xml.Serialization;

namespace Engine;

[Show]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Delegate, Inherited = false)]
public sealed class SerializableType : Attribute
{
}