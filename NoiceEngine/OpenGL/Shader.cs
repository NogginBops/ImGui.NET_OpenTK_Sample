using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Xml.Serialization;

namespace Engine;

[Serializable]
public class Shader : IDisposable
{
	public BufferType bufferType;

	public string path;
	private int uLocation_u_color = -1;

	private int uLocation_u_mvp = -1;

	[XmlIgnore] public Dictionary<string, object> uniforms = new Dictionary<string, object>()
	                                                         {
		                                                         {"u_tint", new Vector4(1, 1, 1, 1)}
	                                                         };

	public Shader()
	{
	}

	public Shader(string filePath)
	{
		path = filePath;
	}

	[XmlIgnore] public int ProgramID { get; set; }

	public void Dispose()
	{
	}

	public void Load()
	{
		if (File.Exists(path) == false)
		{
			path = Path.Combine("Assets", path);
		}

		if (path.Contains(".mat")) // IF ITS mat  not .glsl, just assign SpriteRenderer so we can fix it without crashing
		{
			path = Path.Combine("Assets", "Shaders", "SpriteRenderer.glsl");
		}

		GetAllUniforms();
		string shaderFile = File.ReadAllText(path);

		string vertexCode = GetVertexShaderFromFileString(shaderFile);
		string fragmentCode = GetFragmentShaderFromFileString(shaderFile);
		bufferType = GetBufferTypeFromFileString(shaderFile);


		int vs, fs;

		vs = GL.CreateShader(ShaderType.VertexShaderArb);
		GL.ShaderSource(vs, vertexCode);
		GL.CompileShader(vs);

		string error = "";
		GL.GetShaderInfoLog(vs, out error);
		if (error.Length > 0)
		{
			System.Diagnostics.Debug.WriteLine("ERROR COMPILING VERTEX SHADER " + error);
		}

		fs = GL.CreateShader(ShaderType.FragmentShader);
		GL.ShaderSource(fs, fragmentCode);
		GL.CompileShader(fs);

		error = "";
		GL.GetShaderInfoLog(fs, out error);
		if (error.Length > 0)
		{
			System.Diagnostics.Debug.WriteLine("ERROR COMPILING FRAGMENT SHADER " + error);
		}

		ProgramID = GL.CreateProgram();
		GL.AttachShader(ProgramID, vs);
		GL.AttachShader(ProgramID, fs);

		GL.LinkProgram(ProgramID);

		// Delete shaders
		GL.DetachShader(ProgramID, vs);
		GL.DetachShader(ProgramID, fs);
		GL.DeleteShader(vs);
		GL.DeleteShader(fs);
	}

	public void SetMatrix4x4(string uniformName, Matrix4x4 mat)
	{
		if (uLocation_u_mvp == -1)
		{
			int location = GL.GetUniformLocation(ProgramID, uniformName);
			uLocation_u_mvp = location;
		}

		GL.UniformMatrix4(uLocation_u_mvp, 1, false, GetMatrix4x4Values(mat));
		uniforms[uniformName] = mat;
	}

	public void SetFloat(string uniformName, float fl)
	{
		int location = GL.GetUniformLocation(ProgramID, uniformName);
		GL.Uniform1(location, fl);
		uniforms[uniformName] = fl;
	}

	public void SetVector2(string uniformName, Vector2 vec)
	{
		int location = GL.GetUniformLocation(ProgramID, uniformName);
		GL.Uniform2(location, vec.X, vec.Y);
		uniforms[uniformName] = vec;
	}

	public void SetVector3(string uniformName, Vector3 vec)
	{
		int location = GL.GetUniformLocation(ProgramID, uniformName);
		GL.Uniform3(location, vec.X, vec.Y, vec.Z);
		uniforms[uniformName] = vec;
	}

	public void SetVector4(string uniformName, Vector4 vec)
	{
		int location = GL.GetUniformLocation(ProgramID, uniformName);
		GL.Uniform4(location, vec.X, vec.Y, vec.Z, vec.W);
		uniforms[uniformName] = vec;
	}

