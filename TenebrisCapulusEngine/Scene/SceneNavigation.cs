using System.Numerics;

namespace Engine;

public class SceneNavigation
{
	public static SceneNavigation I { get; private set; }
	private float targetOrthoSize = 1;

	public SceneNavigation()
	{
		I = this;
	}

	public void Update()
	{
		if (TransformHandle.I.clicked)
		{
			return;
		}

		Matrix4x4 dir = Matrix4x4.Identity
		              * Matrix4x4.CreateTranslation(Vector3.Backward)
		              * Matrix4x4.CreateFromYawPitchRoll(Camera.I.transform.Rotation.Y / 180 * Mathf.Pi,
		                                                 -Camera.I.transform.Rotation.X / 180 * Mathf.Pi,
		                                                 -Camera.I.transform.Rotation.Z / 180 * Mathf.Pi)
		              * Matrix4x4.CreateRotationZ(-Camera.I.transform.Rotation.Z / 180f * Mathf.Pi);
		Debug.Log("Camera direction:" + dir.Translation);
		if (Global.EditorAttached)
		{
			// Z POSITION
			if (MouseInput.ScrollDelta != 0)
			{
				if (Camera.I.isOrthographic)
				{
					targetOrthoSize += -MouseInput.ScrollDelta * (targetOrthoSize > 1 ? targetOrthoSize * 0.1f : 0.05f);
					targetOrthoSize = Mathf.Clamp(targetOrthoSize, 0.1f, Mathf.Infinity);
					Camera.I.ortographicSize = Mathf.Eerp(Camera.I.ortographicSize, targetOrthoSize, Time.editorDeltaTime * 7f);
				}
				else
				{
					Camera.I.transform.position += new Vector3(dir.Translation.X, dir.Translation.Y, dir.Translation.Z) * MouseInput.ScrollDelta;
				}
			}

			// PANNING
			if (MouseInput.IsButtonDown(MouseInput.Buttons.Right))
			{
				Camera.I.transform.position -= MouseInput.ScreenDelta;
				MouseInput.ScreenDelta -= MouseInput.ScreenDelta;
			}

			// ROTATING
			if (MouseInput.IsButtonDown(MouseInput.Buttons.Left))
			{
				Camera.I.transform.Rotation += new Vector3(MouseInput.ScreenDelta.Y, MouseInput.ScreenDelta.X, 0) * 0.5f;
			}
		}
	}
}