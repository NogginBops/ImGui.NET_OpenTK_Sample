namespace Scripts;

public class PartyBoi : Component
{
	private BoxRenderer boxRenderer;
	private float hue;
	private Text text;

	public override void Awake()
	{
		boxRenderer = GetComponent<BoxRenderer>();
		text = GetComponent<Text>();
		hue = Rendom.Range(0, 360);

		base.Awake();
	}

	public override void Update()
	{
		hue += Time.deltaTime * 300;
		/*if (hue > 360)
		{
			hue = 0;
			if (text.Value == "uwu")
			{
				text.Value = "owo";
			}
			else
			{
				text.Value = "uwu";
			}
		}*/
		boxRenderer.color = Extensions.ColorFromHSVToXna(hue, 0.6f, 0.4f);
		transform.rotation.Z += Time.deltaTime * 3;
		base.Update();
	}
}