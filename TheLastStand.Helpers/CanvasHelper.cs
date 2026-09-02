using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.Helpers;

public static class CanvasHelper
{
	public static void ScaleCanvas(CanvasScaler canvasScaler, bool allowDecimals = true)
	{
		canvasScaler.scaleFactor = ((Screen.height >= 2160) ? 2f : ((allowDecimals && Screen.height <= 768) ? (2f / 3f) : 1f));
	}

	public static void ScaleCanvasTowards720P(CanvasScaler canvasScaler)
	{
		canvasScaler.scaleFactor = ((Screen.height >= 1440) ? 2f : 1f);
	}
}
