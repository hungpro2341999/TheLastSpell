using System.Collections.Generic;
using DG.Tweening;
using TheLastStand.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.Apocalypse;

public class ApocalypseHeaderSealDisplay : MonoBehaviour
{
	private static class Constants
	{
		public const string SealIconPath = "View/Sprites/UI/Apocalypse/Seals/";

		public const string SealRedOffIconNameFormat = "Apocalypse_Seal_{0}_off";

		public const string SealRedOnIconNameFormat = "Apocalypse_Seal_{0}_on";

		public const string SealCityColorOffIconNameFormat = "Apocalypse_Seal_WorldMap_{0}_Off";

		public const string SealCityColorOnIconNameFormat = "Apocalypse_Seal_WorldMap_{0}_On";
	}

	[SerializeField]
	private Image sealIcon;

	[SerializeField]
	private RectTransform sealIconRectTransform;

	[SerializeField]
	private float sealTweenDuration = 1f;

	[SerializeField]
	private int sealTweenYTarget = 5;

	[SerializeField]
	private Ease sealTweenEasing = Ease.InOutSine;

	private Tween sealMoveTween;

	private string currentCityId;

	private bool mustAnimateSeal;

	private bool atLeastOneApoLevelCompleted;

	private bool isCityViewSeal;

	private Vector2 sealDefaultAnachoredPosition;

	private Dictionary<string, Sprite> cachedRedSealSpriteOff = new Dictionary<string, Sprite>();

	private Dictionary<string, Sprite> cachedRedSealSpriteOn = new Dictionary<string, Sprite>();

	private Dictionary<string, Sprite> cachedCityColorSealSpriteOff = new Dictionary<string, Sprite>();

	private Dictionary<string, Sprite> cachedCityColorSealSpriteOn = new Dictionary<string, Sprite>();

	public void ContinueAnimations()
	{
		CheckIfMustInitTween();
	}

	public void Init(string cityId, bool isAnimated)
	{
		currentCityId = cityId;
		mustAnimateSeal = isAnimated;
		isCityViewSeal = false;
		InitCachedSprites();
		RefreshSeal(resetSealPosition: false);
	}

	public void Init(string cityId, bool isCityCompleted, bool oneApocalypseLevelCompleted)
	{
		currentCityId = cityId;
		mustAnimateSeal = isCityCompleted;
		atLeastOneApoLevelCompleted = oneApocalypseLevelCompleted;
		isCityViewSeal = true;
		InitCachedSprites();
		RefreshSeal(resetSealPosition: true);
	}

	public void PauseAnimations()
	{
		if (sealMoveTween != null)
		{
			sealMoveTween.Pause();
		}
	}

	private void Awake()
	{
		sealDefaultAnachoredPosition = sealIconRectTransform.anchoredPosition;
	}

	private void CheckIfMustInitTween()
	{
		if (mustAnimateSeal)
		{
			if (sealMoveTween == null)
			{
				sealMoveTween = sealIconRectTransform.DOAnchorPosY(sealTweenYTarget, sealTweenDuration).SetEase(sealTweenEasing).SetLoops(-1, LoopType.Yoyo);
			}
			else
			{
				sealMoveTween.Play();
			}
		}
		else if (sealMoveTween != null)
		{
			sealMoveTween.Pause();
		}
	}

	private void InitCachedSprites()
	{
		if (string.IsNullOrEmpty(currentCityId))
		{
			return;
		}
		if (!cachedRedSealSpriteOn.ContainsKey(currentCityId))
		{
			cachedRedSealSpriteOn.Add(currentCityId, ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Apocalypse/Seals/" + $"Apocalypse_Seal_{currentCityId}_on"));
		}
		if (!cachedRedSealSpriteOff.ContainsKey(currentCityId))
		{
			cachedRedSealSpriteOff.Add(currentCityId, ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Apocalypse/Seals/" + $"Apocalypse_Seal_{currentCityId}_off"));
		}
		if (isCityViewSeal)
		{
			if (!cachedCityColorSealSpriteOn.ContainsKey(currentCityId))
			{
				cachedCityColorSealSpriteOn.Add(currentCityId, ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Apocalypse/Seals/" + $"Apocalypse_Seal_WorldMap_{currentCityId}_On"));
			}
			if (!cachedCityColorSealSpriteOff.ContainsKey(currentCityId))
			{
				cachedCityColorSealSpriteOff.Add(currentCityId, ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Apocalypse/Seals/" + $"Apocalypse_Seal_WorldMap_{currentCityId}_Off"));
			}
		}
	}

	private void RefreshSeal(bool resetSealPosition)
	{
		if (string.IsNullOrEmpty(currentCityId))
		{
			return;
		}
		if (resetSealPosition)
		{
			ResetSealPosition();
		}
		if (isCityViewSeal)
		{
			if (atLeastOneApoLevelCompleted)
			{
				sealIcon.sprite = cachedRedSealSpriteOn[currentCityId];
			}
			else
			{
				sealIcon.sprite = (mustAnimateSeal ? cachedCityColorSealSpriteOn[currentCityId] : cachedCityColorSealSpriteOff[currentCityId]);
			}
		}
		else
		{
			sealIcon.sprite = (mustAnimateSeal ? cachedRedSealSpriteOn[currentCityId] : cachedRedSealSpriteOff[currentCityId]);
		}
		CheckIfMustInitTween();
	}

	private void ResetSealPosition()
	{
		if (sealMoveTween != null)
		{
			sealMoveTween.Kill();
			sealMoveTween = null;
		}
		sealIconRectTransform.anchoredPosition = sealDefaultAnachoredPosition;
	}

	private void OnDestroy()
	{
		if (sealMoveTween != null)
		{
			sealMoveTween.Kill();
			sealMoveTween = null;
		}
	}
}
