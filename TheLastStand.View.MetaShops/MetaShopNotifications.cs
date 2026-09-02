using UnityEngine;

namespace TheLastStand.View.MetaShops;

public class MetaShopNotifications : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem[] notificationParticles;

	[SerializeField]
	private GameObject[] notificationObjects;

	public void Toggle(bool show)
	{
		for (int num = notificationParticles.Length - 1; num >= 0; num--)
		{
			notificationParticles[num].gameObject.SetActive(show);
			if (show)
			{
				notificationParticles[num].Play();
			}
			else
			{
				notificationParticles[num].Stop();
			}
		}
		for (int num2 = notificationObjects.Length - 1; num2 >= 0; num2--)
		{
			notificationObjects[num2].SetActive(show);
		}
	}
}
