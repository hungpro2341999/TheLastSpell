using System.Xml.Linq;
using TPLib;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public class EnemiesDebuffedSeveralTimesSingleTurnTrophyDefinition : HeroesTrophyConditionDefinition
{
	public const string Name = "EnemiesDebuffedSeveralTimesSingleTurn";

	public override object[] DescriptionLocalizationParameters => new object[2] { Value, NumberOfDebuffs };

	public int Value { get; private set; }

	public int NumberOfDebuffs { get; private set; }

	public EnemiesDebuffedSeveralTimesSingleTurnTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container as XElement;
		if (!int.TryParse(xElement.Value, out var result))
		{
			TPDebug.LogError("The Value of an Element : EnemiesDebuffedSeveralTimesSingleTurn in TrophiesDefinitions isn't a valid int");
			return;
		}
		if (!int.TryParse(xElement.Attribute("NumberOfDebuffs").Value, out var result2))
		{
			TPDebug.LogError("The Value of the Attribute NumberOfDebuffs for EnemiesDebuffedSeveralTimesSingleTurn in TrophiesDefinitions isn't a valid int");
			return;
		}
		NumberOfDebuffs = result2;
		Value = result;
	}

	public override string ToString()
	{
		return "EnemiesDebuffedSeveralTimesSingleTurn";
	}
}
