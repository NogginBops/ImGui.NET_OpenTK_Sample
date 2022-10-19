using System.Linq;
using System.Reflection;
using Point = System.Drawing.Point;

public static class Extensions
{
	public static List<int> AllIndexesOf(this string str, string value) {
		if (String.IsNullOrEmpty(value))
			throw new ArgumentException("the string to find may not be empty", "value");
		List<int> indexes = new List<int>();
		for (int index = 0;; index += value.Length) {
			index = str.IndexOf(value, index);
			if (index == -1)
				return indexes;
			indexes.Add(index);
		}
	}

	public static float Lerp(float a, float b, float t)
	{
		return Mathf.Lerp(a, b, t);
	}
	public static Color SetA(ref this Color col, float a)
	{
		return new Color(col.R, col.G, col.B, a);
	}
	public static Vector2 Set(ref this Vector2 vec, Vector2 vec2)
	{
		return vec.Set(vec2.X, vec2.Y);
	}

	public static Vector3 Set(ref this Vector3 vec, Vector3 vec2)
	{
		return vec.Set(vec2.X, vec2.Y, vec2.Z);
	}

	public static Vector2 Set(ref this Vector2 vec, float? x = null, float? y = null)
	{
		if (x.HasValue)
		{
			vec.X = x.Value;
		}

		if (y.HasValue)
		{
			vec.Y = y.Value;
		}

		return vec;
	}

	public static Vector3 Set(ref this Vector3 vec, float? x = null, float? y = null, float? z = null)
	{
		if (x.HasValue)
		{
			vec.X = x.Value;
		}

		if (y.HasValue)
		{
			vec.Y = y.Value;
		}

		if (z.HasValue)
		{
			vec.Z = z.Value;
		}


		return vec;
	}

	public static Vector2 MaxY(Vector2 vector1, Vector2 vector2)
	{
		if (vector1.Y > vector2.Y)
		{
			return vector1;
		}

		return vector2;
	}

	public static float MaxVectorMember(this Vector2 vector)
	{
		if (vector.X > vector.Y)
		{
			return vector.X;
		}

		return vector.Y;
	}

	public static float MaxVectorMember(this Vector3 vector)
	{
		if (vector.X >= vector.Y && vector.X >= vector.Z)
		{
			return vector.X;
		}

		if (vector.Y >= vector.X && vector.Y >= vector.Z)
		{
			return vector.Y;
		}

		return vector.Z;
	}

	public static Vector2 MinY(Vector2 vector1, Vector2 vector2)
	{
		if (vector1.Y < vector2.Y)
		{
			return vector1;
		}

		return vector2;
	}

	//  Vector2
	public static Vector3 VectorX(this Vector2 vector)
	{
		return new Vector3(vector.X, 0, 0);
	}

	public static Vector3 VectorY(this Vector2 vector)
	{
		return new Vector3(0, vector.Y, 0);
	}

	//  Vector3
	public static Vector3 VectorX(this Vector3 vector)
	{
		return new Vector3(vector.X, 0, 0);
	}

	public static Vector3 VectorY(this Vector3 vector)
	{
		return new Vector3(0, vector.Y, 0);
	}

	public static Color ToColor(this System.Numerics.Vector4 vector)
	{
		return new Color(vector.X, vector.Y, vector.Z, vector.W);
	}

	public static Color ToColor(this Vector3 vector)
	{
		return new Color(vector.X, vector.Y, vector.Z);
	}

	public static Color ToColor(this Vector4 vector)
	{
		return new Color(vector.X, vector.Y, vector.Z, vector.W);
	}

	public static List<MemberInfo> GetPropertiesOrFields(this Type t, BindingFlags bf = BindingFlags.Public | BindingFlags.Instance)
	{
		return t.GetMembers(bf).Where(mi => mi.MemberType == MemberTypes.Field || mi.MemberType == MemberTypes.Property).ToList();
	}

	public static Vector2 ToVector2(this Vector3 point)
	{
		return new Vector2(point.X, point.Y);
	}

	public static Vector3 ToVector3(this Vector2 point)
	{
		return new Vector3(point.X, point.Y, 0);
	}

	public static Vector3 Normalized(this Vector3 vec)
	{
		Vector3 v = new Vector3(vec.X / vec.Length(), vec.Y / vec.Length(), vec.Z / vec.Length());
		if (vec.Length() == 0)
		{
			v = Vector3.Zero;
		}

		return v;
	}

	public static Vector2 Normalized(this Vector2 vec)
	{
		Vector2 v = new Vector2(vec.X / vec.Length(), vec.Y / vec.Length());
		if (vec.Length() == 0)
		{
			v = Vector2.Zero;
		}

		return v;
	}

