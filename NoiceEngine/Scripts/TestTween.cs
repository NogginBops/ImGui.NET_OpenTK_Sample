using System.Threading.Tasks;
using Engine.Tweening;

namespace Engine;

public class TestTween : Component
{
	[XmlIgnore]
	public Action StartTween;
	[XmlIgnore]
	public Action StartCoroutine;

	public override void Awake()
	{
		StartTween = () =>
		{
			Tweener.Kill(transform);
			Tweener.Tween(-100, 100, 2, (i) => { transform.position.Set(x: i); }).SetLoop(Tween.LoopType.Yoyo).SetTarget(transform);
		};

		StartCoroutine = () => { TTTTT(); };

		base.Awake();
	}

	async void TTTTT()
	{
		while (true)
		{
			transform.position = new Vector2(Rendom.Range(-300, 300), Rendom.Range(-300, 300));
			await Task.Delay(100);
		}

		Debug.Log(2);
	}
}