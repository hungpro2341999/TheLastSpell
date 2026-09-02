using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.MetaShops;

public class OraculumUnlockIcon : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	[SerializeField]
	private PointerEventsListener pointerEventsListener;

	private MetaUpgradeLineView metaUpgradeLineView;

	private Selectable selectable;

	public PointerEventsListener PointerEventsListener => pointerEventsListener;

	public Selectable Selectable
	{
		get
		{
			if (selectable == null)
			{
				selectable = GetComponent<Selectable>();
			}
			return selectable;
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		RefreshTopAndBottomNavigation();
	}

	public virtual void OnDeselect(BaseEventData eventData)
	{
		metaUpgradeLineView.OnIconDeselect();
	}

	protected void SetMetaUpgrade(MetaUpgradeLineView metaUpgradeLineView)
	{
		this.metaUpgradeLineView = metaUpgradeLineView;
		Selectable.SetMode(Navigation.Mode.Explicit);
		RefreshTopAndBottomNavigation();
	}

	private void RefreshTopAndBottomNavigation()
	{
		Transform obj = base.transform;
		int num = 5;
		int siblingIndex = obj.GetSiblingIndex();
		int childCount = obj.parent.transform.childCount;
		int num2 = childCount % num;
		int num3 = ((num2 == 0) ? (childCount - num) : (childCount - num2));
		if (siblingIndex < num)
		{
			Selectable.SetSelectOnUp(metaUpgradeLineView.JoystickSelectable.navigation.selectOnUp);
		}
		if (siblingIndex >= num3)
		{
			Selectable.SetSelectOnDown(metaUpgradeLineView.JoystickSelectable.navigation.selectOnDown);
		}
	}
}
