using TMPro;
using TPLib;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Resource;

public class GoldPanel : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI goldText;

	protected virtual void Awake()
	{
		goldText.text = $"{TPSingleton<ResourceManager>.Instance.Gold}";
		TPSingleton<ResourceManager>.Instance.OnGoldChange += OnGoldChanged;
	}

	private void OnDestroy()
	{
		if (TPSingleton<ResourceManager>.Exist())
		{
			TPSingleton<ResourceManager>.Instance.OnGoldChange -= OnGoldChanged;
		}
	}

	private void OnGoldChanged(int gold)
	{
		goldText.text = gold.ToString();
	}
}
