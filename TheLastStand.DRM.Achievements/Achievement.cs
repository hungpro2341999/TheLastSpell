using System.Collections.Generic;
using System.Linq;
using TPLib.Debugging.Console;

namespace TheLastStand.DRM.Achievements;

public struct Achievement
{
	public class StringToAchievementIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => AchievementContainer.AllAchievements.Select((Achievement a) => a.SteamId).ToList();
	}

	public readonly string SteamId;

	public readonly int SonyId;

	public Achievement(string steamId, int sonyId)
	{
		SteamId = steamId;
		SonyId = sonyId;
	}
}
