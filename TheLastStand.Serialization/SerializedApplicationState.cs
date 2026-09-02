using System;
using System.Collections.Generic;
using TheLastStand.Manager;
using TheLastStand.Serialization.Achievements;
using TheLastStand.Serialization.Apocalypse;
using TheLastStand.Serialization.Item.ItemRestriction;
using TheLastStand.Serialization.Meta;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedApplicationState : SerializedContainer
{
	public SerializedAchievements Achievements;

	public SerializedCities Cities;

	public bool HasSeenIntroduction;

	public uint DamnedSouls;

	public uint DamnedSoulsObtained;

	public uint DaysPlayed;

	public uint RunsCompleted;

	public uint RunsWon;

	public bool TutorialDone;

	public SerializedGlobalApocalypse GlobalApocalypse;

	public SerializedNarrations MetaNarrations;

	public SerializedMetaShops MetaShops;

	public SerializedMetaConditions MetaConditions = new SerializedMetaConditions();

	public SerializedMetaUpgrades MetaUpgrades;

	public bool ApplicationQuitInOraculum;

	public List<SerializedGlyphData> Glyphs;

	public List<string> TutorialsRead;

	public SerializedItemRestrictions ItemRestrictions;

	public override byte GetSaveVersion()
	{
		return SaveManager.AppSaveVersion;
	}
}
