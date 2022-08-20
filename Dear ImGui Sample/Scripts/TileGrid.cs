/*using System.Collections.Generic;
using System.Xml.Serialization;

namespace Scripts;

public class TileGrid : Component
{
	private Vector2 playerSmoothPosition;

	[XmlIgnore] public List<GameObject> tiles = new();
	[XmlIgnore] public List<float> tilesOGPositionsY = new();
	public Vector2 size { get; set; }

	public override void Start()
	{
		playerSmoothPosition = Player.I.transform.position;
		if (tiles.Count == 0)
		{
			for (var x = 0; x < size.X; x++)
			for (var y = 0; y < size.Y; y++)
			{
				var tile = GameObject.Create(name: "Tile_" + y);
				tile.dynamicallyCreated = true;

				tile.AddComponent<SpriteSheetRenderer>();

				var spriteSheetRenderer = tile.GetComponent<SpriteSheetRenderer>();
				tile.Awake();
				//spriteSheetRenderer.blendState = BlendState.Opaque;
				spriteSheetRenderer.Layer = 5;
				tile.transform.SetParent(transform);
				//tile.transform.localPosition = new Vector3(x * 3.2f, y * 3.2f, 0);
				tile.transform.scale = new Vector3(0.1f, 0.1f, 0.1f);

				spriteSheetRenderer.LoadTexture("2D/tiles.png");
				spriteSheetRenderer.SpritesCount = new Vector2(16, 16);
				if (y == 0)
				{
					if (x == 0)
					{
						spriteSheetRenderer.currentSpriteIndex = 0;
					}
					else if (x == size.X - 1)
					{
						spriteSheetRenderer.currentSpriteIndex = 2;
					}
					else
					{
						spriteSheetRenderer.currentSpriteIndex = 1;
					}
				}
				else
				{
					spriteSheetRenderer.currentSpriteIndex = 17;
				}

				tiles.Add(tile);
				tilesOGPositionsY.Add(tile.transform.position.Y);
			}
		}
	}

	//public override void PreSceneSave()
	//{
	//	while (tiles.Count > 0)
	//	{
	//		tiles[0].Destroy();
	//		tiles.RemoveAt(0);
	//	}
	//	base.OnDestroyed();
	//}
	public override void Update()
	{
		if (KeyboardInput.IsKeyDown(Keys.Space))
		{
			for (var i = Scene.I.gameObjects.Count - 1; i > 0; i--)
				if (Scene.I.gameObjects[i].name.ToLower().Contains("tile") && Scene.I.gameObjects[i].GetComponent<TileGrid>() == null)
				{
					Scene.I.gameObjects[i].Destroy();
				}

			tiles.Clear();
		}
		//UpdateFallingGroundPosition();
		//UpdateLighting();

		base.Update();
	}

	private void UpdateLighting()
	{
		if (Player.I == null)
		{
			return;
		}

		playerSmoothPosition = Vector2.Lerp(playerSmoothPosition, Player.I.transform.position, Time.deltaTime * 7);
		var lights = Scene.I.FindComponentsInScene<LightSource>();
		for (var i = 0; i < tiles.Count; i++)
		{
			//float distanceFromPlayer = Vector2.Distance(playerSmoothPosition.TranslateToGrid(2.4f), tiles[i].transform.position) + MathF.Sin(Time.elapsedTime * 1f) * 1;
			float intensity = 0;

			for (var j = 0; j < lights.Count; j++)
			{
				var distance = Vector2.Distance(lights[j].transform.position.TranslateToGrid(2.4f), tiles[i].transform.position) + MathF.Sin(Time.elapsedTime * 1f) * (1 / lights[j].flickerStrength) * 0.12f;
				distance = (float) Math.Pow(distance, lights[j].falloff);
				intensity += lights[j].parameteridk / (distance * lights[j].intensity);
			}

			tiles[i].GetComponent<Renderer>().color = new Color(Color.White * Mathf.Clamp(intensity, 0, 0.9f), 255);
		}
	}

	private void UpdateFallingGroundPosition()
	{
		for (var i = 0; i < tiles.Count; i++)
		{
			var distanceFromPlayerOnXAxis = MathF.Abs(tiles[i].transform.position.X - Player.I.transform.position.X);
			float goDown;
			if (distanceFromPlayerOnXAxis < 22)
			{
				goDown = 0;
			}
			else
			{
				goDown = distanceFromPlayerOnXAxis - 22;
			}

			tiles[i].transform.position.Y = Mathf.Lerp(tiles[i].transform.position.Y, tilesOGPositionsY[i] + goDown, Time.deltaTime * (11 + MathF.Abs(Player.I.GetComponent<Rigidbody>().Velocity.X) * 2));
		}
	}
}*/

