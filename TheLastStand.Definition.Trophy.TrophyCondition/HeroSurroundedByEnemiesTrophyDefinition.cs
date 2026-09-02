using System.Xml.Linq;
using TPLib;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public class HeroSurroundedByEnemiesTrophyDefinition : ValueIntHeroesTrophyConditionDefinition
{
	public const string Name = "HeroSurroundedByEnemies";

	public override object[] DescriptionLocalizationParameters => new object[2] { NumberOfEnemiesToBeSurroundedBy, base.Value };

	public int NumberOfEnemiesToBeSurroundedBy { get; private set; }

	public HeroSurroundedByEnemiesTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container as XElement;
		if (!int.TryParse(xElement.Attribute("NumberOfTurns").Value, out var result))
		{
			TPDebug.LogError("The Attribute 'NumberOfTurns' of an Element : HeroSurroundedByEnnemies in TrophiesDefinitions should have a value of type int.");
			return;
		}
		base.Value = result;
		if (!int.TryParse(xElement.Value, out var result2))
		{
			TPDebug.LogError("The Value of an Element : HeroSurroundedByEnnemies in TrophiesDefinitions isn't a valid int");
		}
		else
		{
			NumberOfEnemiesToBeSurroundedBy = result2;
		}
	}

	public override string ToString()
	{
		return "HeroSurroundedByEnemies";
	}
}
