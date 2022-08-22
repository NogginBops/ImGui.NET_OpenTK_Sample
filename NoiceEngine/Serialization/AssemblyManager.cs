namespace Engine;

using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using Scripts;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Security.Permissions;

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