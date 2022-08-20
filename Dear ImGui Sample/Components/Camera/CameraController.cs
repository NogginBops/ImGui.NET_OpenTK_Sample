namespace Scripts;

public class CameraController : Component
{
	private float targetOrthoSize = 1;

	public override void Start()
	{
		targetOrthoSize = Camera.I.ortographicSize;
		base.Start();
	}

	public override void Update()
	{
		if (Global.EditorAttached)
		{
			if (MouseInput.IsButtonDown(MouseInput.Buttons.Right))
			{
				transform.position -= MouseInput.WorldDelta;
				MouseInput.ScreenDelta -= MouseInput.ScreenDelta;
			}

			targetOrthoSize += -MouseInput.ScrollDelta * (targetOrthoSize > 1 ? targetOrthoSize * 0.1f : 0.05f);
			targetOrthoSize = Mathf.Clamp(targetOrthoSize, 0.1f, Mathf.Infinity);
			Camera.I.ortographicSize = Mathf.Eerp(Camera.I.ortographicSize, targetOrthoSize, Time.editorDeltaTime * 7f);
		}

		base.Update();
	}
}