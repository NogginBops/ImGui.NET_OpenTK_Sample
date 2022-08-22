/*namespace Scripts;

public class AnimationController : Component
{
	public float animationSpeed = 1;

	public Vector2 animRange_Idle = new(0, 0);
	public Vector2 animRange_Jump = new(0, 0);
	public Vector2 animRange_Run = new(0, 0);
	public Vector2 currentAnimRange = new(0, 0);
	public bool jumping;

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
		SetAnimation(animRange_Idle);

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
			transform.rotation.Y = 0;
		}
		else
		{
			transform.rotation.Y = 180;
		}
	}

	public void Jump()
	{
		jumping = true;
		SetAnimation(animRange_Jump);
		animationSpeed = 4.5f;
		OnAnimationFinished += () =>
		{
			SetAnimation(animRange_Run);
			animationSpeed = 3;

			jumping = false;
			OnAnimationFinished = () => { };
		};
	}

	public void SetAnimation(Vector2 animRange)
	{
		if (jumping && animRange != animRange_Jump)
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
}*/

