using System.Collections.Generic;
using TheLastStand.Definition.Unit;

namespace TheLastStand.Model.Item;

public interface IAffix
{
	Dictionary<UnitStatDefinition.E_Stat, float> GetFinalStatModifiers();
}
