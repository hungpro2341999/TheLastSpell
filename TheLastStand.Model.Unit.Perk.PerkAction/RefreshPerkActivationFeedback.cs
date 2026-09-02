using TheLastStand.Controller.Unit.Perk.PerkAction;
using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;

namespace TheLastStand.Model.Unit.Perk.PerkAction;

public class RefreshPerkActivationFeedback : APerkAction
{
	public RefreshPerkActivationFeedbackDefinition RefreshPerkActivationFeedbackDefinition => PerkActionDefinition as RefreshPerkActivationFeedbackDefinition;

	public RefreshPerkActivationFeedback(RefreshPerkActivationFeedbackDefinition perkActionDefinition, RefreshPerkActivationFeedbackController perkActionController, TheLastStand.Model.Unit.Perk.PerkEvent.PerkEvent perkEvent)
		: base(perkActionDefinition, perkActionController, perkEvent)
	{
	}
}
