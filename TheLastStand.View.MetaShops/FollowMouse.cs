using UnityEngine;

namespace TheLastStand.View.MetaShops;

public class FollowMouse : MonoBehaviour
{
	private void Update()
	{
		((RectTransform)base.transform).position = Input.mousePosition;
	}
}
