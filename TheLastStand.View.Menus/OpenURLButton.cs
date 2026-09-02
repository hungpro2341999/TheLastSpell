using TPLib;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Menus;

public class OpenURLButton : MonoBehaviour
{
	[SerializeField]
	private string urlToOpen = string.Empty;

	public void OpenURL()
	{
		if (urlToOpen == string.Empty)
		{
			TPSingleton<ApplicationManager>.Instance.LogWarning("There is no url to upen on that button");
		}
		else
		{
			Application.OpenURL(urlToOpen);
		}
	}
}
