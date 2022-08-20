namespace Engine;

public static class Rendom
{
	public static Random rnd = new();

	public static int Range(int min, int max)
	{
		return rnd.Next(max - min) + min;
	}

	public static float Range(float max)
	{
		return (float) rnd.NextDouble() * max;
	}

	public static float Range(float min, float max)
	{
		return (float) rnd.NextDouble() * (max - min) + min;
	}

	public static Color ColorRange(Color color1, Color color2)
	{
		float howMuch = Range(1);
		float R = Mathf.Lerp(color1.R, color2.R, howMuch);
		return new Color(Mathf.Lerp(color1.R, color2.R, howMuch) / 255,
		                 Mathf.Lerp(color1.G, color2.G, howMuch) / 255,
		                 Mathf.Lerp(color1.B, color2.B, howMuch) / 255,
		                 Mathf.Lerp(color1.A, color2.A, howMuch) / 255);
	}

	public static Color RandomColor()
	{
		return new Color(Range(1), Range(1), Range(1), 1);
	}
}