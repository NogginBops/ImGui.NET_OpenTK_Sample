namespace Scripts;

public class AnimationController : Component
{
	public float animationSpeed = 1;

	public Vector2 animRange_Idle = new(0, 0);
	public Vector2 animRange_Jump = new(0, 0);
	public Vector2 animRange_Run = new(0, 0);
	public Vector2 animRange_MeeleeAttack = new(0, 0);
	private Vector2? forcedAnimation = null;
	public Vector2 currentAnimRange = new(0, 0);

	private Action OnAnimationFinished = () => { };
	private SpriteSheetRenderer spriteSheetRenderer;
	private float timeOnCurrentFrame;

	public override void Awake()
	{
		spriteSheetRenderer = GetComponent<SpriteSheetRenderer>();
		base.Awake();
	}

	public override void Start()
	{
		//SetAnimation(animRange_Idle);

		base.Start();
	}

	public override void Update()
	{
		if (animationSpeed == 0)
		{
			return;
		}

		timeOnCurrentFrame += Time.deltaTime * animationSpeed;
		while (timeOnCurrentFrame > 1 / animationSpeed)
		{
			timeOnCurrentFrame -= 1 / animationSpeed;
			if (spriteSheetRenderer.currentSpriteIndex + 1 >= currentAnimRange.Y)
			{
				spriteSheetRenderer.currentSpriteIndex = (int) currentAnimRange.X;
				OnAnimationFinished?.Invoke();
			}
			else
			{
				spriteSheetRenderer.currentSpriteIndex++;
			}
		}

		base.Update();
	}

	public void ResetCurrentAnimation()
	{
		timeOnCurrentFrame = 0;
		spriteSheetRenderer.currentSpriteIndex = (int) currentAnimRange.X;
	}

	public void Turn(Vector2 direction)
	{
		if (direction == Vector2.Right)
		{
			transform.Rotation = new Vector3(transform.Rotation.X, 0, transform.Rotation.Z);
		}
		else
		{
			transform.Rotation = new Vector3(transform.Rotation.X, 180, transform.Rotation.Z);
		}
	}

	public void Jump()
	{
		forcedAnimation = animRange_Jump;
		SetAnimation(forcedAnimation.Value);
		animationSpeed = 4.5f;
		OnAnimationFinished += () =>
		{
			forcedAnimation = null;

			SetAnimation(animRange_Idle);
			animationSpeed = 3;

			OnAnimationFinished = () => { };
		};
	}

	public void MeleeAttack()
	{
		forcedAnimation = animRange_MeeleeAttack;
		SetAnimation(forcedAnimation.Value);
		animationSpeed = 3f;
		OnAnimationFinished += () =>
		{
			forcedAnimation = null;

			SetAnimation(animRange_Idle);
			animationSpeed = 3;

			OnAnimationFinished = () => { };
		};
	}

	public void SetAnimation(Vector2 animRange)
	{
		if (forcedAnimation != null && forcedAnimation != animRange)
		{
			return;
		}

		var oldAnim = currentAnimRange;

		currentAnimRange = animRange;

		if (oldAnim != currentAnimRange)
		{
			ResetCurrentAnimation();
		}
	}
}