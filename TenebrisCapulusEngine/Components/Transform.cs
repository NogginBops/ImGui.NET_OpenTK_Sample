using System.Collections.Generic;
using System.Xml.Serialization;

namespace Scripts;

public class Transform : Component
{
	[XmlIgnore] public List<Transform> children = new();
	public List<int> childrenIDs = new();
	//[Hide] public Vector3 localPosition { get { return position - GetParentPosition(); } set { position = GetParentPosition() + value; } }
	//[Hide] public Vector3 initialAngleDifferenceFromParent = Vector3.Zero;
	//[Hide] public Vector3 up { get { return position + TransformVector(new Vector3(0, 1, 0)); } }

	/*[ShowInEditor]
	public Vector3 LocalPosition
	{
		get { return transform.position - GetParentPosition(); }
		set
		{
			position = value + GetParentPosition();
			localPosition = value;
		}
	}*/
	[XmlIgnore] public Transform parent;
	[Hide] public int parentID = -1;

	public Vector3 pivot = new(0, 0, 0);
	public Vector3 position = Vector3.Zero;
	public Vector3 rotation = Vector3.Zero;

	private Vector3? lastFramePosition = null;

	//public new bool enabled { get { return true; } }

	public Vector3 scale = Vector3.One;

	public override void EditorUpdate()
	{
		Update();
	}

	public override void Update()
	{
		if (lastFramePosition == null)
		{
			lastFramePosition = position;
		}

		Vector3 positionDelta = position - lastFramePosition.Value;

		if (children.Count > 0)
		{
			for (int i = 0; i < children.Count; i++)
			{
				children[i].transform.position += positionDelta;
			}
		}

		lastFramePosition = position;
	}

	public void RemoveChild(int id)
	{
		for (int i = 0; i < children.Count; i++)
		{
			if (children[i].gameObjectID == id)
			{
				children.RemoveAt(i);
				break;
			}
		}

		for (int i = 0; i < childrenIDs.Count; i++)
		{
			if (childrenIDs[i] == id)
			{
				childrenIDs.RemoveAt(i);
				break;
			}
		}
	}

	public void SetParent(Transform par, bool updateTransform = true)
	{
		if (parentID != -1 && Scene.I.GetGameObject(parentID) != null)
		{
			Scene.I.GetGameObject(parentID).transform.RemoveChild(gameObjectID);
		}

		if (updateTransform)
		{
			rotation -= par.transform.rotation;
			position = par.transform.position + (par.transform.position - transform.position);
			//initialAngleDifferenceFromParent = rotation - par.transform.rotation;
		}

		parent = par;
		parentID = parent.gameObjectID;

		par.children.Add(this);
		par.childrenIDs.Add(gameObjectID);
	}

	public Vector3 GetParentPosition()
	{
		if (parent != null)
		{
			return parent.transform.position;
		}

		return Vector3.Zero;
	}

	public Vector3 TransformVector(Vector3 vec)
	{
		//float sin = (float)Math.Sin(transform.Rotation.X);
		//float cos = (float)Math.Cos(transform.Rotation.X);
		//var zRotation = new Vector3(direction.Y * sin - direction.X * cos, direction.X * sin + direction.Y * cos, transform.Rotation);
		//return zRotation;
		Quaternion a = Quaternion.CreateFromRotationMatrix(
		                                                   Matrix.CreateRotationX(90 * (float) Math.PI / 180) * Matrix.CreateRotationY(0) * Matrix.CreateRotationZ(0));
		Matrix b = Matrix.CreateFromQuaternion(a);

		Quaternion q = Quaternion.CreateFromRotationMatrix(
		                                                   Matrix.CreateRotationX(transform.rotation.X) * Matrix.CreateRotationY(transform.rotation.Y) * Matrix.CreateRotationZ(transform.rotation.Z));
		// Matrix rotation = Matrix.CreateFromYawPitchRoll(transform.rotation.Y, transform.Rotation, transform.rotation.X);
		//Vector3 translation = Vector3.Transform(vec, rotation);
		return transform.position + Matrix.CreateFromQuaternion(q).Backward;
	}
}