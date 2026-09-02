using UnityEngine;

namespace TheLastStand.View;

public class AnimatedCutscenePrefab : MonoBehaviour
{
	[SerializeField]
	private string cutsceneId;

	[SerializeField]
	private GameObject[] slides;

	public GameObject[] Slides => slides;

	public string CutsceneId => cutsceneId;
}
