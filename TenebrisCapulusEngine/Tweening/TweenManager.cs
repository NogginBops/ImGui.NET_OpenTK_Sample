using System.Collections.Generic;

namespace Engine.Tweening;

public class TweenManager
{
	public static TweenManager I { get; private set; }
	public List<Tween> activeTweens = new List<Tween>();

	public TweenManager()
	{
		I = this;
	}

	public Tween StartTween(Tween tween)
	{
		activeTweens.Add(tween);
		return activeTweens[activeTweens.Count - 1];
	}

	public void Update()
	{
		for (int i = activeTweens.Count - 1; i >= 0; i--)
		{
			if (activeTweens[i].currentTime == 0 && activeTweens[i].delay > 0)
			{
				activeTweens[i].currentTime = -activeTweens[i].delay;
				activeTweens[i].delay = -activeTweens[i].delay;
			}

			activeTweens[i].currentTime += Time.deltaTime / activeTweens[i].duration;
			bool isCompleted = activeTweens[i].currentTime > activeTweens[i].duration;

			//activeTweens[i].currentTime = Mathf.Clamp(activeTweens[i].currentTime, -Math.Abs(activeTweens[i].delay), activeTweens[i].duration);
			if (activeTweens[i].currentTime >= 0)
			{
				activeTweens[i].OnUpdate.Invoke(activeTweens[i].GetValue());
			}

			if (isCompleted)
			{
				if (activeTweens[i].GetLoop() == Tween.LoopType.Restart)
				{
					activeTweens[i].currentTime = 0;
				}

				if (activeTweens[i].GetLoop() == Tween.LoopType.Yoyo)
				{
					activeTweens[i].OnComplete.Invoke();
					activeTweens[i].currentTime = 0;

					float startValue = activeTweens[i].startValue;
					activeTweens[i].startValue = activeTweens[i].endValue;
					activeTweens[i].endValue = startValue;
				}

				if (activeTweens[i].GetLoop() == Tween.LoopType.NoLoop)
				{
					activeTweens[i].OnComplete?.Invoke();
					RemoveTween(i);
				}

				continue;
			}
		}
	}

	public void RemoveTween(int index)
	{
		activeTweens.RemoveAt(index);
	}
}