namespace Engine;

public static class ShaderCache
{
	public static int shaderInUse = -1;

	public static void UseShader(Shader shader)
	{
		UseShader(shader.ProgramID);
	}
	public static void UseShader(int programID)
	{
		if (programID == shaderInUse)
		{
			return;
		}

		shaderInUse = programID;
		GL.UseProgram(programID);
	}
}