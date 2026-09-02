using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public abstract class APerkEffectController
{
	public APerkEffect PerkEffect { get; private set; }

	public APerkEffectController(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		PerkEffect = CreateModel(aPerkEffectDefinition, aPerkModule);
	}

	protected abstract APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule);

	public virtual void Trigger(PerkDataContainer data)
	{
	}

	public virtual void OnUnlock(bool onLoad)
	{
	}

	public virtual void Lock(bool onLoad)
	{
	}
}
