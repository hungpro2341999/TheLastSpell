using TheLastStand.Controller.Apocalypse.ApocalypseEffects;
using TheLastStand.Definition.Apocalypse.ApocalypseEffects;

namespace TheLastStand.Model.Apocalypse.ApocalypseEffects;

public class AddEnemiesStatModifierFromTurnApocalypseEffect : AApocalypseEffect
{
	public AddEnemiesStatModifierFromTurnApocalypseEffectController StatModifierEffectController => base.AApocalypseEffectController as AddEnemiesStatModifierFromTurnApocalypseEffectController;

	public AddEnemiesStatModifierFromTurnApocalypseEffectDefinition StatModifierEffectDefinition => base.AApocalypseEffectDefinition as AddEnemiesStatModifierFromTurnApocalypseEffectDefinition;

	public bool IsActive { get; set; }

	public bool IsHookedForActivation { get; set; }

	public bool IsHookedForDeactivation { get; set; }

	public AddEnemiesStatModifierFromTurnApocalypseEffect(AddEnemiesStatModifierFromTurnApocalypseEffectDefinition effectDefinition, AddEnemiesStatModifierFromTurnApocalypseEffectController effectController)
		: base(effectDefinition, effectController)
	{
	}

	public bool CanActivateAtTurn(int turnNb)
	{
		return turnNb >= StatModifierEffectDefinition.Turn;
	}
}
