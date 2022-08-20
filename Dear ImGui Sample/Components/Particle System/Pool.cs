using System.Collections.Concurrent;

namespace Engine;

public class Pool<T>
{
	private ConcurrentBag<T> collection = new();
	private Func<T> objectGenerator;

	public Pool(Func<T> generator)
	{
		if (generator == null)
		{
			throw new ArgumentNullException("objectGenerator");
		}

		collection = new ConcurrentBag<T>();
		objectGenerator = generator;
	}

	public int Count
	{
		get { return collection.Count; }
	}

	public void PutObject(T item)
	{
		collection.Add(item);
	}

	public T GetObject()
	{
		T item;
		if (collection.TryTake(out item))
		{
			return item;
		}

		return objectGenerator();
	}
}