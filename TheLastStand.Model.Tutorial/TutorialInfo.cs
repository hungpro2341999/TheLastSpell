using System.Collections.Generic;
using TheLastStand.View.Tutorial;

namespace TheLastStand.Model.Tutorial;

public class TutorialInfo
{
	public E_TutorialTrigger trigger;

	public List<TutorialPopup> TutorialPopups = new List<TutorialPopup>();

	public bool IsClosed { get; }

	public void Display()
	{
		foreach (TutorialPopup tutorialPopup in TutorialPopups)
		{
			_ = tutorialPopup;
		}
	}
}
