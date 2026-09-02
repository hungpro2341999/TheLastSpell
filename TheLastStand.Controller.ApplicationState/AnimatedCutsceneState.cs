using TheLastStand.Framework.Automaton;
using TheLastStand.Manager;
using UnityEngine.SceneManagement;

namespace TheLastStand.Controller.ApplicationState;

public class AnimatedCutsceneState : State
{
	public const string Name = "AnimatedCutscene";

	public override string GetName()
	{
		return "AnimatedCutscene";
	}

	public override void OnStateEnter()
	{
		SceneManager.LoadScene(ScenesManager.AnimatedCutsceneSceneName);
	}
}
