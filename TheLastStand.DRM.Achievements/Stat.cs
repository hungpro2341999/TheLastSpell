namespace TheLastStand.DRM.Achievements;

public struct Stat
{
	public readonly string SteamId;

	public readonly int SonyId;

	public Stat(string steamId, int sonyId)
	{
		SteamId = steamId;
		SonyId = sonyId;
	}
}
