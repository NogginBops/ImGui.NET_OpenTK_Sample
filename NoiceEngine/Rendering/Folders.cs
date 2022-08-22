using System.IO;

namespace Engine;

public class Folders
{
	public static string Assets
	{
		get { return "Assets"; }
	}
	public static string Shaders
	{
		get { return Path.Combine(Assets, "Shaders"); }
	}
	public static string Materials
	{
		get { return Path.Combine(Assets, "Materials"); }
	}
}