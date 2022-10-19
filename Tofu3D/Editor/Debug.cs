using System.Diagnostics;
using System.Linq;

namespace Tofu3D;

public static class Debug
{
	private static List<string> logs = new();

	public static readonly int LOG_LIMIT = 1000;

	public static Dictionary<string, Stopwatch> timers = new();
	public static Dictionary<string, float> stats = new();

	public static void Log(string message)
	{
		if (Global.EditorAttached == false)
		{
			return;
		}


		logs.Add($"[{DateTime.Now.ToString("HH:mm:ss")}]" + message);

		//Window.I.Title = logs.Last();

		if (logs.Count > LOG_LIMIT + 1)
		{
			logs.RemoveAt(0);
		}
	}
	public static void Log(object message)
	{
		Log(message.ToString());
	}

	public static void StartTimer(string timerName)
	{
		if (Global.EditorAttached == false)
		{
			return;
		}

		if (timers.ContainsKey(timerName))
		{
			timers[timerName].Restart();
		}
		else
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			timers.Add(timerName, sw);
		}
	}

	public static void CountStat(string statName, float value)
	{
		if (Global.EditorAttached == false)
		{
			return;
		}

		if (stats.ContainsKey(statName) == false)
		{
			stats[statName] = 0;
		}

		stats[statName] += value;
	}

	public static void Stat(string statName, float value)
	{
		if (Global.EditorAttached == false)
		{
			return;
		}

		if (stats.ContainsKey(statName) == false)
		{
			stats[statName] = 0;
		}

		stats[statName] = value;
	}

	public static void EndTimer(string timerName)
	{
		if (Global.EditorAttached == false)
		{
			return;
		}

		timers[timerName].Stop();
	}

	public static void ClearTimers()
	{
		timers.Clear();
	}

	public static void ClearStats()
	{
		stats.Clear();
	}

	public static void ClearLogs()
	{
		logs.Clear();
	}

	public static ref List<string> GetLogs()
	{
		return ref logs;
	}
}