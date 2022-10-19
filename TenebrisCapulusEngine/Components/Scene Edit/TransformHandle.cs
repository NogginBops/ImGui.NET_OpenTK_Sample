namespace Tofu3D;

public class TransformHandle : Component
{
	public static TransformHandle I { get; private set; }

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
	//public BoxShape boxColliderZ;
	public ModelRenderer boxRendererX;
	public ModelRenderer boxRendererXY;
	public ModelRenderer boxRendererY;
	//public ModelRenderer boxRendererZ;

	public bool clicked;
	public Axis? CurrentAxisSelected;
	private Transform selectedTransform;

	public override void Awake()
	{
		I = this;
		objectSelected = false;
		gameObject.updateWhenDisabled = true;

		if (GetComponents<BoxRenderer>().Count > 2)
		{
			boxColliderXY = GetComponent<BoxShape>(0);
			boxColliderX = GetComponent<BoxShape>(1);
			boxColliderY = GetComponent<BoxShape>(2);
			//boxColliderZ = GetComponent<BoxShape>(3);

			boxRendererXY = GetComponent<ModelRenderer>(0);
			boxRendererX = GetComponent<ModelRenderer>(1);
			boxRendererY = GetComponent<ModelRenderer>(2);
			//boxRendererZ = GetComponent<ModelRenderer>(3);
		}
		else
		{
			boxColliderX = gameObject.AddComponent<BoxShape>();
			boxColliderX.size = new Vector3(50, 5, 5) / Units.OneWorldUnit;
			//boxColliderX.offset = new Vector2(25, 2.5f);

			boxColliderY = gameObject.AddComponent<BoxShape>();
			boxColliderY.size = new Vector3(5, 50, 5) / Units.OneWorldUnit;

			// boxColliderZ = gameObject.AddComponent<BoxShape>();
			// boxColliderZ.size = new Vector3(5, 5, 50)/Units.OneWorldUnit;
			//boxColliderY.offset = new Vector2(2.5f, 25);

			boxColliderXY = gameObject.AddComponent<BoxShape>();
			boxColliderXY.size = new Vector3(10, 10, 10) / Units.OneWorldUnit;
			//boxColliderXY.offset = new Vector3(5, 5,-5)/Units.OneWorldUnit;

			boxRendererX = gameObject.AddComponent<ModelRenderer>();
			boxRendererY = gameObject.AddComponent<ModelRenderer>();
			// boxRendererZ = gameObject.AddComponent<ModelRenderer>();
			boxRendererXY = gameObject.AddComponent<ModelRenderer>();


			PremadeComponentSetups.PrepareCube(boxRendererX);
			PremadeComponentSetups.PrepareCube(boxRendererY);
			PremadeComponentSetups.PrepareCube(boxRendererXY);
			// PremadeComponentSetups.PrepareCube(boxRendererZ);

			boxRendererXY.Layer = 1000;
			boxRendererX.Layer = 1000;
			boxRendererY.Layer = 1000;
			// boxRendererZ.Layer = 1000;

			boxRendererX.boxShape = boxColliderX;
			boxRendererXY.boxShape = boxColliderXY;
			boxRendererY.boxShape = boxColliderY;
			// boxRendererZ.boxShape = boxColliderZ;
		}

		base.Awake();
	}

	private void SetSelectedObjectRigidbodyAwake(bool tgl)
	{
		// if (selectedTransform?.HasComponent<Rigidbody>() == true & selectedTransform?.GetComponent<Rigidbody>().body?.Awake == false)
		// {
		// 	selectedTransform.GetComponent<Rigidbody>().body.Awake = tgl;
		// }
	}

	public override void Update()
	{
		if (Camera.I.isOrthographic)
		{
			transform.scale = Vector3.One * Global.EditorScale * Camera.I.orthographicSize * 1.5f;
		}
		else
		{
			transform.scale = Vector3.One * Vector3.Distance(transform.position, Camera.I.transform.position) * 0.3f;
		}

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
			boxRendererXY.color = Color.Gold;
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

		// if (selectedTransform.HasComponent<Rigidbody>() && selectedTransform.GetComponent<Rigidbody>().isButton == false)
		// {
		// 	lock (Physics.World)
		// 	{
		// 		Rigidbody rigidbody = selectedTransform.GetComponent<Rigidbody>();
		// 		rigidbody.Velocity = Vector2.Zero;
		// 		if (rigidbody.body != null)
		// 		{
		// 			rigidbody.body.Position = selectedTransform.position;
		// 		}
		// 	}
		// }

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