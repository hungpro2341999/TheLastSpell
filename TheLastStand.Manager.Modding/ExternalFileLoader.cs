using System.IO;
using TheLastStand.Model.Modding;

namespace TheLastStand.Manager.Modding;

public abstract class ExternalFileLoader<T> where T : class, new()
{
	private static T instance;

	protected virtual string FolderPath { get; }

	public static T Instance => Get();

	public bool AllFilesLoaded { get; set; }

	public abstract DirectoryInfo GetDirectory(Mod mod);

	public abstract void OnGetFiles(FileInfo[] fileInfos);

	private static T Get()
	{
		if (instance == null)
		{
			instance = new T();
		}
		return instance;
	}
}
