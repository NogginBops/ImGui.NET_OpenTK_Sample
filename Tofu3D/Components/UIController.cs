using Tofu3D.Tweening;

namespace Tofu3D.Components;

public class UIController : Component
{
	[Show]
	public GameObject playBtn;
	public GameObject bg;

	public override void Start()
	{
		playBtn.GetComponent<Button>().onReleasedAction += PlayClicked;
		base.Start();
	}

	private void PlayClicked()
	{
		Tweener.Tween(1, 0, 0.7f, (f) =>
		{
			Debug.Log($"Tweening alpha progress:{f}");

			playBtn.GetComponent<Renderer>().color = playBtn.GetComponent<Renderer>().color.SetA(f);
			bg.GetComponent<Renderer>().color = bg.GetComponent<Renderer>().color.SetA(f);
		});
		//playBtn.GetComponent<Renderer>();
	}
}