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
		onReleasedAction += SpawnCubes;

		if (GetComponent<ButtonTween>() == null)
		{
			gameObject.AddComponent<ButtonTween>().Awake();
		}

		renderer = GetComponent<Renderer>();
		boxShape = GetComponent<BoxShape>();

		base.Awake();
	}

	private void SpawnCubes()
	{
		for (int x = 0; x < 10; x++)
		for (int y = 0; y < 10; y++)
		{
			GameObject go2 = GameObject.Create(name: "box " + y);
			go2.AddComponent<BoxRenderer>();
			go2.GetComponent<BoxRenderer>().color = Rendom.RandomColor();

			go2.AddComponent<BoxShape>().size = new Vector2(50, 50);
			go2.AddComponent<Rigidbody>();
			go2.GetComponent<Rigidbody>().useGravity = true;
			go2.Awake();

			go2.transform.SetParent(transform);
			go2.transform.position = new Vector2(transform.position.X + x * 50, transform.position.Y + y * 50);
			lock (Physics.World)
			{
				go2.GetComponent<Rigidbody>().body.Position = go2.transform.position;
				go2.GetComponent<Rigidbody>().body.Mass = 1000;
			}

			go2.GetComponent<Rigidbody>().Velocity = new Vector2(Rendom.Range(-100, 100), Rendom.Range(-100, 100));

			go2.transform.pivot = new Vector2(0.5f, 0.5f);
		}
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