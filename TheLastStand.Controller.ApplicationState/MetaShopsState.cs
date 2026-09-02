using TheLastStand.Framework.Automaton;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using UnityEngine.SceneManagement;

namespace TheLastStand.Controller.ApplicationState;

public class MetaShopsState : State
{
	public const string Name = "MetaShops";

	public override string GetName()
	{
		return "MetaShops";
	}

	public override void OnStateEnter()
	{
		if (SceneManager.GetActiveScene().name != ScenesManager.MetaShopSceneName)
		{
			MetaNarrationsManager.NarrationDoneThisDay = false;
			SceneManager.LoadScene(ScenesManager.MetaShopSceneName);
		}
	}
}
