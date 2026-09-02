using System.Collections.Generic;
using Rewired;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.View.Camera;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View;

public class WorldInputsView : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public delegate void OnPointerOverWorldChanged(bool isPointerOverWorld);

	private static List<RaycastResult> raycastResults = new List<RaycastResult>();

	[SerializeField]
	private bool verbose;

	[SerializeField]
	protected BoxCollider2D worldCollider;

	[SerializeField]
	private EventSystem eventSystem;

	private bool isPointerOverWorld;

	protected float camRefAspect = -1f;

	protected float camRefOrthoSize = -1f;

	private GraphicRaycaster[] graphicsRaycasters;

	public bool IsPointerOverWorld
	{
		get
		{
			if (SteamManager.IsRunningOnSteamDeck)
			{
				if (!TheLastStand.Manager.InputManager.IsLastControllerJoystick)
				{
					return isPointerOverWorld;
				}
				return true;
			}
			return isPointerOverWorld;
		}
		set
		{
			if (isPointerOverWorld != value)
			{
				isPointerOverWorld = value;
				if (SteamManager.IsRunningOnSteamDeck)
				{
					this.PointerOverWorldChangeEvent?.Invoke(IsPointerOverWorld);
				}
				else
				{
					this.PointerOverWorldChangeEvent?.Invoke(isPointerOverWorld);
				}
			}
		}
	}

	public event OnPointerOverWorldChanged PointerOverWorldChangeEvent;

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (verbose)
		{
			TPDebug.Log("Pointer Enter", this);
		}
		IsPointerOverWorld = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (verbose)
		{
			TPDebug.Log("Pointer Exit", this);
		}
		IsPointerOverWorld = false;
	}

	private void Awake()
	{
		if (worldCollider == null)
		{
			worldCollider = GetComponent<BoxCollider2D>();
		}
		graphicsRaycasters = TPSingleton<GameView>.Instance.MainCanvasGameObject.GetComponentsInChildren<GraphicRaycaster>(includeInactive: true);
		IsPointerOverWorld = true;
		if (SteamManager.IsRunningOnSteamDeck)
		{
			TheLastStand.Manager.InputManager.LastActiveControllerChanged += OnLastActiveControllerChanged;
		}
	}

	private bool IsPointerOverUI(bool forceNoLog = false)
	{
		PointerEventData eventData = new PointerEventData(eventSystem)
		{
			position = TheLastStand.Manager.InputManager.MousePosition
		};
		raycastResults.Clear();
		if (!verbose || forceNoLog)
		{
			int i = 0;
			for (int num = graphicsRaycasters.Length; i < num; i++)
			{
				graphicsRaycasters[i].Raycast(eventData, raycastResults);
				if (raycastResults.Count > 0)
				{
					return true;
				}
			}
			return false;
		}
		bool flag = false;
		int j = 0;
		for (int num2 = graphicsRaycasters.Length; j < num2; j++)
		{
			graphicsRaycasters[j].Raycast(eventData, raycastResults);
			if (raycastResults.Count > 0)
			{
				flag = true;
				for (int k = 0; k < raycastResults.Count; k++)
				{
					TPDebug.Log("The cursor is over '" + raycastResults[k].gameObject.name + "'", graphicsRaycasters[j]);
				}
				raycastResults.Clear();
			}
		}
		if (flag)
		{
			Debug.Log("---------- END OF RAYCAST --------------");
		}
		return flag;
	}

	private void OnDestroy()
	{
		if (SteamManager.IsRunningOnSteamDeck)
		{
			TheLastStand.Manager.InputManager.LastActiveControllerChanged -= OnLastActiveControllerChanged;
		}
	}

	private void OnLastActiveControllerChanged(ControllerType controllerType)
	{
		this.PointerOverWorldChangeEvent?.Invoke(IsPointerOverWorld);
	}

	public virtual void LateUpdate()
	{
		if (camRefOrthoSize != ACameraView.MainCam.orthographicSize)
		{
			camRefOrthoSize = ACameraView.MainCam.orthographicSize;
			camRefAspect = ACameraView.MainCam.aspect;
			Vector2 size = new Vector2(camRefOrthoSize * 2f * camRefAspect, camRefOrthoSize * 2f);
			size *= 1.1f;
			worldCollider.size = size;
		}
	}
}
