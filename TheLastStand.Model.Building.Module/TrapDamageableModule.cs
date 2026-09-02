using TheLastStand.Controller.Building.Module;
using TheLastStand.Definition.Building.Module;

namespace TheLastStand.Model.Building.Module;

public class TrapDamageableModule : DamageableModule
{
	public TrapDamageableModuleController TrapDamageableModuleController => base.BuildingModuleController as TrapDamageableModuleController;

	public TrapDamageableModule(Building buildingParent, DamageableModuleDefinition damageableModuleDefinition, TrapDamageableModuleController trapDamageableModuleController)
		: base(buildingParent, damageableModuleDefinition, trapDamageableModuleController)
	{
	}
}
