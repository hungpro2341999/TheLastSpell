using UnityEngine;

namespace TheLastStand.View.Settings;

public class OtherSettingsPanel : MonoBehaviour
{
	[SerializeField]
	private AllowDataCollectionPanel allowDataCollectionPanelPanel;

	public void Refresh()
	{
		allowDataCollectionPanelPanel.Refresh();
	}
}
