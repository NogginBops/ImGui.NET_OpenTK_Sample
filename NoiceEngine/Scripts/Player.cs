namespace Engine;

public class Player : Component
{
	public static Player I { get; set; }
	private readonly float PLAYER_SCALE = 7;

	public float moveSpeed = 0.05f;

	private Rigidbody rb;

	public override void Awake()
	{
		I = this;

		rb = GetComponent<Rigidbody>();
		base.Awake();
	}

	public override void Start()
	{
		base.Start();
	}

	public override void Update()
	{
		float velX = rb.Velocity.X;
		float velY = rb.Velocity.Y;
		if (KeyboardInput.IsKeyDown(Keys.A))
		{
			transform.scale.X = -PLAYER_SCALE;
			velX = Engine.Mathf.Lerp(velX, -moveSpeed, Time.deltaTime * 3);
		}
		else if (KeyboardInput.IsKeyDown(Keys.D))
		{
			transform.scale.X = PLAYER_SCALE;
			velX = Engine.Mathf.Lerp(velX, moveSpeed, Time.deltaTime * 3);
		}
		else
		{
			velX = Engine.Mathf.Lerp(velX, 0, Time.deltaTime * 5);
		}

		rb.Velocity = new Vector2(velX, velY);

		base.Update();
	}
}