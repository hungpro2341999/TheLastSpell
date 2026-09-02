using System;
using TMPro;
using TPLib.Localization;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View;

[RequireComponent(typeof(TMP_Text))]
public class HyperlinkListener : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField]
	private string openTag;

	[SerializeField]
	private string closeTag;

	[SerializeField]
	private string openTagOnHover;

	[SerializeField]
	private string closeTagOnHover;

	private TMP_Text textMesh;

	private bool pointerIsHover;

	private TMP_LinkInfo? hoverLink;

	private int previousLinkIndex;

	private TMP_Text TextMesh
	{
		get
		{
			if (textMesh == null)
			{
				textMesh = GetComponent<TMP_Text>();
			}
			return textMesh;
		}
	}

	private void Start()
	{
		ForceRefresh();
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		previousLinkIndex = -1;
		ForceRefresh();
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnDisable()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	public void ForceRefresh()
	{
		TextMesh.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
		TMP_LinkInfo[] linkInfo = TextMesh.textInfo.linkInfo;
		for (int i = 0; i < linkInfo.Length; i++)
		{
			TMP_LinkInfo linkInfo2 = TextMesh.textInfo.linkInfo[i];
			SetStyle(linkInfo2, openTag, closeTag);
			TextMesh.ForceMeshUpdate();
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		int num = TMP_TextUtilities.FindIntersectingLink(TextMesh, eventData.position, null);
		if (num != -1)
		{
			TMP_LinkInfo tMP_LinkInfo = TextMesh.textInfo.linkInfo[num];
			Application.OpenURL(tMP_LinkInfo.GetLinkID());
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		pointerIsHover = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		pointerIsHover = false;
	}

	private void Update()
	{
		if (!pointerIsHover)
		{
			return;
		}
		Vector3 mousePosition = Input.mousePosition;
		int num = TMP_TextUtilities.FindIntersectingLink(TextMesh, mousePosition, null);
		if (num != -1 && num != previousLinkIndex)
		{
			if (previousLinkIndex != -1 && num != previousLinkIndex && hoverLink.HasValue)
			{
				TMP_LinkInfo linkInfo = TextMesh.textInfo.linkInfo[previousLinkIndex];
				RemoveStyle(linkInfo, openTagOnHover, closeTagOnHover);
				SetStyle(linkInfo, openTag, closeTag);
			}
			TMP_LinkInfo tMP_LinkInfo = TextMesh.textInfo.linkInfo[num];
			RemoveStyle(tMP_LinkInfo, openTag, closeTag);
			SetStyle(tMP_LinkInfo, openTagOnHover, closeTagOnHover);
			hoverLink = tMP_LinkInfo;
			TextMesh.ForceMeshUpdate();
		}
		else if (num == -1 && previousLinkIndex != -1 && hoverLink.HasValue)
		{
			TMP_LinkInfo linkInfo2 = TextMesh.textInfo.linkInfo[previousLinkIndex];
			if (hoverLink.Value.hashCode == linkInfo2.hashCode)
			{
				RemoveStyle(linkInfo2, openTagOnHover, closeTagOnHover);
				SetStyle(linkInfo2, openTag, closeTag);
				hoverLink = null;
				TextMesh.ForceMeshUpdate();
			}
		}
		previousLinkIndex = num;
	}

	private void SetStyle(TMP_LinkInfo linkInfo, string openTg, string closeTg)
	{
		string linkText = linkInfo.GetLinkText();
		string newValue = openTg + linkText + closeTg;
		TextMesh.text = TextMesh.text.Replace(linkText, newValue);
	}

	private void RemoveStyle(TMP_LinkInfo linkInfo, string openTg, string closeTg)
	{
		string linkText = linkInfo.GetLinkText();
		string oldValue = openTg + linkText + closeTg;
		TextMesh.text = TextMesh.text.Replace(oldValue, linkText);
	}
}
