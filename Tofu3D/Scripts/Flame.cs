public class Flame : Component
{
	public Rigidbody rb;

	public override void Awake()
	{
		rb = GetComponent<Rigidbody>();
		base.Awake();
	}
}