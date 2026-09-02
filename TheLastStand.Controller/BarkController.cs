using TheLastStand.Definition;
using TheLastStand.Model;
using TheLastStand.View;

namespace TheLastStand.Controller;

public class BarkController
{
	public Bark Bark { get; private set; }

	public BarkController(BarkDefinition definition, BarkView view, IBarker barker)
	{
		Bark = new Bark(definition, this, view);
		view.Bark = Bark;
		Bark.Barker = barker;
		Bark.Sentence = BarkDefinition.GetSentence(definition, barker);
	}
}
