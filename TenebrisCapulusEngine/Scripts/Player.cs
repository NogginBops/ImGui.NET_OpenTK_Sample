// using Engine.Tweening;
//
// namespace Engine;
//
// public class Player : Component
// {
// 	public static Player I { get; set; }
// 	private readonly float PLAYER_SCALE = 7;
//
// 	[Show]
// 	public GameObject flamePrefab;
// 	public float moveSpeed = 0.05f;
// 	public float jumpForce = 15000f;
//
// 	private Rigidbody rb;
//
// 	private AnimationController animationController;
//
// 	public override void Awake()
// 	{
// 		I = this;
//
// 		rb = GetComponent<Rigidbody>();
// 		animationController = GetComponent<AnimationController>();
// 		base.Awake();
// 	}
//
// 	public override void Start()
// 	{
// 		base.Start();
// 	}
//
// 	public override void Update()
// 	{
// 		float velX = rb.Velocity.X;
// 		float velY = rb.Velocity.Y;
// 		if (KeyboardInput.IsKeyDown(Keys.A))
// 		{
// 			velX = Engine.Mathf.Lerp(velX, -moveSpeed, Time.deltaTime * 3);
//
// 			animationController.Turn(Vector2.Left);
// 			animationController.SetAnimation(animationController.animRange_Run);
// 		}
// 		else if (KeyboardInput.IsKeyDown(Keys.D))
// 		{
// 			velX = Engine.Mathf.Lerp(velX, moveSpeed, Time.deltaTime * 3);
//
// 			animationController.Turn(Vector2.Right);
// 			animationController.SetAnimation(animationController.animRange_Run);
// 		}
// 		else
// 		{
// 			velX = Engine.Mathf.Lerp(velX, 0, Time.deltaTime * 5);
//
// 			animationController.SetAnimation(animationController.animRange_Idle);
// 		}
//
// 		if (KeyboardInput.WasKeyJustPressed(Keys.W))
// 		{
// 			//velY = jumpForce;
// 			rb.body.ApplyForce(Vector2.Up * jumpForce);
// 			animationController.Jump();
//
// 			// max velocity is limiting us...
//
//
// 			// it was moving up because of the ground collider.....
// 		}
//
// 		if (KeyboardInput.WasKeyJustPressed(Keys.Space))
// 		{
// 			animationController.MeleeAttack();
// 			FireFlameProjectile();
// 		}
//
// 		rb.Velocity = new Vector2(velX, velY);
//
// 		base.Update();
// 	}
//
// 	private void FireFlameProjectile()
// 	{
// 		GameObject flame = Serializer.I.LoadPrefab(flamePrefab.prefabPath);
// 		flame.transform.position = transform.position + new Vector2(0, -20);
// 		flame.transform.Rotation = transform.Rotation;
//
// 		float velX = flame.transform.Rotation.Y == 0 ? 1 : -1;
// 		flame.GetComponent<Flame>().rb.Velocity = new Vector2(velX * 15, 0);
// 		flame.GetComponent<Flame>().rb.BodyPos = flame.transform.position;
//
// 		float lifetime = 1;
// 		Tweener.Tween(1, 0, 0.5f, (f) => { flame.GetComponent<Renderer>().color = flame.GetComponent<Renderer>().color.SetA(f); }).SetDelay(lifetime).SetOnComplete(() => { flame.Destroy(); }).SetTarget(flame);
// 	}
// }