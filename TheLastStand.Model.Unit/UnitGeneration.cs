using TheLastStand.Controller.Unit;
using TheLastStand.Definition.Unit;

namespace TheLastStand.Model.Unit;

public class UnitGeneration
{
	public UnitGenerationController UnitGenerationController { get; private set; }

	public UnitGenerationLevelDefinition UnitGenerationDefinition { get; private set; }

	public UnitGeneration(UnitGenerationLevelDefinition definition, UnitGenerationController controller)
	{
		UnitGenerationDefinition = definition;
		UnitGenerationController = controller;
	}
}
