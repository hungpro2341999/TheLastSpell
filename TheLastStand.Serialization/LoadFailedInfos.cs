using TheLastStand.Manager;

namespace TheLastStand.Serialization;

public class LoadFailedInfos
{
	public string FileCopyPath;

	public SaveManager.E_BrokenSaveReason BrokenSaveReason;

	public bool BackupHasBeenLoaded;

	public LoadFailedInfos(string fileCopyPath, SaveManager.E_BrokenSaveReason brokenSaveReason = SaveManager.E_BrokenSaveReason.UNKNOWN, bool backupHasBeenLoaded = false)
	{
		FileCopyPath = fileCopyPath;
		BrokenSaveReason = brokenSaveReason;
		BackupHasBeenLoaded = backupHasBeenLoaded;
	}
}
