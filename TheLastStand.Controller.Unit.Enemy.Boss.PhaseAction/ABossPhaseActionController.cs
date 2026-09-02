using System.Collections;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;
using TheLastStand.Model;
using TheLastStand.Model.Unit.Boss;

namespace TheLastStand.Controller.Unit.Enemy.Boss.PhaseAction;

public abstract class ABossPhaseActionController
{
	public int ActionIndex { get; }

	public BossPhaseHandler BossPhaseParentHandler { get; }

	public ABossPhaseActionDefinition ABossPhaseActionDefinition { get; }

	public int Delay => ABossPhaseActionDefinition.DelayExpression?.EvalToInt(FormulaInterpreterContext) ?? 0;

	public FormulaInterpreterContext FormulaInterpreterContext { get; }

	public ABossPhaseActionController(ABossPhaseActionDefinition aBossPhaseActionDefinition, BossPhaseHandler bossPhaseHandlerParent, int actionIndex)
	{
		ABossPhaseActionDefinition = aBossPhaseActionDefinition;
		FormulaInterpreterContext = new FormulaInterpreterContext(this);
		BossPhaseParentHandler = bossPhaseHandlerParent;
		ActionIndex = actionIndex;
	}

	public abstract IEnumerator Execute();
}
