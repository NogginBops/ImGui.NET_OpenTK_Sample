namespace Tofu3D.Physics;

public static class Physics
{
	public static RaycastResult Raycast(Ray ray)
	{
		List<Rigidbody> hitBodies = new List<Rigidbody>();


		// Go through all the bodies and their respective colliders and check for collisions
		for (int bodyIndex = 0; bodyIndex < World.I.bodies.Count; bodyIndex++)
		{
			if (World.I.bodies[bodyIndex].Shape is BoxShape)
			{
				bool hit = CollisionDetection.CheckCollisionRaycastBox(ray, World.I.bodies[bodyIndex].Shape as BoxShape);
				if (hit)
				{
					hitBodies.Add(World.I.bodies[bodyIndex]);
				}
			}
		}

		RaycastResult result = new RaycastResult() {hitBodies = hitBodies};

		return result;
	}
}