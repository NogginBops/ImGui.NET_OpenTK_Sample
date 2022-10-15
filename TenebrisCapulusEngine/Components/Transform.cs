﻿using System.Collections.Generic;
using System.Numerics;
using System.Xml.Serialization;
using OpenTK.Mathematics;
using Quaternion = System.Numerics.Quaternion;

//using Quaternion = Engine.Quaternion;

namespace Scripts;

public class Transform : Component
{
	[XmlIgnore]
	public List<Transform> children = new();
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
	[XmlIgnore]
	public Transform parent;
	[Hide]
	public int parentID = -1;

	public Vector3 pivot = new(0, 0, 0);
	public Vector3 position = Vector3.Zero;
	private Vector3 rotation = Vector3.Zero;
	public Vector3 Rotation
	{
		get { return rotation; }
		set { rotation = new Vector3(value.X % 360, value.Y % 360, value.Z % 360); }
	}
	public Vector3 forward { get; set; }

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
			Rotation -= par.transform.Rotation;
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

	public Vector3 TransformDirection(Vector3 dir)
	{
		Vector3 forward = (Matrix4x4.CreateTranslation(new Vector3(-dir.X, dir.Y, -dir.Z))
		                 * Matrix4x4.CreateFromYawPitchRoll(transform.Rotation.Y / 180 * Mathf.Pi,
		                                                    -transform.Rotation.X / 180 * Mathf.Pi,
		                                                    -transform.Rotation.Z / 180 * Mathf.Pi)).Translation;
		return forward;
	}

	// public Vector3 TransformVector(Vector3 dir)
	// {
	// 	Vector3 direction = new Vector3(
	// 	                                (float) (MathHelper.Sin(MathHelper.DegreesToRadians(transform.Rotation.Y))
	// 	                                       * MathHelper.Cos(MathHelper.DegreesToRadians(transform.Rotation.X))),
	// 	                                (float) (MathHelper.Sin(MathHelper.DegreesToRadians(transform.Rotation.X))),
	// 	                                (float) (MathHelper.Cos(MathHelper.DegreesToRadians(transform.Rotation.Y))
	// 	                                       * MathHelper.Cos(MathHelper.DegreesToRadians(transform.Rotation.X)))
	// 	                               );
	//
	// 	direction = direction.Normalized();
	//
	// 	Matrix4x4 mat = Matrix4x4.CreateTranslation(direction) * Matrix4x4.CreateLookAt(Vector3.Zero, dir, Vector3.Up);
	//
	// 	direction = mat.Translation;
	// 	direction = direction.Normalized();
	// 	return dir;
	// }
}