using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLastStand.View.Building;

public class DamagedBuildingFlameView : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer spriteRenderer;

	[SerializeField]
	private Animator animator;

	private bool isUnloading;

	public Animator Animator => animator;

	public SpriteRenderer SpriteRenderer => spriteRenderer;

	private void Start()
	{
		SceneManager.sceneUnloaded += delegate
		{
			isUnloading = true;
		};
	}

	private void OnDestroy()
	{
		if (!isUnloading)
		{
			_ = base.gameObject.scene.isLoaded;
		}
	}
}
