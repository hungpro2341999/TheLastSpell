using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.Apocalypse;
using TheLastStand.Framework;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.WorldMap;
using TheLastStand.View.HUD;
using TheLastStand.View.WorldMap;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.Apocalypse;

public class ApocalypseView : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
	public static class Constants
	{
		public const string ApocalypseDifficultyNormal = "WorldMap_ApocalypseDifficulty_Normal";

		public const string ApocalypseDifficultyApocalypse = "WorldMap_ApocalypseDifficulty_Apocalypse";

		public const string ApocalypseDescriptionUnavailable = "WorldMap_ApocalypseDescription_Unavailable";

		public const string ApocalypseDescriptionPrefix = "WorldMap_ApocalypseDescription_";

		public const string ApocalypseDamnedSoulsModifierFormat = "WorldMap_ApocalypseDamnedSoulsModifier";

		public const string ApocalypseLevelImagePrefixPath = "View/Sprites/UI/WorldMap/ApocalypseLevels/ApocalypseLevel_";

		public const string FlameAnimationIdle = "WorldMapFlamesIdle";

		public const string FlameAnimationUnsuccess = "WorldMapFlamesUnsuccess";

		public const string FlameAnimationDisabled = "WorldMapFlamesDisabled";

		public const string ApocalypseLevelFormat = "<style=Bad>{0}</style>";
	}

	public bool AlreadySuccessful;

	public bool Available;

	public ApocalypseDefinition ApocalypseDefinition;

	[SerializeField]
	private Image apocalypseIndexImage;

	[SerializeField]
	private TextMeshProUGUI apocalypseTitle;

	[SerializeField]
	private TextMeshProUGUI description;

	[SerializeField]
	private Animator flameAnimator;

	[SerializeField]
	private BetterToggleGauge toggleGauge;

	[SerializeField]
	private GameObject damnedSoulsModifierPanel;

	[SerializeField]
	private JoystickSelectable joystickSelectable;

	public BetterToggleGauge.E_BetterToggleGaugeState State
	{
		get
		{
			if (!(toggleGauge != null))
			{
				return BetterToggleGauge.E_BetterToggleGaugeState.Disabled;
			}
			return toggleGauge.State;
		}
	}

	public JoystickSelectable JoystickSelectable => joystickSelectable;

	public void ContinueAnimations()
	{
		flameAnimator.speed = 1f;
		toggleGauge.Animator.speed = 1f;
	}

	public void Init(BetterToggleGaugeGroup group)
	{
		Available = TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable >= ApocalypseDefinition.Id;
		toggleGauge.Init(ApocalypseDefinition.Id, group);
		if (ApocalypseDefinition.Id != 0)
		{
			apocalypseIndexImage.sprite = ResourcePooler<Sprite>.LoadOnce("View/Sprites/UI/WorldMap/ApocalypseLevels/ApocalypseLevel_" + ApocalypseDefinition.Id.ToString("00"));
			apocalypseTitle.text = Localizer.Get("WorldMap_ApocalypseDifficulty_Apocalypse");
		}
		else
		{
			apocalypseIndexImage.gameObject.SetActive(value: false);
			apocalypseTitle.text = Localizer.Get("WorldMap_ApocalypseDifficulty_Normal");
		}
		damnedSoulsModifierPanel.SetActive(value: false);
		InitApocalypse();
	}

	public void InitApocalypse()
	{
		ChangeFlame();
		if (!Available)
		{
			toggleGauge.SetState(BetterToggleGauge.E_BetterToggleGaugeState.Disabled);
			description.text = Localizer.Get("WorldMap_ApocalypseDescription_Unavailable");
			apocalypseIndexImage.gameObject.SetActive(value: false);
			apocalypseTitle.text = string.Empty;
			return;
		}
		if (ApocalypseDefinition.Id != 0)
		{
			toggleGauge.SetState(BetterToggleGauge.E_BetterToggleGaugeState.Normal);
			apocalypseIndexImage.gameObject.SetActive(value: true);
			apocalypseTitle.text = Localizer.Get("WorldMap_ApocalypseDifficulty_Apocalypse");
		}
		description.text = Localizer.Get("WorldMap_ApocalypseDescription_" + ApocalypseDefinition.Id.ToString("00"));
	}

	public void OnSelect(BaseEventData eventData)
	{
		TPSingleton<GameConfigurationsView>.Instance.AdjustScrollView(base.transform as RectTransform);
	}

	public void PauseAnimations()
	{
		flameAnimator.speed = 0f;
		toggleGauge.Animator.speed = 0f;
	}

	public void Refresh()
	{
		Available = TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable >= ApocalypseDefinition.Id;
		if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity != null && TPSingleton<WorldMapCityManager>.Instance.SelectedCity.MaxApocalypsePassed >= ApocalypseDefinition.Id)
		{
			apocalypseTitle.color = new Color(apocalypseTitle.color.r, apocalypseTitle.color.g, apocalypseTitle.color.b, 1f);
			apocalypseIndexImage.color = new Color(apocalypseIndexImage.color.r, apocalypseIndexImage.color.g, apocalypseIndexImage.color.b, 1f);
		}
		else
		{
			apocalypseTitle.color = new Color(apocalypseTitle.color.r, apocalypseTitle.color.g, apocalypseTitle.color.b, 0.25f);
			apocalypseIndexImage.color = new Color(apocalypseIndexImage.color.r, apocalypseIndexImage.color.g, apocalypseIndexImage.color.b, 0.25f);
		}
		AlreadySuccessful = TPSingleton<WorldMapCityManager>.Instance.SelectedCity != null && ApocalypseDefinition.Id <= TPSingleton<WorldMapCityManager>.Instance.SelectedCity.MaxApocalypsePassed;
		InitApocalypse();
	}

	private void ChangeFlame()
	{
		if (!Available)
		{
			flameAnimator.Play("WorldMapFlamesDisabled", 0, UnityEngine.Random.value);
		}
		else
		{
			flameAnimator.Play(AlreadySuccessful ? "WorldMapFlamesIdle" : "WorldMapFlamesUnsuccess", 0, UnityEngine.Random.value);
		}
	}

	private void Start()
	{
		toggleGauge.OnStateHasChanged.AddListener(OnToggleClicked);
	}

	private void OnToggleClicked(BetterToggleGauge.E_BetterToggleGaugeState state)
	{
	}
}
