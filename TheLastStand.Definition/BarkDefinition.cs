using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.Unit.Race;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Model;

namespace TheLastStand.Definition;

public class BarkDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Constants
	{
		public class Ids
		{
			public const string ManyEnemyUnitSpawn = "ManyEnemyUnitSpawn";

			public const string BuildingHasCriticalHealth = "BuildingHasCriticalHealth";

			public const string BuildingHit = "BuildingHit";

			public const string BuildingGaugeCompletion = "BuildingGaugeCompletion";

			public const string DayCycleBeginning = "DayCycleBeginning";

			public const string NightCycleBeginning = "NightCycleBeginning";

			public const string PlayableUnitAttackPlayableUnit = "PlayableUnitAttackPlayableUnit";

			public const string PlayableUnitsAlmostOutOfMana = "PlayableUnitsAlmostOutOfMana";

			public const string PlayableUnitCriticAttack = "PlayableUnitCriticAttack";

			public const string PlayableUnitDeath = "PlayableUnitDeath";

			public const string PlayableUnitSelfDeath = "PlayableUnitSelfDeath";

			public const string PlayableUnitHit = "PlayableUnitHit";

			public const string PlayableUnitHitByPlayableUnit = "PlayableUnitHitByPlayableUnit";

			public const string PlayableUnitKillAnEnemy = "PlayableUnitKillAnEnemy";

			public const string PlayableUnitLoseABigAmountofHealth = "PlayableUnitLoseABigAmountofHealth";

			public const string PlayableUnitsHaveKilledALot = "PlayableUnitsHaveKilledALot";

			public const string DawnStart = "DawnStart";

			public const string BarkIDMask = "Bark_{0}_{1}";

			public const string BarkWithRaceIDMask = "Bark_{0}_{1}_{2}";
		}
	}

	public string Id { get; private set; }

	public float Proba { get; private set; }

	public Dictionary<string, int> RacesSentencesCount { get; private set; }

	public int SentencesCount { get; private set; }

	public BarkDefinition(XContainer container)
		: base(container)
	{
	}

	public static string GetSentence(BarkDefinition barkDefinition, IBarker barker, int sentenceIndex = -1)
	{
		int num = barkDefinition.SentencesCount;
		RaceDefinition barkerRaceDefinition = barker.BarkerRaceDefinition;
		if (barkerRaceDefinition != null)
		{
			barkDefinition.RacesSentencesCount.TryGetValue(barkerRaceDefinition.Id, out var value);
			num += value;
		}
		int num2 = ((sentenceIndex == -1) ? RandomManager.GetRandomRange(barkDefinition, 0, num) : sentenceIndex);
		if (num2 >= barkDefinition.SentencesCount && num2 < num)
		{
			num2 -= barkDefinition.SentencesCount;
			return Localizer.Get($"Bark_{barkDefinition.Id}_{num2}_{barkerRaceDefinition.Id}");
		}
		if (num2 < barkDefinition.SentencesCount)
		{
			return Localizer.Get($"Bark_{barkDefinition.Id}_{num2}");
		}
		return null;
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute.IsNullOrEmpty())
		{
			TPDebug.LogError("BarkDefinition must have an Id");
			return;
		}
		Id = xAttribute.Value;
		XElement xElement2 = xElement.Element("Proba");
		if (xElement2.IsNullOrEmpty())
		{
			TPDebug.LogError("Bark " + Id + " hasn't a Proba !");
			return;
		}
		if (!float.TryParse(xElement2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			TPDebug.LogError("Bark " + Id + "'s Proba " + HasAnInvalidFloat(xElement2.Value));
			return;
		}
		Proba = result * 0.01f;
		RacesSentencesCount = new Dictionary<string, int>();
		IEnumerable<XElement> enumerable = xElement.Elements("Sentences");
		if (enumerable.Count() == 0)
		{
			TPDebug.LogError("Bark " + Id + " hasn't a Sentences !");
			return;
		}
		foreach (XElement item in enumerable)
		{
			DeserializeSentences(item);
		}
	}

	private void DeserializeSentences(XElement xSentences)
	{
		XAttribute xAttribute = xSentences.Attribute("RaceId");
		bool flag = xAttribute != null;
		string text = (flag ? xAttribute.Value : null);
		string text2 = string.Empty;
		if (flag)
		{
			text2 = " for Race: " + text;
		}
		XAttribute xAttribute2 = xSentences.Attribute("Count");
		if (!xAttribute2.IsNullOrEmpty())
		{
			if (!int.TryParse(xAttribute2.Value, out var result))
			{
				TPDebug.LogError("Bark " + Id + "'s Sentences Count: " + HasAnInvalidInt(xAttribute2.Value) + text2);
			}
			else if (flag)
			{
				RacesSentencesCount.AddValueOrCreateKey(text, result, (int a, int b) => a + b);
			}
			else
			{
				SentencesCount = result;
			}
		}
		else
		{
			TPDebug.LogError("Bark " + Id + "'s Sentences hasn't a Count" + text2 + " !");
		}
	}
}
