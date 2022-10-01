using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Genbox.VelcroPhysics.Dynamics;

namespace Engine;

public static class Physics
{
	public static World World;
	public static readonly float WORLD_SCALE = 100;
	// WORLD SCALE

	public static readonly Vector2 gravity = new(0,-90);

	public static bool Running = true;

	private static Task PhysicsTask;
	private static Stopwatch sw = new();

	public static void Init()
	{
		World = new World(gravity);
// World.vel
		//World.ContactManager.VelocityConstraintsMultithreadThreshold = 256;
		//World.ContactManager.PositionConstraintsMultithreadThreshold = 256;
		//World.ContactManager.CollideMultithreadThreshold = 256;
		//Thread physicsThread = new Thread(
		//new ThreadStart(PhysicsLoop));
		//
		//physicsThread.Name = "TenebrisCapulusEngine Physics";
		//physicsThread.IsBackground = true;
		//physicsThread.Start();
		PhysicsTask = Task.Run(PhysicsLoop);
	}

	public static void PhysicsLoop()
	{
		while (true)
		{
			if (Running && Global.GameRunning)
			{
				var a = Stopwatch.StartNew();
				Step();

				a.Stop();
				Wait(Time.fixedDeltaTime - a.Elapsed.Seconds); // if update took 5 ms, and deltaTime is 15 ms, only wait for 10 ms
			}
			else
			{
				Wait(0.3f); // wait if physics is disabled
			}
		}
	}

	private static void Step()
	{
		lock (World)
		{
			World.Step(Time.fixedDeltaTime);
		}
	}

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