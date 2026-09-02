using TheLastStand.Definition.Unit;

namespace TheLastStand.View.Unit.Stat;

public class AdditionalUnitStatDisplay : UnitStatDisplay
{
	private UnitStatDefinition additionalStatDefinition;

	public UnitStatDefinition AdditionalStatDefinition
	{
		get
		{
			return additionalStatDefinition;
		}
		set
		{
			if (additionalStatDefinition != value)
			{
				additionalStatDefinition = value;
				fullRefreshNeeded = true;
			}
		}
	}
}
