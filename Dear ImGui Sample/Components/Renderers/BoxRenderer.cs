using System.IO;
using System.Linq;
using Engine.Components.Renderers;

namespace Engine;

public class BoxRenderer : Renderer
{
	public override void Awake()
	{
		base.Awake();
	}

	public override void CreateMaterial()
	{
		if (material == null)
		{
			material = MaterialCache.GetMaterial("BoxMaterial");
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

		material.shader.SetMatrix4x4("u_mvp", LatestModelViewProjection);
		material.shader.SetColor("u_color", color.ToVector4());
		material.shader.SetVector4("u_tint", (Vector4) material.shader.uniforms["u_tint"]);
		if (material.shader.uniforms.ContainsKey("time"))
		{
			material.shader.SetFloat("time", (float) material.shader.uniforms["time"]);

		}
		BufferCache.BindVAO(material.vao);

		//GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

		GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

		//GL.DrawElements(BeginMode.Triangles, 6, DrawElementsType.UnsignedInt, 0);
		Debug.CountStat("Draw Calls", 1);
	}
}