using TheLastStand.Framework.Automaton;
using TheLastStand.Manager;
using UnityEngine.SceneManagement;

namespace TheLastStand.Controller.ApplicationState;

public class CreditsState : State
{
	public const string Name = "Credits";

	public override string GetName()
	{
		return "Credits";
	}

	public override void OnStateEnter()
	{
		if (!(SceneManager.GetActiveScene().name == ScenesManager.CreditsSceneName))
		{
			SceneManager.LoadScene(ScenesManager.CreditsSceneName);
		}
	}
}
