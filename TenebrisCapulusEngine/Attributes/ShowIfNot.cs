using System.Reflection;

namespace Engine;

[ShowIfNot(null)]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, Inherited = false)]
public sealed class ShowIfNot : Show
{
	public string fieldName;
	public ShowIfNot(string fieldName)
	{
		this.fieldName = fieldName;
	}
}