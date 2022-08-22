using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engine;

public static class PersistentData
{
	private static bool inited = false;
	private static Dictionary<string, object> data = new();

	private static void LoadAllData()
	{
		if (File.Exists("persistentData") == false)
		{
			return;
		}

		data = new Dictionary<string, object>();
		using (StreamReader sr = new StreamReader("persistentData"))
		{
			while (sr.Peek() != -1)
			{
				string line = sr.ReadLine();
				if (line?.Length > 0)
				{
					string key = line.Substring(0, line.IndexOf(":"));
					string value = line.Substring(line.IndexOf(":") + 1);
					data.Add(key, value);
				}
			}
		}
	}

	private static void Save()
	{
		if (File.Exists("persistentData") == false)
		{
			File.Delete("persitentData");
		}

		FileStream fs = File.Create("persistentData");
		fs.Close();
		using (StreamWriter sw = new StreamWriter("persistentData"))
		{
			for (int i = 0; i < data.Count; i++) sw.WriteLine(data.Keys.ElementAt(i) + ":" + data.Values.ElementAt(i));
		}
	}

	public static void DeleteAll()
	{
		data = new Dictionary<string, object>();
		Save();
	}

	private static object Get(string key, object? defaultValue = null)
	{
		if (data.Count == 0)
		{
			LoadAllData();
		}

		if (data.ContainsKey(key) == false && defaultValue != null)
		{
			return defaultValue;
		}

		if (data.ContainsKey(key) == false)
		{
			return null;
		}

		return data[key];
	}

	public static string GetString(string key, string? defaultValue = null)
	{
		return Get(key, defaultValue)?.ToString();
	}

	public static int GetInt(string key, int? defaultValue = null)
	{
		return int.Parse(Get(key, defaultValue)?.ToString());
	}

	public static bool GetBool(string key, bool? defaultValue = null)
	{
		return bool.Parse(Get(key, defaultValue)?.ToString());
	}

	public static void Set(string key, object value)
	{
		if (data.Count == 0)
		{
			LoadAllData();
		}

		data[key] = value;

		Save();
	}
}