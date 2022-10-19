using Tofu3D.Tweening;

namespace Tofu3D;

public class SceneNavigation
{
	public static SceneNavigation I { get; private set; }
	private float targetOrthoSize = -1;

	private bool clickedInsideScene = false;

	public SceneNavigation()
	{
		I = this;
	}

	public void MoveToGameObject(GameObject targetGO)
	{
		Vector3 cameraStartPos = Camera.I.transform.position;
		Vector3 cameraEndPos = targetGO.transform.position + new Vector3(0, 0, -4);

		if (cameraStartPos == cameraEndPos)
		{
			cameraEndPos = targetGO.transform.position + new Vector3(0, 0, -2);
		}

		Tweener.Tween(0, 1, 0.3f, (progress) => { Camera.I.transform.position = Vector3.Lerp(cameraStartPos, cameraEndPos, progress); });
	}

	public void Update()
	{
		if (targetOrthoSize == -1 && Camera.I != null)
		{
			targetOrthoSize = Camera.I.orthographicSize;
		}
		if (TransformHandle.I.clicked)
		{
			return;
		}

		if (Global.EditorAttached == false)
		{
			return;
		}

		if (KeyboardInput.WasKeyJustPressed(Keys.F))
		{
			MoveToGameObject(Editor.I.GetSelectedGameObject());
		}


		bool isMouseOverSceneView = MouseInput.ScreenPosition.X < Camera.I.size.X && MouseInput.ScreenPosition.Y < Camera.I.size.Y;

		bool justClicked = MouseInput.ButtonPressed();
		if (justClicked)
		{
			clickedInsideScene = isMouseOverSceneView;
		}

		if (isMouseOverSceneView)
		{
			if ((justClicked && clickedInsideScene) || justClicked == false)
			{
				HandleMouseControls();
			}
		}
	}

	private void HandleMouseControls()
	{
		// Z POSITION
		if (MouseInput.ScrollDelta != 0)
		{
			if (Camera.I.isOrthographic)
			{
				targetOrthoSize += -MouseInput.ScrollDelta * ( targetOrthoSize*0.04f);
				targetOrthoSize = Mathf.Clamp(targetOrthoSize, 0.1f, Mathf.Infinity);
				// Camera.I.ortographicSize = Mathf.Eerp(Camera.I.ortographicSize, targetOrthoSize, Time.editorDeltaTime * 10f);
				// macbook trackpad has smooth scrolling so no eerping
				Camera.I.orthographicSize = targetOrthoSize;
			}
			else
			{
				MoveCameraInDirection(new Vector3(0, 0, Mathf.ClampMax(MouseInput.ScrollDelta, 5)));

				//Camera.I.transform.position += Camera.I.transform.TransformDirection(Vector3.Forward) * MouseInput.ScrollDelta * 0.05f;
			}
		}

		// PANNING
		if (MouseInput.IsButtonDown(MouseInput.Buttons.Left))
		{
			//MoveCameraInDirection(new Vector2(-MouseInput.ScreenDelta.X, -MouseInput.ScreenDelta.Y));
			Camera.I.transform.position -= Camera.I.transform.TransformDirection(new Vector2(MouseInput.ScreenDelta.X, MouseInput.ScreenDelta.Y)) / Units.OneWorldUnit * Camera.I.orthographicSize;
			MouseInput.ScreenDelta -= MouseInput.ScreenDelta;
		}

		// ROTATING
		if (MouseInput.IsButtonDown(MouseInput.Buttons.Left) && false)
		{
			Camera.I.transform.Rotation += new Vector3(MouseInput.ScreenDelta.Y, MouseInput.ScreenDelta.X, 0) * 0.2f;
			//Camera.I.transform.Rotation = new Vector3(Camera.I.transform.Rotation.X,Camera.I.transform.Rotation .Y, 0);

			Vector3 keyboardInputDirectionVector = Vector3.Zero;
			if (KeyboardInput.IsKeyDown(Keys.W))
			{
				keyboardInputDirectionVector += Vector3.Forward;
			}

			if (KeyboardInput.IsKeyDown(Keys.S))
			{
				keyboardInputDirectionVector += Vector3.Backward;
			}

			if (KeyboardInput.IsKeyDown(Keys.A))
			{
				keyboardInputDirectionVector += Vector3.Left;
			}

			if (KeyboardInput.IsKeyDown(Keys.D))
			{
				keyboardInputDirectionVector += Vector3.Right;
			}

			float moveSpeed = 0.02f;
			if (KeyboardInput.IsKeyDown(Keys.LeftShift))
			{
				moveSpeed = 0.04f;
			}

			MoveCameraInDirection(keyboardInputDirectionVector, moveSpeed);
		}
	}

	private void MoveCameraInDirection(Vector3 dir, float moveSpeed = 0.02f)
	{
		Camera.I.transform.position += Camera.I.transform.TransformDirection(dir) * moveSpeed;
	}
}