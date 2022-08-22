namespace Scripts;

public class FollowMouse : Component
{
	public override void Update()
	{
		transform.position = MouseInput.ScreenPosition;
	}
}