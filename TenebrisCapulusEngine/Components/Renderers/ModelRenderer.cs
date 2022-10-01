using System.IO;
using Engine.Components.Renderers;

public class ModelRenderer : Renderer
{
	public Model model;

	public override void Awake()
	{
		CreateMaterial();
		/*
		if (model == null)
		{
			model = new Model();
		}
		else
		{
			LoadTexture(texture.path);
		}
		*/

		base.Awake();
	}

	public override void CreateMaterial()
	{
		if (material == null)
		{
			material = MaterialCache.GetMaterial("ModelRenderer");
		}

		base.CreateMaterial();
	}

	public override void OnDestroyed()
	{
		//BatchingManager.RemoveAttribs(texture.id, gameObjectID);
		base.OnDestroyed();
	}

	public override void Render()
	{
		if (onScreen == false)
		{
			return;
		}

		if (boxShape == null)
		{
			return;
		}

		GL.Enable(EnableCap.DepthTest);

		ShaderCache.UseShader(material.shader);
		material.shader.SetMatrix4x4("u_mvp", LatestModelViewProjection);

		ShaderCache.BindVAO(material.vao);

		GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);


		//GL.DrawElements(PrimitiveType.Triangles, 6 * 2, DrawElementsType.UnsignedShort, 0);
		GL.DrawElements(PrimitiveType.Triangles, 6 * 2 * 3, DrawElementsType.UnsignedShort, 0);

		Debug.CountStat("Draw Calls", 1);
	}
}

// STENCIL working

/*
public override void Render()
		{
			if (boxShape == null) return;
			if (texture.loaded == false) return;
			// stencil experiment
			GL.Enable(EnableCap.StencilTest);
			GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
			GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
			GL.StencilMask(0xFF);
			shader.Use();
			shader.SetMatrix4x4("u_mvp", GetModelViewProjection());
			shader.SetVector4("u_color", color.ToVector4());

			GL.BindVertexArray(vao);
			GL.Enable(EnableCap.Blend);

			if (additive)
			{
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusConstantColor);
			}
			else
			{
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			}
			texture.Use();
			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
			// stencil after
			GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
			GL.StencilMask(0x00);
			GL.Disable(EnableCap.DepthTest);

			shader.Use();
			shader.SetMatrix4x4("u_mvp", GetModelViewProjectionForOutline(thickness));
			//shader.SetVector4("u_color", new Vector4(MathF.Abs(MathF.Sin(Time.elapsedTime * 0.3f)), MathF.Abs(MathF.Cos(Time.elapsedTime * 0.3f)), 1, 1));
			shader.SetVector4("u_color", Color.Black.ToVector4());

			texture.Use();
			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

			GL.StencilMask(0xFF);
			GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
			GL.Enable(EnableCap.DepthTest);

			GL.BindVertexArray(0);
			GL.Disable(EnableCap.Blend);
		}
*/