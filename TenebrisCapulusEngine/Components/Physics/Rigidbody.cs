using Tofu3D.Physics;

namespace Scripts;

public class Rigidbody : Component
{
	[Hide]
	public new bool allowMultiple = false;

	public float angularDrag = 1f;

	[XmlIgnore]
	[LinkableComponent]
	public Shape Shape
	{
		get { return GetComponent<Shape>(); }
	}
	[XmlIgnore]
	public List<Rigidbody> touchingRigidbodies = new List<Rigidbody>();
	public Vector2 BodyPos;

	public override void Awake()
	{
		CreateBody();

		base.Awake();
	}

	public void CreateBody()
	{
		BoxShape boxShape = GetComponent<BoxShape>();

		if (boxShape != null)
		{
			BoxShape shape = new BoxShape();

			lock (PhysicsController.World)
			{
				PhysicsController.World.AddBody(this);
			}
		}
	}

	public override void FixedUpdate()
	{
	}

	// public void UpdateTransform()
	// {
	// 	if (body == null)
	// 	{
	// 		return;
	// 	}
	//
	// 	transform.position = new Vector2(body.Position.X, body.Position.Y) * Physics.WORLD_SCALE;
	// 	transform.Rotation = new Vector3(transform.Rotation.X, transform.Rotation.Y, body.Rotation * Mathf.TwoPi * 2);
	// }

	public override void OnDestroyed()
	{
		for (int i = 0; i < touchingRigidbodies.Count; i++)
		{
			touchingRigidbodies[i].OnCollisionExit(this);
			OnCollisionExit(touchingRigidbodies[i]);
		}
	}

	public override void OnCollisionEnter(Rigidbody rigidbody) // TODO-TRANSLATE CURRENT VELOCITY TO COLLIDED RIGIDBODY, ADD FORCE (MassRatio2/MassRatio1)
	{
		touchingRigidbodies.Add(rigidbody);

		// Call callback on components that implement interface IPhysicsCallbackListener
		for (int i = 0; i < gameObject.components.Count; i++)
			if (gameObject.components[i] is Rigidbody == false)
			{
				gameObject.components[i].OnCollisionEnter(rigidbody);
			}
	}

	public override void OnCollisionExit(Rigidbody rigidbody)
	{
		if (touchingRigidbodies.Contains(rigidbody))
		{
			touchingRigidbodies.Remove(rigidbody);
		}

		for (int i = 0; i < gameObject.components.Count; i++)
			if (gameObject.components[i] is Rigidbody == false)
			{
				gameObject.components[i].OnCollisionExit(rigidbody);
			}
	}

	public override void OnTriggerEnter(Rigidbody rigidbody)
	{
		touchingRigidbodies.Add(rigidbody);

		// Call callback on components that implement interface IPhysicsCallbackListener
		for (int i = 0; i < gameObject.components.Count; i++)
			if (gameObject.components[i] is Rigidbody == false)
			{
				gameObject.components[i].OnTriggerEnter(rigidbody);
			}
	}

	public override void OnTriggerExit(Rigidbody rigidbody)
	{
		if (touchingRigidbodies.Contains(rigidbody))
		{
			touchingRigidbodies.Remove(rigidbody);
		}

		for (int i = 0; i < gameObject.components.Count; i++)
			if (gameObject.components[i] is Rigidbody == false)
			{
				gameObject.components[i].OnTriggerExit(rigidbody);
			}
	}
}