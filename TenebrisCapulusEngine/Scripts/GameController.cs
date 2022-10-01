/*using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace Engine;

public class GameController : Component
{
	[Show] public GameObject groundPrefab;
	[Show] public GameObject stick;
	[Show] public GameObject player;
	[XmlIgnore] public Action SpawnGroundAction;

	private float groundY = -80;
	private float groundWidth = 150;
	private float groundWidthHalf = 75;

	private int spawnedGroundsCount = 0;

	private bool spaceKeyDown = false;
	// GAME VARIABLES
	private float stickLength = 1;
	private bool stickFalling = false;
	private float neededLength = 0;

	public override void Awake()
	{
		SpawnGroundAction += () => SpawnGround();

		base.Awake();
	}

	public override void Start()
	{
		Camera.I.transform.position = Vector2.Zero;

		StartGame();
		base.Start();
	}

	public override void Update()
	{
		if (KeyboardInput.IsKeyDown(Keys.Space))
		{
			stickLength += Time.deltaTime * 140;
			UpdateStickLength();
		}

		if (KeyboardInput.IsKeyUp(Keys.Space) && spaceKeyDown) // lifted space up
		{
			stickFalling = true;
		}

		if (stickFalling)
		{
			stick.transform.rotation.Set(z: stick.transform.rotation.Z + Time.deltaTime * 110);

			if (stick.transform.rotation.Z > 90)
			{
				stick.transform.rotation.Set(z: 90);
				stickFalling = false;
				StickFallen();
			}
		}


		spaceKeyDown = KeyboardInput.IsKeyDown(Keys.Space);
	}

	private void StickFallen()
	{
		if (stickLength + 5 >= neededLength && stickLength + 5 < neededLength + groundWidth)
		{
			Debug.Log("good");
			MoveToNextGround();
			return;
		}
		else
		{
			Debug.Log("not good");
		}
	}

	private void MoveToNextGround()
	{
	}

	private void UpdateStickLength()
	{
		stick.GetComponent<BoxShape>().size.Set(y: stickLength);
	}

	void StartGame()
	{
		GameObject go = SpawnGround();
		go.transform.position = new Vector2(-200, go.transform.position.Y);

		
		//stick.transform.position = new Vector2(GetEndOfGroundPosYForStick(go.transform), 0);
		stick.transform.position.Set(GetEndOfGroundPosYForStick(go.transform), groundY);

		player.transform.position.Set(go.transform.position);
		SpawnGround();
	}

	private float GetEndOfGroundPosYForStick(Transform groundTransform)
	{
		return groundTransform.position.X + groundWidthHalf; // - 5;
	}

	private GameObject SpawnGround()
	{
		//GameObject.Create();

		GameObject go = Serializer.I.LoadPrefab(groundPrefab.prefabPath);

		int space = 200;
		go.transform.position = new Vector2(spawnedGroundsCount * space, groundY);
		neededLength = space + 50;
		EditorWindow_Hierarchy.I.SelectGameObject(go.id);

		spawnedGroundsCount++;

		return go;
	}
}*/