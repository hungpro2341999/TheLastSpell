namespace TheLastStand.Serialization;

public struct SaveInfo
{
	public SerializedContainer Container;

	public string FilePath;

	public string BackupFilePath;

	public string TemporaryBackupFilePath;

	public bool UseEncryption;
}
