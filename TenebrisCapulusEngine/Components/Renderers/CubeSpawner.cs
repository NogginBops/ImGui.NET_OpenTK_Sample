public class CubeSpawner : Component
{
	public override void Start()
	{
		SpawnCubes();
		base.Start();
	}

	private void SpawnCubes()
	{
		for (int x = 0; x < 10; x++)
		{
			for (int y = 0; y < 10; y++)
			{
				for (int z = 0; z < 10; z++)
				{
					GameObject go = GameObject.Create(Camera.I.transform.position + new Vector3(x, y, z) * 500, name: "Cube");
					go.transform.SetParent(transform);
					go.AddComponent<BoxShape>();
					go.GetComponent<BoxShape>().size = new Vector3(150);

					go.AddComponent<ModelRenderer>();
					go.Awake();
				}
			}
		}
	}
}