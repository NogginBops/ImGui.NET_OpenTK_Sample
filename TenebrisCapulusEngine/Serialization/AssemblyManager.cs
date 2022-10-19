namespace Tofu3D;

using System.IO;
using System.Reflection;

public static class AssemblyManager
{

	static AssemblyManager()
	{
	}

	public static void CompileScriptsAssembly(string path)
	{
		string currentAssemblyPath = Assembly.GetExecutingAssembly().Location;

		using (FileStream fs = new FileStream(currentAssemblyPath, FileMode.Open))
		{
			using (FileStream newAssemblyFileStream = new FileStream(path, FileMode.Create))
			{
				fs.CopyTo(newAssemblyFileStream);
			}
		}
	}

	public static Assembly LoadScriptsAssembly(string path)
	{
		return Assembly.LoadFile(path);
	}
}