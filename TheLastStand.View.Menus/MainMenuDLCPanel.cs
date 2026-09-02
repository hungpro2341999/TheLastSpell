using System.Collections.Generic;
using TPLib;
using TheLastStand.Definition.DLC;
using TheLastStand.Manager.DLC;
using UnityEngine;

namespace TheLastStand.View.Menus;

public class MainMenuDLCPanel : MonoBehaviour
{
	[SerializeField]
	private RectTransform dlcDisplayContainer;

	[SerializeField]
	private MainMenuDLCDisplay dlcDisplayPrefab;

	private List<MainMenuDLCDisplay> dlcDisplays = new List<MainMenuDLCDisplay>();

	public void Init()
	{
		if (dlcDisplays.Count > 0)
		{
			foreach (MainMenuDLCDisplay dlcDisplay in dlcDisplays)
			{
				dlcDisplay.gameObject.SetActive(value: false);
			}
		}
		if (!TPSingleton<DLCManager>.Exist())
		{
			return;
		}
		int count = TPSingleton<DLCManager>.Instance.OwnedDLCIds.Count;
		if (count <= 0)
		{
			return;
		}
		CreateMissingDLCDisplays(count);
		int num = 0;
		foreach (string ownedDLCId in TPSingleton<DLCManager>.Instance.OwnedDLCIds)
		{
			DLCDefinition dLCFromId = TPSingleton<DLCManager>.Instance.GetDLCFromId(ownedDLCId);
			if (dLCFromId != null)
			{
				MainMenuDLCDisplay mainMenuDLCDisplay = dlcDisplays[num];
				mainMenuDLCDisplay.SetContent(dLCFromId);
				mainMenuDLCDisplay.gameObject.SetActive(value: true);
				mainMenuDLCDisplay.Refresh();
				num++;
			}
		}
	}

	private void Awake()
	{
		Init();
	}

	private void CreateMissingDLCDisplays(int totalNeededDisplays)
	{
		int count = dlcDisplays.Count;
		if (totalNeededDisplays > count)
		{
			int num = totalNeededDisplays - count;
			for (int i = 0; i < num; i++)
			{
				MainMenuDLCDisplay item = Object.Instantiate(dlcDisplayPrefab, dlcDisplayContainer.transform);
				dlcDisplays.Add(item);
			}
		}
	}
}
