using TPLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.WorldMap.Glyphs;

public class PreviewedGlyphDisplay : AGlyphDisplay
{
	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		TPSingleton<GameConfigurationsView>.Instance.AdjustScrollView(base.transform as RectTransform);
	}
}
