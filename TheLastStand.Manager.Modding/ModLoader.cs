namespace TheLastStand.Manager.Modding;

public abstract class ModLoader<T> where T : class, new()
{
	private static T instance;

	protected bool initialized;

	protected bool modsLoaded;

	public static T Instance => Get();

	public virtual bool ModsLoaded => modsLoaded;

	private static T Get()
	{
		if (instance == null)
		{
			instance = new T();
		}
		return instance;
	}

	public abstract void Init();

	public abstract void LoadMods();
}
