using TheLastStand.Definition.Unit;
using TheLastStand.Model.Unit;

namespace TheLastStand.Controller.Unit;

public class UnitGenerationController
{
	public UnitGeneration UnitGeneration { get; private set; }

	public UnitGenerationController(UnitGenerationLevelDefinition definition)
	{
		UnitGeneration = new UnitGeneration(definition, this);
	}
}
