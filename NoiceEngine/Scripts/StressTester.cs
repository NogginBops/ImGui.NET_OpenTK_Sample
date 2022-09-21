using System.Threading;

public class StressTester : Component
{
	public override void Update()
	{
		Thread.Sleep(Rendom.Range(10,30));
		base.Update();
	}
}