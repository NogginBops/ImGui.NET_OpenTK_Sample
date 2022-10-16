using System.Numerics;
using System.Xml.Serialization;
using OpenTK.Mathematics;

namespace Engine;

public class Camera : Component
{
	public bool isOrthographic = true;
	//public int antialiasingStrength = 0;
	public Color color = new(34, 34, 34);

	[ShowIf(nameof(isOrthographic))]
	public float ortographicSize = 2;
	[ShowIfNot(nameof(isOrthographic))]
	public float fieldOfView = 2;
	public float nearPlaneDistance = 1;
	public float farPlaneDistance = 50;

	//public float cameraSize = 0.1f;
	[XmlIgnore]
	public Matrix4x4 projectionMatrix;
	[XmlIgnore]
	public Matrix4x4 translationMatrix;

	public Vector2 size = new(1380, 900);
	[XmlIgnore]
	public Matrix4x4 viewMatrix;
	//[XmlIgnore] public RenderTarget2D renderTarget;

	public static Camera I { get; private set; }

	public override void Awake()
	{
		I = this;

		gameObject.alwaysUpdate = true;
		if (Global.EditorAttached == false)
		{
			size = new Vector2(Window.I.ClientSize.X, Window.I.ClientSize.Y);
		}

		projectionMatrix = GetProjectionMatrix();
		viewMatrix = GetViewMatrix();
		translationMatrix = GetTranslationRotationMatrix();
		/*	renderTarget = new RenderTarget2D(
		  Scene.I.GraphicsDevice,
		  (int)Size.X,
		  (int)Size.Y,
		  false,
		  Scene.I.GraphicsDevice.PresentationParameters.BackBufferFormat,
		  DepthFormat.Depth24);*/
	}

	public override void Update()
	{
		projectionMatrix = GetProjectionMatrix();
		viewMatrix = GetViewMatrix();
		translationMatrix = GetTranslationRotationMatrix();
		base.Update();
	}

	private Matrix4x4 GetViewMatrix()
	{
		//Matrix4x4 _view = Matrix4x4.CreateLookAt(new Vector3(0, 0, 30), new Vector3(0, 0, 0), new Vector3(0, 1, 0));

		Vector3 pos = transform.position;
		Vector3 forward = transform.TransformDirection(Vector3.Forward);
		Vector3 up = Vector3.Up;

		//Debug.Log($"pos:{pos}|forward:{forward}|up:{up}");

		//Matrix4x4 _view = Matrix4x4.CreateLookAt(forward, pos, up);
		Matrix4x4 _view = Matrix4x4.CreateWorld(pos,forward,up);
		// Matrix4x4 _view = Matrix4x4.CreateLookAt(pos,forward,up);
		
		// Matrix4x4 _view = Matrix4x4.CreateLookAt(transform.position,transform.TransformDirection(Vector3.Forward),transform.TransformDirection(Vector3.Up));
		return Matrix4x4.Identity;
	}

	private Matrix4x4 GetProjectionMatrix()
	{
		if (isOrthographic)
		{
			float left = -size.X / 2;
			float right = size.X / 2;
			float bottom = -size.Y / 2;
			float top = size.Y / 2;

			Matrix4x4 orthoMatrix = Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, 0.00001f, 10000000f);

			return translationMatrix * orthoMatrix * GetScaleMatrix();
		}
		else
		{
			fieldOfView = Mathf.ClampMin(fieldOfView, 0.0001f);
			nearPlaneDistance = Mathf.Clamp(nearPlaneDistance, 0.001f, farPlaneDistance);
			farPlaneDistance = Mathf.Clamp(farPlaneDistance, nearPlaneDistance + 0.001f, Mathf.Infinity);
			Matrix4x4 pm = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fieldOfView), size.X / size.Y, nearPlaneDistance, farPlaneDistance);

			return translationMatrix * pm;
		}
	}

	private Matrix4x4 GetTranslationRotationMatrix()
	{
		Matrix4x4 _tr = Matrix4x4.CreateTranslation(-transform.position.X * Units.OneWorldUnit, -transform.position.Y * Units.OneWorldUnit, transform.position.Z * Units.OneWorldUnit);
		Matrix4x4 rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(transform.Rotation.Y / 180 * Mathf.Pi,
		                                                            -transform.Rotation.X / 180 * Mathf.Pi,
		                                                            -transform.Rotation.Z / 180 * Mathf.Pi);
		return _tr * rotationMatrix;
	}

	private Matrix4x4 GetScaleMatrix()
	{
		Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(1 / ortographicSize);
		return scaleMatrix;
	}

	public void Move(Vector2 moveVector)
	{
		transform.position += moveVector;
	}

	public Vector2 WorldToScreen(Vector2 worldPosition)
	{
		return Vector2.Transform(worldPosition, GetTranslationRotationMatrix());
	}

	public Vector2 ScreenToWorld(Vector2 screenPosition)
	{
		return Vector2.Transform(screenPosition / size * 2,
		                         Matrix.Invert(GetProjectionMatrix()))
		     - size * ortographicSize / 2;
	}

	public Vector2 CenterOfScreenToWorld()
	{
		return ScreenToWorld(new Vector2(size.X / 2, size.Y / 2));
	}

	public bool RectangleVisible(BoxShape shape)
	{
		bool isIn = Vector2.Distance(shape.transform.position, transform.position) < size.X * 1.1f * (ortographicSize / 2) + shape.size.X / 2 * shape.transform.scale.MaxVectorMember();

		return isIn;
	}
}