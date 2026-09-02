using Sirenix.OdinInspector;
using TPLib.Localization.Fonts;
using UnityEngine;

namespace TheLastStand.View;

public abstract class TabbedPageView : SerializedMonoBehaviour
{
	[SerializeField]
	protected Canvas pageCanvas;

	[SerializeField]
	protected FontLocalizedParent fontLocalizedParent;

	public bool IsOpened { get; private set; } = true;

	public bool IsDirty { get; set; }

	public virtual void Close()
	{
		IsOpened = false;
		pageCanvas.enabled = false;
	}

	public virtual void Open()
	{
		IsOpened = true;
		pageCanvas.enabled = true;
		IsDirty = true;
		fontLocalizedParent?.RefreshChildren();
	}

	public virtual void Refresh()
	{
		IsDirty = false;
	}

	protected virtual void Start()
	{
		Close();
	}

	protected virtual void Update()
	{
		if (IsDirty)
		{
			Refresh();
		}
	}
}
