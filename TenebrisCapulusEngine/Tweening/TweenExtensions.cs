namespace Tofu3D.Tweening;

public static class Tweener
{
	public static Tween Tween(int startValue, int endValue, float duration, Action<float> OnUpdate)
	{
		Tween tween = new Tween(){duration =duration,endValue = endValue,startValue = startValue, currentTime = 0, OnUpdate = OnUpdate}; 
		return TweenManager.I.StartTween(tween);
	}

	public static void Kill(object target)
	{
		for (int i = 0; i < TweenManager.I.activeTweens.Count; i++)
		{
			if (TweenManager.I.activeTweens[i].target == target)
			{
				TweenManager.I.RemoveTween(i);
				return;
			}
		}
	}
}