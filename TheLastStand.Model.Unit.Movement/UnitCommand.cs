using TheLastStand.Definition;
using TheLastStand.Framework.Command;
using TheLastStand.Framework.Command.Conversation;

namespace TheLastStand.Model.Unit.Movement;

public abstract class UnitCommand : ICompensableCommand, ICommand
{
	private GameDefinition.E_Direction startDirection;

	public PlayableUnit PlayableUnit { get; private set; }

	protected UnitCommand(PlayableUnit playableUnit)
	{
		PlayableUnit = playableUnit;
	}

	public abstract bool Execute();

	public abstract void Compensate();
}