	public void SetColor(string uniformName, Color col)
	{
		SetColor(uniformName, col.ToVector4());
	}

	public void SetColor(string uniformName, Vector4 vec)
	{
		if (uLocation_u_color == -1)
		{
			int location = GL.GetUniformLocation(ProgramID, uniformName);
			uLocation_u_color = location;
		}

		GL.Uniform4(uLocation_u_color, vec.X, vec.Y, vec.Z, vec.W);
		uniforms[uniformName] = vec;
	}

	// uniform sampler2D textureObject;
	// just find "uniform" and 2 words after; then we know the variables and we can display it 
	// todo
	public ShaderUniform[] GetAllUniforms()
	{
		List<ShaderUniform> uniforms = new List<ShaderUniform>();

		path = path.Replace(@"\", "/");
			string filename = Path.GetFileName(path);
			
			path = Path.Combine("Assets","Shaders", filename);
		
		using (StreamReader sr = new StreamReader(path))
		{
			string shaderString = sr.ReadToEnd();
			int currentIndexInString = 0;
			string trimmedShaderString = shaderString;

			while (trimmedShaderString.Contains("uniform"))
			{
				int startIndex = trimmedShaderString.IndexOf("uniform");
				int endIndex = startIndex + trimmedShaderString.Substring(startIndex).IndexOf(";");

				int endIndexWithEqualsOperator = startIndex + trimmedShaderString.Substring(startIndex).IndexOf("=");

				if (endIndexWithEqualsOperator < endIndex) // if we have "=", trim it so it isnt in the name
				{
					endIndex = endIndexWithEqualsOperator;
				}

				if (startIndex > endIndex)
				{
					break;
				}

				ShaderUniform uniform = new ShaderUniform();

				string[] uniformString = trimmedShaderString.Substring(startIndex, endIndex - startIndex).Split(' ');

				uniform.name = uniformString[2];
				uniform.type = GetUniformType(uniformString[1]);
				currentIndexInString = endIndex + (shaderString.Length - trimmedShaderString.Length);
				trimmedShaderString = shaderString.Substring(currentIndexInString);

				uniforms.Add(uniform);
			}
		}

		return uniforms.ToArray();
	}

	private Type GetUniformType(string typeName)
	{
		if (typeName == "vec4")
		{
			return typeof(Vector4);
		}

		if (typeName == "vec3")
		{
			return typeof(Vector3);
		}

		if (typeName == "mat4")
		{
			return typeof(Matrix4x4);
		}

		if (typeName == "float")
		{
			return typeof(float);
		}

		return typeof(string);
	}

	private float[] GetMatrix4x4Values(Matrix4x4 m)
	{
		return new[]
		       {
			       m.M11, m.M12, m.M13, m.M14,
			       m.M21, m.M22, m.M23, m.M24,
			       m.M31, m.M32, m.M33, m.M34,
			       m.M41, m.M42, m.M43, m.M44
		       };
	}

	public int GetAttribLocation(string attribName)
	{
		return GL.GetAttribLocation(ProgramID, attribName);
	}

	public static BufferType GetBufferTypeFromFileString(string shaderFile)
	{
		string typeString = shaderFile.Substring(shaderFile.IndexOf("[BUFFERTYPE]:") + 13,
		                                         shaderFile.IndexOf("[VERTEX]") - shaderFile.IndexOf("[BUFFERTYPE]") - 13); //File.ReadA;

		BufferType type;
		Enum.TryParse(typeString, out type);

		return type;
	}

	public static string GetVertexShaderFromFileString(string shaderFile)
	{
		return shaderFile.Substring(shaderFile.IndexOf("[VERTEX]") + 8,
		                            shaderFile.IndexOf("[FRAGMENT]") - shaderFile.IndexOf("[VERTEX]") - 8); //File.ReadA;
	}

	public static string GetFragmentShaderFromFileString(string shaderFile)
	{
		return shaderFile.Substring(shaderFile.IndexOf("[FRAGMENT]") + 10); //File.ReadA;
	}
}