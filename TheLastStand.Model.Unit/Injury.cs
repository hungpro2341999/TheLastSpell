using System.Collections.Generic;
using TheLastStand.Definition.Unit;

namespace TheLastStand.Model.Unit;

public class Injury
{
	public Dictionary<UnitStatDefinition.E_Stat, float> StatModifiers { get; set; }

	public Injury(Dictionary<UnitStatDefinition.E_Stat, float> statModifiers, float panicModifier)
	{
		StatModifiers = statModifiers;
	}
}
