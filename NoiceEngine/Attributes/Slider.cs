namespace Engine;

[Show]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
public sealed class Slider : Attribute
{
	public int minValue;
	public int maxValue;

	public Slider(int _min, int _max)
	{
		minValue = _min;
		maxValue = _max;
	}
}