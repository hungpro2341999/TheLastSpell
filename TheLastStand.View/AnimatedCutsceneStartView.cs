using TPLib;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View;

public class AnimatedCutsceneStartView : MonoBehaviour
{
	private void Start()
	{
		string cutsceneId = "Introduction";
		if (TPSingleton<AnimatedCutsceneManager>.Instance.CurrentCutsceneId != null)
		{
			cutsceneId = TPSingleton<AnimatedCutsceneManager>.Instance.CurrentCutsceneId;
		}
		TPSingleton<AnimatedCutsceneView>.Instance.StartAnimatedCutScene(cutsceneId);
	}
}
