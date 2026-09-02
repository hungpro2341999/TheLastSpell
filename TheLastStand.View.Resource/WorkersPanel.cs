using TMPro;
using TPLib;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Resource;

public class WorkersPanel : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI workersText;

	private void Awake()
	{
		workersText.text = $"{TPSingleton<ResourceManager>.Instance.Workers}/{TPSingleton<ResourceManager>.Instance.MaxWorkers}";
		TPSingleton<ResourceManager>.Instance.OnWorkersChange += OnWorkersChange;
	}

	private void OnDestroy()
	{
		if (TPSingleton<ResourceManager>.Exist())
		{
			TPSingleton<ResourceManager>.Instance.OnWorkersChange -= OnWorkersChange;
		}
	}

	private void OnWorkersChange(int workers, int maxWorkers)
	{
		workersText.text = $"{workers}/{maxWorkers}";
	}
}
