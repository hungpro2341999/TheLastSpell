using TPLib;
using TheLastStand.Manager;

namespace TheLastStand.Serialization;

public static class SaveTypeExtension
{
	public static SaveInfo GetSaveInfo(this E_SaveType saveType)
	{
		switch (saveType)
		{
		case E_SaveType.App:
			return new SaveInfo
			{
				Container = (TPSingleton<ApplicationManager>.Instance.Serialize() as SerializedContainer),
				FilePath = SaveManager.GetCurrentAppSaveFilePath(),
				BackupFilePath = SaveManager.GetCurrentAppSaveBackupFilePath(),
				TemporaryBackupFilePath = SaveManager.GetCurrentAppSaveTemporaryBackupFilePath(),
				UseEncryption = !SaveManager.IsSaveEncryptionDisabled
			};
		case E_SaveType.Game:
			return new SaveInfo
			{
				Container = (TPSingleton<GameManager>.Instance.Serialize() as SerializedGameState),
				FilePath = SaveManager.GetCurrentGameSaveFilePath(),
				BackupFilePath = SaveManager.GetCurrentGameSaveBackupFilePath(),
				TemporaryBackupFilePath = SaveManager.GetGameSaveTemporaryBackupFilePath(),
				UseEncryption = !SaveManager.IsSaveEncryptionDisabled
			};
		case E_SaveType.Settings:
			return new SaveInfo
			{
				Container = (TPSingleton<SettingsManager>.Instance.Serialize() as SerializedSettings),
				FilePath = SaveManager.GetSettingsFilePath(),
				BackupFilePath = string.Empty,
				TemporaryBackupFilePath = string.Empty,
				UseEncryption = false
			};
		default:
			TPSingleton<ApplicationManager>.Instance.LogError("Unexpected save type : " + saveType);
			return default(SaveInfo);
		}
	}
}