	public static Vector3 Abs(this Vector3 vec)
	{
		Vector3 v = new Vector3(Math.Abs(vec.X), Math.Abs(vec.Y), Math.Abs(vec.Z));
		return v;
	}

	public static System.Drawing.Color ToOtherColor(this Color color)
	{
		return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
	}

	public static Color ToOtherColor(this System.Drawing.Color color)
	{
		return new Color(color.R, color.G, color.B, color.A);
	}

	public static Color ColorFromHSVToXna(double hue, double saturation, double value)
	{
		int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
		double f = hue / 60 - Math.Floor(hue / 60);

		value = value * 255;
		int v = Convert.ToInt32(value);
		int p = Convert.ToInt32(value * (1 - saturation));
		int q = Convert.ToInt32(value * (1 - f * saturation));
		int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

		if (hi == 0)
		{
			return new Color(v, t, p, 255);
		}

		if (hi == 1)
		{
			return new Color(q, v, p, 255);
		}

		if (hi == 2)
		{
			return new Color(p, v, t, 255);
		}

		if (hi == 3)
		{
			return new Color(p, q, v, 255);
		}

		if (hi == 4)
		{
			return new Color(t, p, v, 255);
		}

		return new Color(v, p, q, 255);
	}

	public static Color ColorFromHSV(double hue, double saturation, double value)
	{
		int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
		double f = hue / 60 - Math.Floor(hue / 60);

		value = value * 255;
		int v = Convert.ToInt32(value);
		int p = Convert.ToInt32(value * (1 - saturation));
		int q = Convert.ToInt32(value * (1 - f * saturation));
		int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

		if (hi == 0)
		{
			return new Color(v, t, p, 255);
		}

		if (hi == 1)
		{
			return new Color(q, v, p, 255);
		}

		if (hi == 2)
		{
			return new Color(p, v, t, 255);
		}

		if (hi == 3)
		{
			return new Color(p, q, v, 255);
		}

		if (hi == 4)
		{
			return new Color(t, p, v, 255);
		}

		return new Color(v, p, q, 255);
	}

	public static int Clamp(int value, int min, int max)
	{
		if (value < min)
		{
			return min;
		}

		if (value > max)
		{
			return max;
		}

		return value;
	}

	public static Vector2 Clamp(Vector2 value, float minX, float maxX, float minY, float maxY)
	{
		if (value.X < minX)
		{
			value.X = minX;
		}

		if (value.X > maxX)
		{
			value.X = maxX;
		}

		if (value.Y < minY)
		{
			value.Y = minY;
		}

		if (value.Y > maxY)
		{
			value.Y = maxY;
		}

		return value;
	}

	public static float Clamp(float value, float min, float max)
	{
		if (value < min)
		{
			return min;
		}

		if (value > max)
		{
			return max;
		}

		return value;
	}

	public static float ClampMin(float value, float min)
	{
		if (value < min)
		{
			return min;
		}

		return value;
	}

	public static float Clamp01(float value)
	{
		if (value < 0)
		{
			return 0;
		}

		if (value > 1)
		{
			return 1;
		}

		return value;
	}

	public static Vector2 Round(this Vector2 vector)
	{
		return new Vector2((float) Math.Round((decimal) vector.X, 2), (float) Math.Round((decimal) vector.Y, 2));
	}

	public static Vector3 Round(this Vector3 vector)
	{
		return new Vector3((float) Math.Round((decimal) vector.X, 2), (float) Math.Round((decimal) vector.Y, 2), (float) Math.Round((decimal) vector.Z, 2));
	}

	public static Point Round(this Point point, int scale)
	{
		return new Point((int) Math.Floor(point.X / (float) scale) * scale, (int) Math.Floor(point.Y / (float) scale) * scale);
	}

	public static float TranslateToGrid(this float value, int gridSize = 10)
	{
		return (int) ((decimal) value / gridSize) * gridSize - gridSize;
	}

	public static Vector2 TranslateToGrid(this Vector2 vector, float gridSize = 10)
	{
		return new Vector2((int) (vector.X / gridSize) * gridSize - gridSize, (int) (vector.Y / gridSize) * gridSize - gridSize);
	}

	public static Vector3 TranslateToGrid(this Vector3 vector, float gridSize = 10)
	{
		return new Vector3((int) (vector.X / gridSize) * gridSize - gridSize, (int) (vector.Y / gridSize) * gridSize - gridSize, (int) (vector.Z / gridSize) * gridSize - gridSize);
	}

	public static float AngleBetween(Vector2 vector1, Vector2 vector2)
	{
		float returnAngle = (float) Math.Acos(Vector2.Dot(vector1, vector2) / (vector1.Length() * vector2.Length()));
		if (returnAngle == float.NaN)
		{
			returnAngle = 0;
		}

		return returnAngle;
	}
}