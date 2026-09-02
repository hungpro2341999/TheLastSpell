using TheLastStand.Definition.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.MetaShops;

public class MetaShopFilterLink : MonoBehaviour
{
	[SerializeField]
	private MetaShopFilter firstFilter;

	[SerializeField]
	private MetaShopFilter secondFilter;

	[SerializeField]
	private Image switchImage;

	[SerializeField]
	private Sprite switchIconFirstOn;

	[SerializeField]
	private Sprite switchIconSecondOn;

	[SerializeField]
	private Sprite switchIconOff;

	public void Refresh()
	{
		switchImage.sprite = (firstFilter.Toggled ? switchIconFirstOn : (secondFilter.Toggled ? switchIconSecondOn : switchIconOff));
	}

	private void Start()
	{
		firstFilter.OnToggled += OnFilterToggled;
		secondFilter.OnToggled += OnFilterToggled;
	}

	private void OnFilterToggled(MetaUpgradeDefinition.E_MetaUpgradeFilter filter, bool toggle)
	{
		Refresh();
	}

	private void OnDestroy()
	{
		firstFilter.OnToggled -= OnFilterToggled;
		secondFilter.OnToggled -= OnFilterToggled;
	}
}
