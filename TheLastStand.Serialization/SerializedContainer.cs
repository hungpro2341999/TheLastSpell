using System;
using TheLastStand.Manager;

namespace TheLastStand.Serialization;

[Serializable]
public abstract class SerializedContainer : ISerializedData
{
	public byte SaveVersion;

	public ApplicationManager.E_BuildType BuildType = ApplicationManager.E_BuildType.Release;

	public int GameMajorVersion;

	public int GameMinorVersion;

	public int GamePatchVersion;

	public int GameHotfixVersion;

	public string SaveDate = "???";

	public void UpdateHeader()
	{
		SaveVersion = GetSaveVersion();
		BuildType = ApplicationManager.BuildType;
		GameMajorVersion = ApplicationManager.MajorVersion;
		GameMinorVersion = ApplicationManager.MinorVersion;
		GamePatchVersion = ApplicationManager.PatchVersion;
		GameHotfixVersion = ApplicationManager.HotfixVersion;
		SaveDate = DateTime.UtcNow.ToLongDateString();
	}

	public abstract byte GetSaveVersion();
}
