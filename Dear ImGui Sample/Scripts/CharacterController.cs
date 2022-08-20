namespace Scripts;

public class CharacterController : Component
{
	public float jumpForce = 10000;
	private bool jumpKeyDown;
	public float moveSpeed = 10;

	// LINKABLECOMPONENT PURGE [LinkableComponent]
	private Rigidbody rb;

	public override void Awake()
	{
		rb = GetComponent<Rigidbody>();
		base.Awake();
	}

	public override void FixedUpdate()
	{
		if (rb == null)
		{
			return;
		}

		Vector2 input = Vector2.Zero;
		if (KeyboardInput.IsKeyDown(Keys.A))
		{
			input.X = -moveSpeed;
		}

		if (KeyboardInput.IsKeyDown(Keys.D))
		{
			input.X = moveSpeed;
		}

		if (jumpKeyDown == false && KeyboardInput.IsKeyDown(Keys.W))
		{
			//rb.body.ApplyForce(new Vector2(0, -JumpForce));
		}

		jumpKeyDown = KeyboardInput.IsKeyDown(Keys.W);
		//rb.body.ApplyForce(new Vector2(input.X, 0));
		base.Update();
	}
}