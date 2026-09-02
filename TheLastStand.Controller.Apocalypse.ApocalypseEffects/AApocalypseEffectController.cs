using TheLastStand.Definition.Apocalypse.ApocalypseEffects;
using TheLastStand.Model.Apocalypse.ApocalypseEffects;

namespace TheLastStand.Controller.Apocalypse.ApocalypseEffects;

public abstract class AApocalypseEffectController
{
	public AApocalypseEffect AApocalypseEffect { get; }

	public AApocalypseEffectController(ApocalypseEffectDefinition effectDefinition)
	{
		AApocalypseEffect = CreateModel(effectDefinition);
	}

	protected virtual void OnActivation(bool onLoad)
	{
	}

	protected virtual void OnDeactivation(bool onLoad)
	{
	}

	protected abstract AApocalypseEffect CreateModel(ApocalypseEffectDefinition effectDefinition);
}
