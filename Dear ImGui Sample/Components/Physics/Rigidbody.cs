using System.Collections.Generic;
using System.Xml.Serialization;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Dynamics;

namespace Scripts;

public class Rigidbody : Component
{
	public new bool allowMultiple = false;

	public float angularDrag = 1f;
	[XmlIgnore] public Body body;

	public float friction = 1;
	public bool isButton = false;
	public bool isStatic = false;
	public bool isTrigger = false;

	[XmlIgnore] [LinkableComponent] public Shape shape;
	[XmlIgnore] public List<Rigidbody> touchingRigidbodies = new();
	public bool useGravity = false;
	[XmlIgnore]
	public Vector2 Velocity
	{
		get
		{
			if (body == null)
			{
				return Vector2.Zero;
			}

			return body.LinearVelocity;
		}
		set
		{
			if (body == null)
			{
				return;
			}

			body.LinearVelocity = value;
		}
	}
	public float Mass { get; set; } = 100;
	//public float velocityDrag = 0.99f;
	[Show] public float Bounciness { get; set; } = 0f;

	[XmlIgnore]
	public float AngularVelocity
	{
		get
		{
			if (body == null)
			{
				return 0;
			}

			return body.AngularVelocity;
		}
		set
		{
			if (body == null)
			{
				return;
			}

			body.AngularVelocity = value;
		}
	}

	public override void Awake()
	{
		//gameObject.OnComponentAdded += CheckForColliderAdded;
		if (isButton)
		{
			return;
		}

		CreateBody();

		base.Awake();
	}

	public void CreateBody()
	{
		BodyDef bodyDef = new BodyDef();
		bodyDef.Position = transform.position;
		bodyDef.Type = isStatic ? BodyType.Static : BodyType.Dynamic;
		bodyDef.AllowSleep = true;

		//body.SleepingAllowed = false;

		if (GetComponent<CircleShape>() != null)
		{
			CircleShape circleShape = GetComponent<CircleShape>();

			FixtureDef fixtureDef = new FixtureDef();
			fixtureDef.Shape = new Genbox.VelcroPhysics.Collision.Shapes.CircleShape(circleShape.radius, 100);
			fixtureDef.Friction = 0.1f;
			lock (Physics.World)
			{
				body = Physics.World.CreateBody(bodyDef);
				body.SleepingAllowed = true;
				body.CreateFixture(fixtureDef);
				body.LinearDamping = 0;
				body.AngularDamping = 0;
				body.Mass = Mass;
			}
		}
		else if (GetComponent<BoxShape>() != null)
		{
			//BoxShape boxShape = GetComponent<BoxShape>();
			//var pfixture = body.CreateRectangle(boxShape.size.X * transform.scale.X, boxShape.size.Y * transform.scale.Y, 1, Vector2.Zero);
			//// Give it some bounce and friction
			//pfixture.Friction = 0.1f;
			//body.LinearDamping = 0;
			////body.LinearDamping = 3;
		}
	}

	public override void FixedUpdate()
	{
		if (isStatic || isButton)
		{
			return;
		}

		UpdateTransform();

		//lock (Physics.World)
		//{
		//	body.Mass = Mass;
		//}
	}

	public void UpdateTransform()
	{
		if (body == null)
		{
			return;
		}

		transform.position = new Vector2(body.Position.X, body.Position.Y);
		transform.rotation.Z = body.Rotation * Mathf.TwoPi * 2;
	}

	public override void OnDestroyed()
	{
		if (body != null)
		{
			lock (Physics.World)
			{
				body.Enabled = false;
				Physics.World.DestroyBody(body);
			}
		}

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