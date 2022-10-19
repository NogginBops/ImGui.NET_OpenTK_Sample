namespace Tofu3D.Physics;

public static class CollisionDetection
{
	public static bool CheckCollisionRaycastBox(Ray ray, BoxShape boxShape)
	{
		Vector3 min = boxShape.GetMinPos();
		Vector3 max = boxShape.GetMaxPos();

		float t1 = (min.X - ray.origin.X) / ray.direction.X;
		float t2 = (max.X - ray.origin.X) / ray.direction.X;
		float t3 = (min.Y - ray.origin.Y) / ray.direction.Y;
		float t4 = (max.Y - ray.origin.Y) / ray.direction.Y;
		float t5 = (min.Z - ray.origin.Z) / ray.direction.Z;
		float t6 = (max.Z - ray.origin.Z) / ray.direction.Z;

		float tmin = Mathf.Max(Mathf.Max(Mathf.Min(t1, t2), Mathf.Min(t3, t4)), Mathf.Min(t5, t6));
		float tmax = Mathf.Min(Mathf.Min(Mathf.Max(t1, t2), Mathf.Max(t3, t4)), Mathf.Max(t5, t6));

		// if tmax < 0, ray (line) is intersecting AABB, but whole AABB is behing us
		if (tmax < 0)
		{
			return false;
		}

		// if tmin > tmax, ray doesn't intersect AABB
		if (tmin > tmax)
		{
			return false;
		}

		return true;
	}
}