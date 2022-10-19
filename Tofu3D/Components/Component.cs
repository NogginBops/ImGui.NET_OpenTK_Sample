using System.ComponentModel;

namespace Scripts;

public class Component : IDestroyable
{
	public bool allowMultiple = true;

	[XmlIgnore]
	[DefaultValue(false)]
	public bool awoken;
	public bool enabled = true;
	[XmlIgnore]
	public GameObject gameObject;

	public int gameObjectID;
	public bool started;

	[XmlIgnore]
	public Transform transform
	{
		get { return gameObject.transform; }
		set { gameObject.transform = value; }
	}

	public virtual void OnDestroyed()
	{
	}

	public T GetComponent<T>(int? index = null) where T : Component
	{
		return gameObject.GetComponent<T>(index);
	}

	public bool HasComponent<T>() where T : Component
	{
		return gameObject.HasComponent<T>();
	}

	public List<T> GetComponents<T>() where T : Component
	{
		return gameObject.GetComponents<T>();
	}

	public Vector3 TransformToWorld(Vector3 localPoint)
	{
		return localPoint + transform.position;
	}

	public virtual void Awake()
	{
		awoken = true;
	}

	public virtual void Start()
	{
		started = true;
	}

	public virtual void EditorUpdate()
	{
	}

	public virtual void Update()
	{
	}

	public virtual void FixedUpdate()
	{
	}

	public virtual void PreSceneSave()
	{
	}

	public virtual void OnCollisionEnter(Rigidbody rigidbody)
	{
	}

	public virtual void OnCollisionExit(Rigidbody rigidbody)
	{
	}

	public virtual void OnTriggerEnter(Rigidbody rigidbody)
	{
	}

	public virtual void OnTriggerExit(Rigidbody rigidbody)
	{
	}

	public virtual void OnNewComponentAdded(Component comp)
	{
	}

	public int CompareTo(bool other)
	{
		if (this == null)
		{
			return 0;
		}

		return 1;
	}

	public static implicit operator bool(Component instance)
	{
		if (instance == null)
		{
			return false;
		}

		return true;
	}
}