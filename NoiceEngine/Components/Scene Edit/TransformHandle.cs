namespace Engine;

public class TransformHandle : Component
{
	public enum Axis
	{
		X,
		Y,
		XY
	}

	public static bool objectSelected;
	public BoxShape boxColliderX;

	public BoxShape boxColliderXY;
	public BoxShape boxColliderY;
	public BoxRenderer boxRendererX;
	public BoxRenderer boxRendererXY;
	public BoxRenderer boxRendererY;
	
	public bool clicked;
	public Axis? CurrentAxisSelected;
	private Transform selectedTransform;

	public override void Awake()
	{
		objectSelected = false;
		gameObject.updateWhenDisabled = true;

		gameObject.AddComponent<Rigidbody>().useGravity = true;
		GetComponent<Rigidbody>().isStatic = false;
		GetComponent<Rigidbody>().isButton = true;

		if (GetComponents<BoxRenderer>().Count > 2)
		{
			boxColliderXY = GetComponent<BoxShape>(0);
			boxColliderX = GetComponent<BoxShape>(1);
			boxColliderY = GetComponent<BoxShape>(2);

			boxRendererXY = GetComponent<BoxRenderer>(0);
			boxRendererX = GetComponent<BoxRenderer>(1);
			boxRendererY = GetComponent<BoxRenderer>(2);
		}
		else
		{
			boxColliderXY = gameObject.AddComponent<BoxShape>();
			boxColliderXY.size = new Vector2(15, 15);
			boxColliderXY.offset = new Vector2(5, 5);

			boxColliderX = gameObject.AddComponent<BoxShape>();
			boxColliderX.size = new Vector2(50, 5);
			//boxColliderX.offset = new Vector2(25, 2.5f);

			boxColliderY = gameObject.AddComponent<BoxShape>();
			boxColliderY.size = new Vector2(5, 50);
			//boxColliderY.offset = new Vector2(2.5f, 25);

			boxRendererXY = gameObject.AddComponent<BoxRenderer>();
			boxRendererX = gameObject.AddComponent<BoxRenderer>();
			boxRendererY = gameObject.AddComponent<BoxRenderer>();

			boxRendererXY.Layer = 1000;
			boxRendererX.Layer = 1000;
			boxRendererY.Layer = 1000;

			boxRendererXY.color = Color.Orange;
			boxRendererX.color = Color.Red;
			boxRendererY.color = Color.Cyan;

			boxRendererX.boxShape = boxColliderX;
			boxRendererXY.boxShape = boxColliderXY;
			boxRendererY.boxShape = boxColliderY;
		}

		base.Awake();
	}

	private void SetSelectedObjectRigidbodyAwake(bool tgl)
	{
		if (selectedTransform?.HasComponent<Rigidbody>() == true && selectedTransform?.GetComponent<Rigidbody>().body?.Awake == false)
		{
			selectedTransform.GetComponent<Rigidbody>().body.Awake = tgl;
		}
	}

	public override void Update()
	{
		transform.scale = Vector3.One * Global.EditorScale * Camera.I.ortographicSize;

		if (MouseInput.ButtonReleased())
		{
			CurrentAxisSelected = null;
		}

		if (MouseInput.ButtonPressed())
		{
			clicked = false;
			if (MouseInput.WorldPosition.In(boxColliderX))
			{
				CurrentAxisSelected = Axis.X;
				clicked = true;
			}

			if (MouseInput.WorldPosition.In(boxColliderY))
			{
				CurrentAxisSelected = Axis.Y;
				clicked = true;
			}

			if (MouseInput.WorldPosition.In(boxColliderXY))
			{
				CurrentAxisSelected = Axis.XY;
				clicked = true;
			}
		}

		if (MouseInput.IsButtonDown() && gameObject.activeInHierarchy && clicked)
		{
			SetSelectedObjectRigidbodyAwake(false);
			Move(MouseInput.WorldDelta);
		}
		else
		{
			SetSelectedObjectRigidbodyAwake(true);
		}

		if (objectSelected == false || selectedTransform == null)
			//GameObject.Active = false;
		{
			return;
		}

		transform.position = selectedTransform.position;
		if (MouseInput.WorldPosition.In(boxColliderX) || CurrentAxisSelected == Axis.X)
		{
			boxRendererX.color = Color.WhiteSmoke;
		}
		else
		{
			boxRendererX.color = Color.Red;
		}

		if (MouseInput.WorldPosition.In(boxColliderY) || CurrentAxisSelected == Axis.Y)
		{
			boxRendererY.color = Color.WhiteSmoke;
		}
		else
		{
			boxRendererY.color = Color.Cyan;
		}

		if (MouseInput.WorldPosition.In(boxColliderXY) || CurrentAxisSelected == Axis.XY)
		{
			boxRendererXY.color = Color.WhiteSmoke;
		}
		else
		{
			boxRendererXY.color = Color.Orange;
		}

		base.Update();
	}

	public void Move(Vector3 deltaVector)
	{
		Vector3 moveVector = Vector3.Zero;
		switch (CurrentAxisSelected)
		{
			case Axis.X:
				moveVector += deltaVector.VectorX();
				break;
			case Axis.Y:
				moveVector += deltaVector.VectorY();
				break;
			case Axis.XY:
				moveVector += deltaVector;
				break;
		}

		transform.position += moveVector; // we will grab it with offset, soe we want to move it only by change of mouse position
		selectedTransform.position = transform.position;

		// todo just do the position delta move in transform component for (int i = 0; i < selectedTransform.children.Count; i++) selectedTransform.children[i].position += moveVector;

		if (selectedTransform.HasComponent<Rigidbody>() && selectedTransform.GetComponent<Rigidbody>().isButton == false)
		{
			lock (Physics.World)
			{
				Rigidbody rigidbody = selectedTransform.GetComponent<Rigidbody>();
				rigidbody.Velocity = Vector2.Zero;
				if (rigidbody.body != null)
				{
					rigidbody.body.Position = selectedTransform.position;
				}
			}
		}

		if (KeyboardInput.IsKeyDown(Keys.LeftShift))
		{
			switch (CurrentAxisSelected)
			{
				case Axis.X:
					selectedTransform.position = new Vector3(MouseInput.WorldPosition.TranslateToGrid().X, selectedTransform.position.Y, 0);
					break;
				case Axis.Y:
					selectedTransform.position = new Vector3(selectedTransform.position.X, MouseInput.WorldPosition.TranslateToGrid().Y, 0);
					break;
				case Axis.XY:
					selectedTransform.position = MouseInput.WorldPosition.TranslateToGrid(50);
					break;
			}
		}
	}

	public void SelectObject(GameObject selectedGO)
	{
		gameObject.activeSelf = selectedGO != null;

		if (selectedGO == null)
		{
			objectSelected = false;
			return;
		}

		transform.position = selectedGO.transform.position;
		selectedTransform = selectedGO.transform;
		objectSelected = true;
	}
}