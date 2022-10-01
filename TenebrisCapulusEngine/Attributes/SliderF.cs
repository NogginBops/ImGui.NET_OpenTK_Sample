namespace Engine;

[Show]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
public sealed class SliderF : Attribute
{
	public float minValue;
	public float maxValue;

	public SliderF(float _min, float _max)
	{
		minValue = _min;
		maxValue = _max;
	}
}