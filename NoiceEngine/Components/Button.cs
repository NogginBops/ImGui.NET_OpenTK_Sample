using System.Xml.Serialization;
using Engine.UI;

namespace Engine;

public class Button : Component
{
	public delegate void MouseAction();

	[LinkableComponent] public BoxShape boxShape;
	private bool clicked;
	private bool mouseIsOver;
	[XmlIgnore] private MouseAction onClickedAction;
	[XmlIgnore] public MouseAction onReleasedAction;

	[LinkableComponent] public Renderer renderer;

	public override void Awake()
	{
		//onClickedAction += () => renderer.color = new Color(215, 125, 125);
		//onReleasedAction += () => renderer.color = Color.White;
		//onReleasedAction += SpawnCubes;

		if (GetComponent<ButtonTween>() == null)
		{
			gameObject.AddComponent<ButtonTween>().Awake();
		}

		renderer = GetComponent<Renderer>();
		boxShape = GetComponent<BoxShape>();

		base.Awake();
	}

	public override void Update()
	{
		if (renderer == false || boxShape == false)
		{
			return;
		}

		mouseIsOver = MouseInput.WorldPosition.In(boxShape);
		if (MouseInput.ButtonPressed() && mouseIsOver)
		{
			onClickedAction?.Invoke();
			clicked = true;
		}
		else if (MouseInput.ButtonReleased())
		{
			if (mouseIsOver)
			{
				onReleasedAction?.Invoke();
			}

			clicked = false;
		}

		if (clicked == false)
		{
			//renderer.color = mouseIsOver ? Color.Gray : Color.White;
		}

		if (clicked && mouseIsOver == false) // up event when me move out of button bounds, even when clicked
		{
			onReleasedAction?.Invoke();
			clicked = false;
		}
		//renderer.color = Color.Black;
	}
}