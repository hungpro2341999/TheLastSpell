using System;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;

namespace TheLastStand.Definition.Unit;

public class SkillProgression
{
	private SkillDefinition skillDefinitionCache;

	public string ApocalypseFlagUnlock { get; }

	public string Id { get; }

	public SkillDefinition SkillDefinition
	{
		get
		{
			if (skillDefinitionCache == null)
			{
				skillDefinitionCache = SkillDatabase.SkillDefinitions.GetValueOrDefault(Id);
			}
			return skillDefinitionCache;
		}
	}

	public int UnlockedAtDay { get; }

	public int LockedAtDay { get; }

	public GlyphManager.E_SkillProgressionFlag GlyphFlagUnlock { get; }

	private SkillProgression(string id, int unlockedAtDay, int lockedAtDay, GlyphManager.E_SkillProgressionFlag glyphFlagUnlock, string apocalypseFlagUnlock)
	{
		Id = id;
		UnlockedAtDay = unlockedAtDay;
		LockedAtDay = lockedAtDay;
		GlyphFlagUnlock = glyphFlagUnlock;
		ApocalypseFlagUnlock = apocalypseFlagUnlock;
	}

	public static SkillProgression Deserialize(XElement container)
	{
		XAttribute xAttribute = container.Attribute("Id");
		string text = container.Attribute("UnlockedAtDay")?.Value;
		string text2 = container.Attribute("LockedAtDay")?.Value;
		string value = container.Attribute("GlyphFlagUnlock")?.Value;
		string apocalypseFlagUnlock = container.Attribute("ApocalypseFlagUnlock")?.Value;
		int unlockedAtDay = ((!string.IsNullOrEmpty(text)) ? int.Parse(text) : (-1));
		int lockedAtDay = ((!string.IsNullOrEmpty(text2)) ? int.Parse(text2) : (-1));
		if (string.IsNullOrEmpty(value) || !Enum.TryParse<GlyphManager.E_SkillProgressionFlag>(value, out var result))
		{
			result = GlyphManager.E_SkillProgressionFlag.None;
		}
		return new SkillProgression(xAttribute.Value, unlockedAtDay, lockedAtDay, result, apocalypseFlagUnlock);
	}

	public bool AreConditionsValid(int customDayNumber, int minimumUnlockedAtDay)
	{
		bool num = (UnlockedAtDay == -1 || customDayNumber >= UnlockedAtDay) && (LockedAtDay == -1 || customDayNumber < LockedAtDay) && minimumUnlockedAtDay <= UnlockedAtDay;
		bool flag = GlyphFlagUnlock == GlyphManager.E_SkillProgressionFlag.None || (TPSingleton<GlyphManager>.Instance.SkillProgressionFlag & GlyphFlagUnlock) != 0;
		bool flag2 = string.IsNullOrEmpty(ApocalypseFlagUnlock) || ApocalypseManager.CurrentApocalypse.SkillProgressionFlags.Contains(ApocalypseFlagUnlock);
		return num && flag && flag2;
	}
}
