using TPLib;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Panic;
using UnityEngine;

namespace TheLastStand.View.Panic;

public class PanicView : MonoBehaviour
{
	[SerializeField]
	private PanicPanel panicPanel;

	public TheLastStand.Model.Panic.Panic Panic { get; set; }

	public PanicPanel PanicPanel => panicPanel;

	public bool DisplayOrHide()
	{
		if (Panic == null)
		{
			return false;
		}
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && !CutsceneManager.AnyCutscenePlaying)
		{
			panicPanel.Open();
			return true;
		}
		panicPanel.Close();
		return false;
	}

	public void Hide()
	{
		panicPanel.Close();
	}

	public void Refresh()
	{
		if (DisplayOrHide())
		{
			panicPanel.Refresh(Panic);
		}
	}

	public void RefreshPanicExpectedValue()
	{
		if (DisplayOrHide())
		{
			panicPanel.RefreshPanicExpectedValue(Panic);
		}
	}

	public void RefreshPanicValue()
	{
		if (DisplayOrHide())
		{
			panicPanel.RefreshPanicValue(Panic);
		}
	}
}
