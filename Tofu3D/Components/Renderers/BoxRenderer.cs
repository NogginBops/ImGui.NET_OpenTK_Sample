﻿using Tofu3D.Components.Renderers;

namespace Tofu3D;

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
		//material.shader.SetVector4("u_tint", (Vector4) material.shader.uniforms["u_tint"]);
		if (material.shader.uniforms.ContainsKey("time"))
		{
			material.shader.SetFloat("time", (float) material.shader.uniforms["time"]);
		}
		ShaderCache.BindVAO(material.vao);

		//GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

		GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

		//GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
		Debug.CountStat("Draw Calls", 1);
	}
}