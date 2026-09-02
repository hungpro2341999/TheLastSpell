using TMPro;
using UnityEngine;

namespace TheLastStand.Dev;

public class MetaConditionSeparatorDebugView : MonoBehaviour
{
	[SerializeField]
	private new TextMeshProUGUI name;

	public void SetText(string text)
	{
		name.text = text;
	}
}
