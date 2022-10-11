using System.Numerics;
using ImGuiNET;
using OpenTK.Mathematics;

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
					Camera.I.transform.position += Camera.I.transform.TransformVector(Vector3.Forward) * MouseInput.ScrollDelta;
				}
			}

			// PANNING
			if (MouseInput.IsButtonDown(MouseInput.Buttons.Right))
			{
				Camera.I.transform.position -= Camera.I.transform.TransformVector(MouseInput.ScreenDelta);
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