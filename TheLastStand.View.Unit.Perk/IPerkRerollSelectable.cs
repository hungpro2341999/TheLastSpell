namespace TheLastStand.View.Unit.Perk;

public interface IPerkRerollSelectable
{
	bool CanCollectionRerollsCompletely { get; }

	bool IsCollectionRerollCompletelyLocked { get; }

	bool DoesCollectionRerollsCompletely { get; }

	void ExecuteReroll();

	void ChangeSelection(bool isSelected);

	bool CanReroll();

	bool CanPayTargetReroll();

	bool HasPotentialReroll();
}
