using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TPLib;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.TileMap;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.View;

public class FogView : MonoBehaviour
{
	public static class Constants
	{
		public static class LocalizationPrefixes
		{
			public const string FogNamePrefix = "FogName_";

			public const string FogDescriptionPrefix = "FogDescription_";
		}

		public static class Sprites
		{
			public const string FogIconPrefix = "View/Sprites/UI/Fog/Icons/FogIcon_";
		}

		public static class Names
		{
			public const string FogName = "Fog";

			public const string LightFogName = "LightFog";
		}
	}

	[SerializeField]
	private float dayIntensity;

	[SerializeField]
	private float nightIntensity = 1f;

	[SerializeField]
	private float intensityTransitionDuration = 2f;

	[SerializeField]
	private Ease nightIntensityTransitionEasing = Ease.InOutSine;

	[SerializeField]
	private Ease dayIntensityTransitionEasing = Ease.InOutSine;

	[SerializeField]
	private float fogAreaTransitionDuration = 2f;

	[SerializeField]
	private Ease appearFogAreaTransitionEasing = Ease.OutCubic;

	[SerializeField]
	private Ease disappearFogAreaTransitionEasing = Ease.OutCubic;

	[SerializeField]
	private float fogLimitDayIntensity;

	[SerializeField]
	private float fogLimitNightIntensity = 1f;

	[SerializeField]
	private Ease densityFadeInEasing = Ease.OutCubic;

	[SerializeField]
	private float densityFadeInDuration = 0.4f;

	[SerializeField]
	private Ease densityFadeOutEasing = Ease.OutCubic;

	[SerializeField]
	private float densityFadeOutDuration = 0.8f;

	[SerializeField]
	[Tooltip("Time to wait after night report before going to the nearest fog to show the increase.")]
	private float waitBeforeFogIncreaseSequence = 0.5f;

	[SerializeField]
	[Tooltip("Time to wait starting right after the camera movement has started to go show the nearest fog.")]
	private float waitBeforeFogIncreaseShow = 1f;

	[SerializeField]
	[Tooltip("Time to wait after the fog increaseing has been shown and the arrows has been displayed.")]
	private float waitAfterFogIncreaseShow = 1f;

	[SerializeField]
	private float fogFocusTime = 0.5f;

	[SerializeField]
	[Tooltip("The material applied to all the enemies in fog or light fog.")]
	private Material enemiesInFogMaterial;

	private Tween fogAlphaTween;

	private Color fogAreaColorInit;

	private Tween fogAreaIntensityTween;

	private float fogIntensity;

	private float fogLimitIntensity;

	private Tweener fogIntensityTweener;

	private Tweener fogLimitIntensityTweener;

	public float FogFocusTime => fogFocusTime;

	public float FogIntensity
	{
		get
		{
			return fogIntensity;
		}
		private set
		{
			fogIntensity = value;
			Color color = TileMapView.FogTilemap.color;
			color.a = fogIntensity;
			TileMapView.FogTilemap.color = color;
		}
	}

	public float FogLimitIntensity
	{
		get
		{
			return fogLimitIntensity;
		}
		private set
		{
			fogLimitIntensity = value;
			Color color = TileMapView.FogLimitTilemap.color;
			color.a = fogLimitIntensity;
			TileMapView.FogLimitTilemap.color = color;
		}
	}

	public Material EnemiesInFogMaterial => enemiesInFogMaterial;

	public float WaitAfterFogIncreaseShow => waitAfterFogIncreaseShow;

	public float WaitBeforeFogIncreaseSequence => waitBeforeFogIncreaseSequence;

	public float WaitBeforeFogIncreaseShow => waitBeforeFogIncreaseShow;

