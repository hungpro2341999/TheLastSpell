using TMPro;
using TPLib;
using TPLib.Localization.Fonts;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Building.Construction;

public class CostPanel : MonoBehaviour
{
	private static class Constants
	{
		public const string CostTextFormat = "<style={0}>{1}</style>";
	}

	[SerializeField]
	private TextMeshProUGUI costText;

	[SerializeField]
	private LocalizedFont costLocalizedFont;

	public void Refresh(int cost, bool isGold)
	{
		bool num = (isGold ? (TPSingleton<ResourceManager>.Instance.Gold >= cost) : (TPSingleton<ResourceManager>.Instance.Materials >= cost));
		string text = cost.ToString();
		if (!num)
		{
			text = "<style=\"Bad\">" + text + "</style>";
		}
		text = string.Format("<style={0}>{1}</style>", isGold ? "Gold" : "Materials", text);
		costText.text = text;
		if (costLocalizedFont != null)
		{
			costLocalizedFont.RefreshFont();
		}
	}
}
