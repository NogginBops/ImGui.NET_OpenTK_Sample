namespace Engine.Tweening;

public class Tween
{
	public float startValue;
	public float endValue;
	public float duration;
	public float currentTime;
	public Action<float> OnUpdate;
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