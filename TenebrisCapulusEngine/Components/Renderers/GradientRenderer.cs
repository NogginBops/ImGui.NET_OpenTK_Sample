using System.IO;
using System.Linq;
using Engine.Components.Renderers;

namespace Engine;

public class GradientRenderer : Renderer
{
	[Show] public Color gradientColorA;
	[Show] public Color gradientColorB;

	public override void Awake()
	{
		base.Awake();

		CreateMaterial();
	}

	public override void CreateMaterial()
	{
		if (material == null)
		{
			material = MaterialCache.GetMaterial("GradientMaterial");
		}

		base.CreateMaterial();
	}

	public override void Render()
	{
		if (boxShape == null || material == null)
		{
			return;
		}

		ShaderCache.UseShader(material.shader);


		material.shader.SetVector4("u_color_a", gradientColorA.ToVector4());
		material.shader.SetVector4("u_color_b", gradientColorB.ToVector4());

		material.shader.SetVector2("u_resolution", new Vector2(100, 100));
		material.shader.SetMatrix4x4("u_mvp", LatestModelViewProjection);
		material.shader.SetColor("u_color", color.ToVector4());
		material.shader.SetVector4("u_tint", (Vector4) material.shader.uniforms["u_tint"]);
		if (material.shader.uniforms.ContainsKey("time"))
		{
			material.shader.SetFloat("time", (float) material.shader.uniforms["time"]);
		}

		ShaderCache.BindVAO(material.vao);

		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

		GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

		Debug.CountStat("Draw Calls", 1);
	}
}