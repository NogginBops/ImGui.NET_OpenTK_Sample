using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Dear_ImGui_Sample;

internal struct UniformFieldInfo
{
	public int Location;
	public string Name;
	public int Size;
	public ActiveUniformType Type;
}

internal class ImGuiShader
{
	private readonly (ShaderType Type, string Path)[] Files;
	public readonly string Name;
	private readonly Dictionary<string, int> UniformToLocation = new();
	private bool Initialized;

	public ImGuiShader(string name, string vertexShader, string fragmentShader)
	{
		Name = name;
		Files = new[]
		        {
			        (ShaderType.VertexShader, vertexShader),
			        (ShaderType.FragmentShader, fragmentShader)
		        };
		Program = CreateProgram(name, Files);
	}

	public int Program { get; }

	public void UseShader()
	{
		ShaderCache.shaderInUse = Program;
		GL.UseProgram(Program);
	}

	public void Dispose()
	{
		if (Initialized)
		{
			GL.DeleteProgram(Program);
			Initialized = false;
		}
	}

	public UniformFieldInfo[] GetUniforms()
	{
		GL.GetProgram(Program, GetProgramParameterName.ActiveUniforms, out int UnifromCount);

		UniformFieldInfo[] Uniforms = new UniformFieldInfo[UnifromCount];

		for (int i = 0; i < UnifromCount; i++)
		{
			string Name = GL.GetActiveUniform(Program, i, out int Size, out ActiveUniformType Type);

			UniformFieldInfo FieldInfo;
			FieldInfo.Location = GetUniformLocation(Name);
			FieldInfo.Name = Name;
			FieldInfo.Size = Size;
			FieldInfo.Type = Type;

			Uniforms[i] = FieldInfo;
		}

		return Uniforms;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetUniformLocation(string uniform)
	{
		if (UniformToLocation.TryGetValue(uniform, out int location) == false)
		{
			location = GL.GetUniformLocation(Program, uniform);
			UniformToLocation.Add(uniform, location);

			if (location == -1)
			{
				Debug.Log($"The uniform '{uniform}' does not exist in the shader '{Name}'!");
			}
		}

		return location;
	}

	private int CreateProgram(string name, params (ShaderType Type, string source)[] shaderPaths)
	{
		Util.CreateProgram(name, out int Program);

		int[] Shaders = new int[shaderPaths.Length];
		for (int i = 0; i < shaderPaths.Length; i++) Shaders[i] = CompileShader(name, shaderPaths[i].Type, shaderPaths[i].source);

		foreach (int shader in Shaders) GL.AttachShader(Program, shader);

		GL.LinkProgram(Program);

		GL.GetProgram(Program, GetProgramParameterName.LinkStatus, out int Success);
		if (Success == 0)
		{
			string Info = GL.GetProgramInfoLog(Program);
			Debug.Log($"GL.LinkProgram had info log [{name}]:\n{Info}");
		}

		foreach (int Shader in Shaders)
		{
			GL.DetachShader(Program, Shader);
			GL.DeleteShader(Shader);
		}

		Initialized = true;

		return Program;
	}

	private int CompileShader(string name, ShaderType type, string source)
	{
		Util.CreateShader(type, name, out int Shader);
		GL.ShaderSource(Shader, source);
		GL.CompileShader(Shader);

		GL.GetShader(Shader, ShaderParameter.CompileStatus, out int success);
		if (success == 0)
		{
			string Info = GL.GetShaderInfoLog(Shader);
			Debug.Log($"GL.CompileShader for shader '{Name}' [{type}] had info log:\n{Info}");
		}

		return Shader;
	}
}