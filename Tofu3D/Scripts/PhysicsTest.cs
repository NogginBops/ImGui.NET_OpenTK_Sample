using Tofu3D.Physics;

public class PhysicsTest : Component
{
	private BoxShape boxShape;
	private ModelRenderer modelRenderer;

	public override void Awake()
	{
		boxShape = GetComponent<BoxShape>();
		modelRenderer = GetComponent<ModelRenderer>();
		base.Awake();
	}

	public override void Start()
	{
		base.Start();
	}

	public override void Update()
	{
		if (boxShape == null || modelRenderer == null)
		{
			return;
		}

		// Ray ray = new Ray(Camera.I.ScreenToWorld(MouseInput.ScreenPosition)*1000, Camera.I.TransformToWorld(Vector3.Forward).Normalized());
		Ray ray = new Ray(Camera.I.TransformToWorld(MouseInput.ScreenPosition), Camera.I.TransformToWorld(MouseInput.ScreenPosition) + Camera.I.TransformToWorld(Vector3.Forward).Normalized());

		RaycastResult result = Physics.Raycast(ray: ray);

		//Debug.Log("Center of screen in world:"+Camera.I.CenterOfScreenToWorld());
		Debug.Log("Mouse world pos:" + ray.origin);
		// if (result.hitBodies.Count > 0)
		// {
		// 	modelRenderer.color = Color.Red;
		// }
		// else
		// {
		// 	modelRenderer.color = Color.White;
		// }

		base.Update();
	}
}