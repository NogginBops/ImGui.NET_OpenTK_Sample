using System.Numerics;
using System.Xml.Serialization;
using Engine.Components.Renderers;
using OpenTK.Mathematics;
using Vector2 = Engine.Vector2;
using Vector4 = Engine.Vector4;

namespace Scripts;

public class Renderer : Component, IComparable<Renderer>
{
	[LinkableComponent]
	public BoxShape boxShape;
	public Color color = Color.White;

	private float layer;

	[Hide]
	public float layerFromHierarchy = 0;

	[Show]
	public Material material;
	internal bool onScreen = true;
	public float distanceFromCamera;
	public float Layer
	{
		get { return layer; }
		set
		{
			layer = value;
			//Scene.I?.RenderQueueChanged();
		}
	}
	[XmlIgnore] public Matrix4x4 LatestModelViewProjection { get; private set; }

	// public int CompareTo(Renderer comparePart)
	// {
	// 	// A null value means that this object is greater.
	// 	if (comparePart == null)
	// 	{
	// 		return 1;
	// 	}
	//
	// 	return Layer.CompareTo(comparePart.Layer + comparePart.layerFromHierarchy);
	// }
	public int CompareTo(Renderer comparePart)
	{
		// A null value means that this object is greater.
		if (comparePart == null)
		{
			return 1;
		}

		return comparePart.distanceFromCamera.CompareTo(distanceFromCamera);
	}

	public override void Awake()
	{
		CreateMaterial();

		base.Awake();
	}

	public virtual void CreateMaterial()
	{
		material.SetShader(material.shader);
	}

	// private Matrix4x4 GetModelViewProjectionOld()
	// {
	// 	Vector2 pivotOffset = -(boxShape.size * transform.scale) / 2 + new Vector2(boxShape.size.X * transform.scale.X * transform.pivot.X, boxShape.size.Y * transform.scale.Y * transform.pivot.Y);
	// 	Matrix4x4 _translation = Matrix4x4.CreateTranslation(transform.position + boxShape.offset * transform.scale - pivotOffset);
	//
	// 	Matrix4x4 _rotation = Matrix4x4.CreateFromYawPitchRoll(transform.rotation.Y / 180 * Mathf.Pi,
	// 	                                                       transform.rotation.X / 180 * Mathf.Pi,
	// 	                                                       transform.rotation.Z / 180 * Mathf.Pi);
	// 	Matrix4x4 _scale = Matrix4x4.CreateScale(boxShape.size.X * transform.scale.X, boxShape.size.Y * transform.scale.Y, 1);
	//
	// 	return _scale * Matrix4x4.Identity * _rotation * _translation * Camera.I.viewMatrix * Camera.I.projectionMatrix;
	// }

	public virtual Matrix4x4 GetModelViewProjectionFromBoxShape()
	{
		return GetModelMatrix() * Camera.I.viewMatrix * Camera.I.projectionMatrix;
	}

	public Matrix4x4 GetModelMatrix()
	{
		Vector3 pivotOffset = -(boxShape.size * transform.scale) / 2
		                    + new Vector3(boxShape.size.X * transform.scale.X * transform.pivot.X,
		                                  boxShape.size.Y * transform.scale.Y * transform.pivot.Y,
		                                  boxShape.size.Z * transform.scale.Z * transform.pivot.Z);

		Matrix4x4 _pivot = Matrix4x4.CreateTranslation(-pivotOffset.X, -pivotOffset.Y, -pivotOffset.Z);
		Matrix4x4 _translation = Matrix4x4.CreateTranslation(transform.position + boxShape.offset * transform.scale) * Matrix4x4.CreateScale(1, 1, -1);

		Matrix4x4 _rotation = Matrix4x4.CreateFromYawPitchRoll(transform.Rotation.Y / 180 * Mathf.Pi,
		                                                       -transform.Rotation.X / 180 * Mathf.Pi,
		                                                       -transform.Rotation.Z / 180 * Mathf.Pi);
		Matrix4x4 _scale = Matrix4x4.CreateScale(boxShape.size.X * transform.scale.X, boxShape.size.Y * transform.scale.Y, transform.scale.Z * boxShape.size.Z);
		return (_scale * Matrix4x4.Identity * _pivot * _rotation * _translation) * Matrix4x4.CreateScale(Units.OneWorldUnit);
	}

	public Matrix4x4 GetMVPForOutline()
	{
		Vector3 pivotOffset = -(boxShape.size * transform.scale) / 2
		                    + new Vector3(boxShape.size.X * transform.scale.X * transform.pivot.X,
		                                  boxShape.size.Y * transform.scale.Y * transform.pivot.Y,
		                                  boxShape.size.Z * transform.scale.Z * transform.pivot.Z);
		
		Matrix4x4 _pivot = Matrix4x4.CreateTranslation(-pivotOffset.X, -pivotOffset.Y, -pivotOffset.Z);
		Matrix4x4 _translation = Matrix4x4.CreateTranslation(transform.position + boxShape.offset * transform.scale) * Matrix4x4.CreateScale(1, 1, -1);

		Matrix4x4 _rotation = Matrix4x4.CreateFromYawPitchRoll(transform.Rotation.Y / 180 * Mathf.Pi,
		                                                       -transform.Rotation.X / 180 * Mathf.Pi,
		                                                       -transform.Rotation.Z / 180 * Mathf.Pi);
		float outlineThickness = 0.03f * Mathf.ClampMin(MathHelper.Abs((float) MathHelper.Sin((double) Time.editorElapsedTime)), 0.5f);
		Matrix4x4 _scale = Matrix4x4.CreateScale(boxShape.size.X * transform.scale.X + outlineThickness, boxShape.size.Y * transform.scale.Y + outlineThickness, transform.scale.Z * boxShape.size.Z + outlineThickness);
		return (_scale * Matrix4x4.Identity * _pivot * _rotation * _translation) * Matrix4x4.CreateScale(Units.OneWorldUnit) * Camera.I.viewMatrix * Camera.I.projectionMatrix;
	}

	public Vector4 GetSize()
	{
		return new Vector4(boxShape.size.X * transform.scale.X, boxShape.size.Y * transform.scale.Y, 1, 1);
	}

	public override void Update()
	{
		distanceFromCamera = CalculateDistanceFromCamera();

		if (boxShape == null)
		{
			return;
		}
		//if (Time.elapsedTicks % 10 == 0) onScreen = Camera.I.RectangleVisible(boxShape);

		if (onScreen)
		{
			UpdateMVP();
		}

		base.Update();
	}

	private float CalculateDistanceFromCamera()
	{
		return Vector2.Distance(transform.position, Camera.I.transform.position);
	}

	internal void UpdateMVP()
	{
		LatestModelViewProjection = GetModelViewProjectionFromBoxShape();
	}

	public virtual void Render()
	{
	}
}