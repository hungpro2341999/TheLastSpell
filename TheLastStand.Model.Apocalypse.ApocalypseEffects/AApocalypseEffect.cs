using TheLastStand.Controller.Apocalypse.ApocalypseEffects;
using TheLastStand.Definition.Apocalypse.ApocalypseEffects;

namespace TheLastStand.Model.Apocalypse.ApocalypseEffects;

public abstract class AApocalypseEffect
{
	public ApocalypseEffectDefinition AApocalypseEffectDefinition { get; }

	public AApocalypseEffectController AApocalypseEffectController { get; }

	public AApocalypseEffect(ApocalypseEffectDefinition effectDefinition, AApocalypseEffectController effectController)
	{
		AApocalypseEffectDefinition = effectDefinition;
		AApocalypseEffectController = effectController;
	}
}
