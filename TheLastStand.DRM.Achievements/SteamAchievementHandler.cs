using Steamworks;

namespace TheLastStand.DRM.Achievements;

public class SteamAchievementHandler : AAchievementHandler
{
	public override void RefreshAchievements()
	{
		SteamUserStats.StoreStats();
	}

	public override int GetAchievementProgression(Stat stat)
	{
		if (SteamUserStats.GetStat(stat.SteamId, out int pData))
		{
			return pData;
		}
		return -1;
	}

	public override void SetAchievementProgression(Stat stat, int value, bool refreshAchievements = true)
	{
		SteamUserStats.SetStat(stat.SteamId, value);
		if (refreshAchievements)
		{
			RefreshAchievements();
		}
	}

	public override void IncreaseAchievementProgression(Stat stat, int value, bool refreshAchievements = true)
	{
		SteamUserStats.GetStat(stat.SteamId, out int pData);
		SteamUserStats.SetStat(stat.SteamId, pData + value);
		if (refreshAchievements)
		{
			RefreshAchievements();
		}
	}

	public override void UnlockAchievement(Achievement achievement, bool refreshIfAchieved = true)
	{
		SteamUserStats.GetAchievement(achievement.SteamId, out var pbAchieved);
		if (!pbAchieved)
		{
			Analytics.TrySendAchievementUnlockedEvent(achievement.SteamId);
			SteamUserStats.SetAchievement(achievement.SteamId);
			if (refreshIfAchieved)
			{
				RefreshAchievements();
			}
		}
	}

	public override void UnlockAchievement(string achievementId, bool refreshIfAchieved = true)
	{
		SteamUserStats.GetAchievement(achievementId, out var pbAchieved);
		if (!pbAchieved)
		{
			Analytics.TrySendAchievementUnlockedEvent(achievementId);
			SteamUserStats.SetAchievement(achievementId);
			if (refreshIfAchieved)
			{
				RefreshAchievements();
			}
		}
	}

	public override bool IsAchievementUnlocked(Achievement achievement)
	{
		SteamUserStats.GetAchievement(achievement.SteamId, out var pbAchieved);
		return pbAchieved;
	}

	public override void ClearAchievements()
	{
		SteamUserStats.ResetAllStats(bAchievementsToo: true);
	}
}
