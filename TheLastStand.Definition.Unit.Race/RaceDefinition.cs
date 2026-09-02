using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Definition.Unit.Trait;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Unit;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Race;

public class RaceDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Constants
	{
		public const string AnimatorControllerName = "{0}_Template";

		public const string DefaultRaceId = "Human";

		public const string HumanRaceId = "Human";

		public const string DwarfRaceId = "Dwarf";

		public const string ElfRaceId = "Elf";

		public const string DefaultNamesFolderPath = "TextAssets/Races/";

		public const string DefaultNamesFileName = "{0}NameDefinitions-{1}";

		public const string DefaultIconsFolderPath = "View/Sprites/UI/Races/";

		public const string IconName = "Icon_Race_{0}_On";

		public const string IconHoveredName = "Icon_Race_{0}_Hovered";

		public const int DefaultGenderGenerationWeight = 1;
	}

	private Sprite cachedRaceSprite;

	private Sprite cachedRaceHoveredSprite;

	public string AnimatorName => $"{Id}_Template";

	public Dictionary<string, int> GenderGenerationWeights { get; private set; }

	public Vector2 HUDOffset { get; private set; }

	public string Id { get; private set; }

	public string Name => Localizer.Get("RaceTooltip_" + Id);

	public bool OverrideDefaultHUDOffset { get; private set; }

	public bool OverrideUnitAnimator { get; private set; }

	public HashSet<string> PerksIds { get; private set; }

	public Dictionary<string, List<string>> PlayableUnitNames { get; private set; }

	public Sprite RaceSprite
	{
		get
		{
			if ((object)cachedRaceSprite == null)
			{
				cachedRaceSprite = GetIcon();
			}
			return cachedRaceSprite;
		}
	}

	public Sprite RaceHoveredSprite
	{
		get
		{
			if ((object)cachedRaceHoveredSprite == null)
			{
				cachedRaceHoveredSprite = GetIcon(isHoveredState: true);
			}
			return cachedRaceHoveredSprite;
		}
	}

	public Dictionary<UnitStatDefinition.E_Stat, Vector2Int> StatBoundaryModifiers { get; } = new Dictionary<UnitStatDefinition.E_Stat, Vector2Int>();

	public List<UnitTraitDefinition.StatModifier> StatModifiers { get; private set; } = new List<UnitTraitDefinition.StatModifier>();

	public RaceDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		Id = xAttribute.Value;
		PlayableUnitNames = new Dictionary<string, List<string>>();
		foreach (XElement item in xElement.Elements("OverrideNames"))
		{
			DeserializeNames(item);
		}
		if (xElement.Element("OverrideUnitAnimator") != null)
		{
			OverrideUnitAnimator = true;
		}
		PerksIds = new HashSet<string>();
		XElement xElement2 = xElement.Element("Perks");
		if (xElement2 != null)
		{
			foreach (XElement item2 in xElement2.Elements())
			{
				if (item2.IsNullOrEmpty())
				{
					CLoggerManager.Log("A Perk in race " + Id + " is Empty !", LogType.Error);
				}
				else if (!PerksIds.Contains(item2.Value))
				{
					PerksIds.Add(item2.Value);
				}
			}
		}
		XElement xElement3 = xElement.Element("HUDOffset");
		if (xElement3 != null)
		{
			XAttribute xAttribute2 = xElement3.Attribute("X");
			XAttribute xAttribute3 = xElement3.Attribute("Y");
			HUDOffset = new Vector2(int.Parse(xAttribute2?.Value ?? "0"), int.Parse(xAttribute3?.Value ?? "0"));
			OverrideDefaultHUDOffset = true;
		}
		else
		{
			OverrideDefaultHUDOffset = false;
			HUDOffset = Vector2.zero;
		}
		XElement xElement4 = xElement.Element("StatBoundaryModifiers");
		if (xElement4 != null)
		{
			foreach (XElement item3 in xElement4.Elements("StatBoundaryModifier"))
			{
				XAttribute xAttribute4 = item3.Attribute("Stat");
				if (xAttribute4 == null)
				{
					Debug.LogError("The StatBoundaryModifier has no stat in " + Id + " RaceDefinition !");
					continue;
				}
				UnitStatDefinition.E_Stat key = (UnitStatDefinition.E_Stat)Enum.Parse(typeof(UnitStatDefinition.E_Stat), xAttribute4.Value);
				XAttribute xAttribute5 = item3.Attribute("MinModifier");
				int num = ((xAttribute5 != null) ? int.Parse(xAttribute5.Value) : 0);
				XAttribute xAttribute6 = item3.Attribute("MaxModifier");
				int num2 = ((xAttribute6 != null) ? int.Parse(xAttribute6.Value) : 0);
				if (UnitDatabase.UnitStatDefinitions.ContainsKey(key) && UnitDatabase.UnitStatDefinitions[key].ParentStatId != UnitStatDefinition.E_Stat.Undefined && (num != 0 || num2 != 0))
				{
					num = 0;
					num2 = 0;
				}
				if (!StatBoundaryModifiers.ContainsKey(key))
				{
					StatBoundaryModifiers.Add(key, new Vector2Int(num, num2));
				}
				else
				{
					StatBoundaryModifiers[key] += new Vector2Int(num, num2);
				}
			}
			ApplyStatBoundaryToChildStats();
		}
		XElement xElement5 = xElement.Element("StatModifiers");
		if (xElement5 != null)
		{
			foreach (XElement item4 in xElement5.Elements("StatModifier"))
			{
				XAttribute xAttribute7 = item4.Attribute("Stat");
				if (xAttribute7 == null)
				{
					Debug.LogError("The StatModifier has no stat in " + Id + " RaceDefinition !");
					continue;
				}
				XAttribute xAttribute8 = item4.Attribute("DescOverrideKey");
				string descriptionOverrideKey = ((xAttribute8 != null) ? xAttribute8.Value : string.Empty);
				StatModifiers.Add(new UnitTraitDefinition.StatModifier((UnitStatDefinition.E_Stat)Enum.Parse(typeof(UnitStatDefinition.E_Stat), xAttribute7.Value), float.Parse(item4.Value, NumberStyles.Float, CultureInfo.InvariantCulture), descriptionOverrideKey));
			}
		}
		GenderGenerationWeights = new Dictionary<string, int>();
		foreach (XElement item5 in xElement.Elements("GenderGenerationWeight"))
		{
			XAttribute xAttribute9 = item5.Attribute("Gender");
			GenderGenerationWeights.Add(xAttribute9.Value, int.Parse(item5.Value, NumberStyles.Integer, CultureInfo.InvariantCulture));
		}
		if (GenderGenerationWeights.Count > 0)
		{
			if (!GenderGenerationWeights.ContainsKey("Male"))
			{
				GenderGenerationWeights.Add("Male", 1);
			}
			if (!GenderGenerationWeights.ContainsKey("Female"))
			{
				GenderGenerationWeights.Add("Female", 1);
			}
		}
	}

	public string GetDescription(PlayableUnit playableUnit)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		if (StatModifiers != null && StatModifiers.Count > 0)
		{
			int count = StatModifiers.Count;
			for (int i = 0; i < count; i++)
			{
				stringBuilder.Append(StatModifiers[i].GetDescription(getStylizedStatNames: true));
				if (i + 1 < count)
				{
					stringBuilder.AppendLine();
				}
			}
		}
		if (StatBoundaryModifiers.Count > 0)
		{
			stringBuilder.AppendLine();
			int count2 = StatBoundaryModifiers.Count;
			int num = 0;
			foreach (KeyValuePair<UnitStatDefinition.E_Stat, Vector2Int> statBoundaryModifier in StatBoundaryModifiers)
			{
				num++;
				if (UnitDatabase.UnitStatDefinitions.ContainsKey(statBoundaryModifier.Key) && UnitDatabase.UnitStatDefinitions[statBoundaryModifier.Key].ParentStatId != UnitStatDefinition.E_Stat.Undefined)
				{
					continue;
				}
				Vector2 vector = Vector2.zero;
				if (playableUnit != null)
				{
					vector = playableUnit.PlayableUnitStatsController.GetStat(statBoundaryModifier.Key).StartingBoundaries;
				}
				bool flag = false;
				if (statBoundaryModifier.Value.x != 0)
				{
					bool value = statBoundaryModifier.Value.x > 0;
					stringBuilder.Append(Localizer.Format("RaceTooltip_MinStatBoundaryWithModifier", statBoundaryModifier.Key.GetStylizedName(), statBoundaryModifier.Key.GetValueStylized(vector.x + (float)statBoundaryModifier.Value.x, outlined: true, displaySign: false, value)));
					flag = true;
				}
				if (statBoundaryModifier.Value.y != 0)
				{
					if (flag)
					{
						stringBuilder.AppendLine();
					}
					bool value = statBoundaryModifier.Value.y > 0;
					stringBuilder.Append(Localizer.Format("RaceTooltip_MaxStatBoundaryWithModifier", statBoundaryModifier.Key.GetStylizedName(), statBoundaryModifier.Key.GetValueStylized(vector.y + (float)statBoundaryModifier.Value.y, outlined: true, displaySign: false, value)));
				}
				if (num < count2)
				{
					stringBuilder.AppendLine();
				}
			}
		}
		if (PerksIds.Count > 0)
		{
			int count3 = PerksIds.Count;
			int num2 = 0;
			foreach (string perksId in PerksIds)
			{
				if (PlayableUnitDatabase.PerkDefinitions.TryGetValue(perksId, out var value2))
				{
					stringBuilder2.Append(FormatPerkName(value2));
					if (num2 + 1 < count3)
					{
						stringBuilder2.AppendLine();
					}
				}
				num2++;
			}
		}
		return Localizer.Format("RaceTooltipDescription_" + Id, stringBuilder.ToString(), stringBuilder2.ToString());
	}

	private void ApplyStatBoundaryToChildStats()
	{
		List<Tuple<UnitStatDefinition.E_Stat, int>> list = new List<Tuple<UnitStatDefinition.E_Stat, int>>();
		foreach (UnitStatDefinition.E_Stat key in StatBoundaryModifiers.Keys)
		{
			if (UnitDatabase.UnitStatDefinitions.ContainsKey(key))
			{
				UnitStatDefinition.E_Stat childStatId = UnitDatabase.UnitStatDefinitions[key].ChildStatId;
				if (childStatId != UnitStatDefinition.E_Stat.Undefined)
				{
					list.Add(new Tuple<UnitStatDefinition.E_Stat, int>(childStatId, StatBoundaryModifiers[key].y));
				}
			}
		}
		foreach (Tuple<UnitStatDefinition.E_Stat, int> item in list)
		{
			StatBoundaryModifiers.Add(item.Item1, new Vector2Int(0, item.Item2));
		}
	}

	public List<string> GetNamesForGender(string gender)
	{
		if (PlayableUnitNames.TryGetValue(gender, out var value))
		{
			return value;
		}
		return PlayableUnitDatabase.GetNamesForGender(gender);
	}

	private void DeserializeNames(XElement xNames)
	{
		string value = xNames.Attribute("Gender").Value;
		if (string.IsNullOrEmpty(value) || (!string.Equals(value, "Male", StringComparison.Ordinal) && !string.Equals(value, "Female", StringComparison.Ordinal)))
		{
			Debug.LogError("The Gender for Names in RaceDefinition '" + Id + "' is invalid");
		}
		string text = $"{Id}NameDefinitions-{value}";
		XAttribute xAttribute = xNames.Attribute("FileName");
		if (xAttribute != null)
		{
			text = xAttribute.Value;
		}
		List<string> list = null;
		TextAsset textAsset = ResourcePooler.LoadOnce<TextAsset>("TextAssets/Races/" + text);
		if (textAsset != null)
		{
			list = new List<string>(textAsset.text.Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)).ToList();
		}
		if (list != null)
		{
			AddNames(value, list);
		}
	}

	private void AddNames(string gender, List<string> namesList)
	{
		if (PlayableUnitNames.ContainsKey(gender))
		{
			PlayableUnitNames[gender].AddRange(namesList);
		}
		else
		{
			PlayableUnitNames.Add(gender, namesList);
		}
	}

	private string FormatPerkName(PerkDefinition perkDefinition)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("<style=GoodNbOutlined>+</style> ");
		stringBuilder.Append(perkDefinition.ColorizedName);
		return stringBuilder.ToString();
	}

	private Sprite GetIcon(bool isHoveredState = false)
	{
		Sprite sprite = null;
		string iconName = GetIconName(isHoveredState, Id);
		sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Races/" + iconName);
		if (sprite == null)
		{
			iconName = GetIconName(isHoveredState, "Human");
			sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Races/" + iconName);
		}
		return sprite;
	}

	private string GetIconName(bool isHoveredState, string raceId)
	{
		return string.Format(isHoveredState ? "Icon_Race_{0}_Hovered" : "Icon_Race_{0}_On", raceId);
	}
}
