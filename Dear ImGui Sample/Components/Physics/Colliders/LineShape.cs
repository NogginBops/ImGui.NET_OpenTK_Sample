namespace Scripts;

public class LineShape : Shape
{
	//[ShowInEditor]
	//[System.ComponentModel.Editor(typeof(Editor.MethodEditor), typeof(System.Drawing.Design.UITypeEditor))]
	//public bool EditPoints { get; set; } = false;
	public float length = 0;

	public float? staticAngle;

	public Vector2 GetLineStart() // put both methods into tuple method?
	{
		if (staticAngle != null)
		{
			return transform.position + new Vector2((float) Math.Cos((float) staticAngle), (float) Math.Sin((float) staticAngle));
		}

		return transform.position + new Vector2((float) Math.Cos(transform.rotation.Z), (float) Math.Sin(transform.rotation.Z));
	}

	public Vector2 GetLineEnd()
	{
		if (staticAngle != null)
		{
			return transform.position + new Vector2(-(float) Math.Cos((float) staticAngle), (float) Math.Sin((float) staticAngle)) * length;
		}

		return transform.position + new Vector2(-(float) Math.Cos(transform.rotation.Z), (float) Math.Sin(transform.rotation.Z)) * length;
	}
}