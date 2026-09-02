using TheLastStand.Controller;
using TheLastStand.Definition;
using TheLastStand.View;

namespace TheLastStand.Model;

public class Bark
{
	public BarkController BarkController { get; private set; }

	public BarkDefinition BarkDefinition { get; private set; }

	public BarkView BarkView { get; private set; }

	public bool IgnoreDeathCheck { get; set; }

	public IBarker Barker { get; set; }

	public string Sentence { get; set; }

	public float WaitTime { get; set; }

	public Bark(BarkDefinition definition, BarkController controller, BarkView view)
	{
		BarkDefinition = definition;
		BarkController = controller;
		BarkView = view;
	}
}
