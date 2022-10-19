namespace Tofu3D.Physics;

public class World
{
	public static World I { get; private set; }

	public List<Rigidbody> bodies;

	public World()
	{
		I = this;

		bodies = new List<Rigidbody>();
	}

	public void AddBody(Rigidbody rb)
	{
		bodies.Add(rb);
	}
}