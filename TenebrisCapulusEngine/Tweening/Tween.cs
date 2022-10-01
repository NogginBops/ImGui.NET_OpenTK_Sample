namespace Engine.Tweening;

public class Tween
{
	public float startValue;
	public float endValue;
	public float duration;
	public float currentTime;
	public float delay;
	public Action<float> OnUpdate;
	public Action OnComplete;
	public object target;

	public enum LoopType
	{
		NoLoop,
		Yoyo,
		Restart
	};

	private LoopType loopType;

	public float GetValue()
	{
		return Mathf.Lerp(startValue, endValue, currentTime / duration);
	}

	public Tween SetLoop(LoopType lt)
	{
		loopType = lt;
		return this;
	}

	public Tween SetDelay(float dl)
	{
		delay = dl;
		return this;
	}

	public Tween SetOnComplete(Action onComplete)
	{
		OnComplete = onComplete;
		return this;
	}

	public LoopType GetLoop()
	{
		return loopType;
	}

	public Tween SetTarget(object obj)
	{
		target = obj;
		return this;
	}
}