/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Engine;
using KeyboardInput = Engine.KeyboardInput;

namespace Scripts
{
	public class MovementController : Component
	{
		[ShowInEditor] public float MoveSpeed { get; set; } = 10;
		[ShowInEditor] public float jumpForce = 10000;
		[LinkableComponent]
		private Rigidbody rb;
		bool jumpKeyDown = false;
		float targetSpeedX = 0;

		[ShowInEditor] public float CameraOffsetY = 10;

		[LinkableComponent] private AnimationController animationController;
		public override void Awake()
		{
			rb = GetComponent<Rigidbody>();
			animationController = GetComponent<AnimationController>();

			base.Awake();
		}
		float jumpXSpeedMultiplier = 1;
		public override void FixedUpdate()
		{
			if (rb == null) return;

			bool pressedLeft = KeyboardInput.IsKeyDown(GLFW.Keys.A);
			bool pressedRight = KeyboardInput.IsKeyDown(GLFW.Keys.D);

			if (pressedLeft || pressedRight)
			{
				animationController.SetAnimation(animationController.AnimRange_Run);
			}
			else
			{
				animationController.SetAnimation(animationController.AnimRange_Idle);
			}

			Vector2 input = Vector2.Zero;
			if (pressedLeft)
			{
				input.X = -1;
				animationController.Turn(Vector2.Left);
			}
			else if (pressedRight)
			{
				input.X = 1;
				animationController.Turn(Vector2.Right);
			}

			if (jumpKeyDown == false && KeyboardInput.IsKeyDown(GLFW.Keys.W))
			{
				jumpXSpeedMultiplier = 5f;
				//rb.body.ApplyForce(new Vector2(0, -jumpForce));
				animationController.Jump();
			}
			if (animationController.jumping)
			{
				jumpXSpeedMultiplier = MathHelper.Lerp(jumpXSpeedMultiplier, 1.8f, Time.deltaTime * 10);
			}
			else
			{
				jumpXSpeedMultiplier = 1;
			}
			jumpKeyDown = KeyboardInput.IsKeyDown(GLFW.Keys.W);

			if (pressedLeft || pressedRight)
			{
				targetSpeedX = MathHelper.Lerp(targetSpeedX, input.X * MoveSpeed * jumpXSpeedMultiplier, Time.deltaTime * 6);
			}
			else
			{
				targetSpeedX = MathHelper.Lerp(targetSpeedX, input.X * MoveSpeed, Time.deltaTime * 15);
			}
			//rb.body.ApplyForce(new Vector2(targetSpeedX, 0));

			Vector2 targetPos = new Vector2(transform.position.X - Camera.I.Size.X / (2f / Camera.I.CameraSize) + 35, (Player.I.transform.position.Y + CameraOffsetY) * 0.4f);
			Camera.I.transform.position = Vector2.Lerp(Camera.I.transform.position, targetPos, Time.deltaTime * 9);
			base.Update();
		}
	}
}
*/

