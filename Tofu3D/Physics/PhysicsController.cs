using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Tofu3D.Physics;

public static class PhysicsController
{
	public static bool Running = true;

	public static World World;
	private static Task PhysicsTask;
	private static Stopwatch sw = new();

	public static void Init()
	{
		World = new World();

		// PhysicsTask = Task.Run(PhysicsLoop);
	}

	// public static void PhysicsLoop()
	// {
	// 	while (true)
	// 	{
	// 		if (Running && Global.GameRunning)
	// 		{
	// 			var a = Stopwatch.StartNew();
	// 			Step();
	//
	// 			a.Stop();
	// 			Wait(Time.fixedDeltaTime - a.Elapsed.Seconds); // if update took 5 ms, and deltaTime is 15 ms, only wait for 10 ms
	// 		}
	// 		else
	// 		{
	// 			Wait(0.3f); // wait if physics is disabled
	// 		}
	// 	}
	// }

	// private static void Step()
	// {
	// 	lock (World)
	// 	{
	// 		World.Step(Time.fixedDeltaTime);
	// 	}
	// }

	private static void Wait(double seconds)
	{
		if (seconds < 0)
		{
			return;
		}

		Thread.Sleep((int) (seconds * 1000f));
		//sw.Restart();
		//
		//while (sw.ElapsedMilliseconds < milliseconds)
		//{
		//
		//}
	}

	public static void StartPhysics()
	{
		Running = true;
	}

	public static void StopPhysics()
	{
		Running = false;
	}
}