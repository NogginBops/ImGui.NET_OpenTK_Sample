namespace Scripts;

public class BoxShape : Shape
{
	public Vector3 offset = Vector3.Zero;
	public Vector3 size;

	public Vector3 GetMinPos()
	{
		return transform.position;
	}

	public Vector3 GetMaxPos()
	{
		return transform.position + size;
	}
}