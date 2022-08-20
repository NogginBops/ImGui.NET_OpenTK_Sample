namespace Engine.UI;

public class ButtonTween : Component
{
	private bool clicked;
	public float scaleSpeed = 20;
	public float scaleTarget = 0.9f;

	public override void Awake()
	{
		base.Awake();
	}

	public override void Update()
	{
		//if (needToScale == false) { return; }
		bool mouseInside = MouseInput.WorldPosition.In(GetComponent<BoxShape>());
		if (MouseInput.ButtonPressed() && mouseInside)
		{
			transform.scale = Vector3.One;

			clicked = true;
		}
		else if (MouseInput.ButtonReleased())
		{
			clicked = false;
		}

		if (clicked)
		{
			transform.scale = Vector3.Lerp(transform.scale, Vector3.One * scaleTarget, Time.deltaTime * scaleSpeed);
		}
		else
		{
			transform.scale = Vector3.Lerp(transform.scale, Vector3.One, Time.deltaTime * scaleSpeed);
		}
	}
}