	public void DisplayFog(bool instant)
	{
		bool flag = TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night;
		float intensity = (flag ? nightIntensity : dayIntensity);
		float limitIntensity = (flag ? fogLimitNightIntensity : fogLimitDayIntensity);
		if (flag || dayIntensity > 0f)
		{
			TileMapView.SetTiles(TileMapView.FogTilemap, TPSingleton<FogManager>.Instance.Fog.FogTiles, "View/Tiles/World/Fog");
		}
		TileMapView.FogLimitTilemap.ClearAllTiles();
		TileMapView.SetTiles(TileMapView.FogLimitTilemap, TPSingleton<FogManager>.Instance.Fog.TilesOutOfFog, "View/Tiles/Feedbacks/MistLimits/MistLimits");
		if (instant)
		{
			FogIntensity = intensity;
			FogLimitIntensity = limitIntensity;
		}
		else
		{
			StartCoroutine(SetFogAndLimitIntensityCoroutine(intensity, limitIntensity, intensityTransitionDuration, flag ? nightIntensityTransitionEasing : dayIntensityTransitionEasing));
		}
	}

	public void FadeFogTiles(IEnumerable<Tile> tiles, bool fadeIn, bool instantly = false)
	{
		if (instantly)
		{
			foreach (Tile tile in tiles)
			{
				if (fadeIn)
				{
					TileMapView.SetTile(TileMapView.FogTilemap, tile, "View/Tiles/World/Fog");
				}
				else
				{
					TileMapView.SetTile(TileMapView.FogTilemap, tile);
				}
			}
			return;
		}
		StartCoroutine(TPSingleton<TileMapView>.Instance.FadeTilesAlphaCoroutine(tiles, fadeIn, TileMapView.FogTilemap, "View/Tiles/World/Fog", fadeIn ? densityFadeInDuration : densityFadeOutDuration, fadeIn ? densityFadeInEasing : densityFadeOutEasing));
	}

	private void Awake()
	{
		fogAreaColorInit = TileMapView.FogAreaTilemap.color;
		DisplayFog(instant: true);
	}

	public void ChangeFogAreaIntensity(bool show)
	{
		float endValue = (show ? fogAreaColorInit.a : 0f);
		TileMapView.FogAreaTilemap.color = new Color(fogAreaColorInit.r, fogAreaColorInit.g, fogAreaColorInit.b, show ? 0f : fogAreaColorInit.a);
		fogAreaIntensityTween?.Kill();
		fogAreaIntensityTween = DOTween.To(GetFogAlpha, SetFogAlpha, endValue, fogAreaTransitionDuration).SetFullId("FogAreaIntensity", this).SetEase(show ? appearFogAreaTransitionEasing : disappearFogAreaTransitionEasing);
	}

	private float GetFogAlpha()
	{
		if (!(ApplicationManager.Application.State.GetName() == "Game"))
		{
			return 0f;
		}
		return TileMapView.FogAreaTilemap.color.a;
	}

	private void SetFogAlpha(float value)
	{
		if (!(ApplicationManager.Application.State.GetName() != "Game"))
		{
			TileMapView.FogAreaTilemap.color = new Color(fogAreaColorInit.r, fogAreaColorInit.g, fogAreaColorInit.b, value);
		}
	}

	private IEnumerator SetFogAndLimitIntensityCoroutine(float intensity, float limitIntensity, float duration, Ease easing)
	{
		if (fogIntensityTweener != null && fogIntensityTweener.IsPlaying())
		{
			fogIntensityTweener.ChangeEndValue(intensity, duration, snapStartValue: true);
		}
		else
		{
			fogIntensityTweener = DOTween.To(() => FogIntensity, delegate(float v)
			{
				FogIntensity = v;
			}, intensity, duration).SetId("FogIntensity").SetEase(easing)
				.OnKill(delegate
				{
					fogIntensityTweener = null;
				});
		}
		if (fogLimitIntensityTweener != null && fogLimitIntensityTweener.IsPlaying())
		{
			fogLimitIntensityTweener.ChangeEndValue(limitIntensity, duration, snapStartValue: true);
		}
		else
		{
			fogLimitIntensityTweener = DOTween.To(() => FogLimitIntensity, delegate(float v)
			{
				FogLimitIntensity = v;
			}, limitIntensity, duration).SetId("FogLimitIntensity").SetEase(easing)
				.OnKill(delegate
				{
					fogLimitIntensityTweener = null;
				});
		}
		yield return fogIntensityTweener.WaitForCompletion();
		if (fogLimitIntensityTweener != null)
		{
			yield return fogLimitIntensityTweener.WaitForCompletion();
		}
	}
}
