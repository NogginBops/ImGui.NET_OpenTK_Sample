using System.IO;

namespace Scripts;

public class ParticleSystemRenderer : SpriteRenderer
{
	public new bool allowMultiple = false;

	private int particlesInBatcher;
	public ParticleSystem particleSystem;

	public override void Awake()
	{
		material.additive = true;
		BatchingManager.AddObjectToBatcher(texture.id, this);

		base.Awake();
	}
	public override void CreateMaterial()
	{
		material = new Material();
		Shader shader = new(Path.Combine(Folders.Shaders, "BoxRenderer.glsl"));
		material.SetShader(shader);
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

		if (texture.loaded == false)
		{
			return;
		}

		while (particlesInBatcher < particleSystem.particles.Count)
		{
			BatchingManager.AddObjectToBatcher(texture.id, this, particlesInBatcher);
			particlesInBatcher++;
		}

		for (int i = 0; i < particleSystem.particles.Count; i++)
			BatchingManager.UpdateAttribs(texture.id, gameObjectID, particleSystem.particles[i].worldPosition, new Vector2(particleSystem.particles[i].radius),
			                              particleSystem.particles[i].color, i);

		Debug.Stat("Particles", particleSystem.particles.Count);
	}
}