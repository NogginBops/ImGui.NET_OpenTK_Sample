using System.Collections.Generic;
using System.Xml.Serialization;

namespace Scripts;

public class PolygonShape : Shape
{
	public int highlightEdgeIndex = 0;
	private float lastRotation;
	[XmlIgnore] public Action onPointsEdit; // = Engine.ColliderEditor.GetInstance().ToggleEditing;
	public Vector2 Position = new(0, 0);

	/// <summary>
	///         LOCAL points
	/// </summary>

	[Show]
	public List<Vector2> Points { get; } = new();
	public List<Vector2> OriginalPoints { get; } = new();

	public List<Vector2> Edges { get; } = new()
	                                      {new Vector2(0, 0)};

	/// <summary>
	///         Returns center in WORLD
	/// </summary>
	public Vector2 Center
	{
		get
		{
			float totalX = 0;
			float totalY = 0;
			for (int i = 0; i < Points.Count; i++)
			{
				totalX += Points[i].X;
				totalY += Points[i].Y;
			}

			return TransformToWorld(new Vector2(totalX / Points.Count, totalY / Points.Count));
		}
	}

	public override void Awake()
	{
		BuildEdges();
		base.Awake();
	}

	public void BuildEdges()
	{
		if (OriginalPoints.Count == 0 || Points.Count != OriginalPoints.Count)
		{
			OriginalPoints.Clear();
			OriginalPoints.AddRange(Points.ToArray());
		}

		Vector2 p1;
		Vector2 p2;
		Edges.Clear();
		for (int i = 0; i < Points.Count; i++)
		{
			p1 = Points[i];
			if (i + 1 >= Points.Count)
			{
				p2 = Points[0];
			}
			else
			{
				p2 = Points[i + 1];
			}

			Edges.Add(p2 - p1);
		}
	}

	public override void Update()
	{
		if (transform.rotation.Z != lastRotation)
		{
			SetRotation(transform.rotation.Z);
			lastRotation = transform.rotation.Z;
		}

		base.Update();
	}

	public void SetRotation(float angle)
	{
		//if (angle > 0.01 || float.IsNaN(angle)) { return; }
		for (int i = 0; i < Points.Count; i++)
		{
			Vector2 originalPoint = OriginalPoints[i];

			Points[i] = new Vector2(originalPoint.X * (float) Math.Cos(-angle) - originalPoint.Y * (float) Math.Sin(-angle), originalPoint.X * (float) Math.Sin(-angle) + originalPoint.Y * (float) Math.Cos(-angle));
		}

		BuildEdges();
	}
}