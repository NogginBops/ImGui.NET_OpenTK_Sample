namespace Tofu3D;

[ShowIf(null)]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, Inherited = false)]
public sealed class ShowIf : Show
{
	public string fieldName;
	public ShowIf(string fieldName)
	{
		this.fieldName = fieldName;
	}
}