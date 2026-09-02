using System.Linq;
using TPLib;
using TheLastStand.Helpers;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Sound;
using TheLastStand.Model.Tutorial;
using TheLastStand.Model.WorldMap;
using TheLastStand.View.WorldMap;
using TheLastStand.View.WorldMap.ItemRestriction;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.Manager.WorldMap;

public class WorldMapUIManager : Manager<WorldMapUIManager>
{
	[SerializeField]
	private CanvasScaler canvasScaler;

	[SerializeField]
	private Button metaShopsButton;

	[SerializeField]
	private OpenWeaponRestrictionsView openWeaponRestrictionsView;

	[SerializeField]
	private WorldMapApocalypseMaxLevelView worldMapApocalypseMaxLevelView;

	[SerializeField]
	private Transform tooltipsContainer;

	private AudioSource audioSource;

	public static AudioSource AudioSource
	{
		get
		{
			if (TPSingleton<WorldMapUIManager>.Instance.audioSource == null)
			{
				TPSingleton<WorldMapUIManager>.Instance.audioSource = TPSingleton<WorldMapUIManager>.Instance.GetComponent<AudioSource>();
			}
			return TPSingleton<WorldMapUIManager>.Instance.audioSource;
		}
	}

	public static bool CanAccessMetaShops
	{
		get
		{
			if (!MetaShopsManager.OraculumForceAccess)
			{
				return TPSingleton<WorldMapCityManager>.Instance.Cities.Any((WorldMapCity city) => !city.CityDefinition.IsTutorialMap && city.RefreshIsVisible() && city.RefreshIsUnlocked());
			}
			return true;
		}
	}

	public static bool CanOpenWeaponRestrictionsPanel => TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.IsAvailable;

	public Transform TooltipsContainer => tooltipsContainer;

	public CanvasScaler CanvasScaler => canvasScaler;

	public static void OnReturnedFromOraculum()
	{
		TPSingleton<WorldMapUIManager>.Instance.openWeaponRestrictionsView.DisplayOrRefresh();
		TPSingleton<WorldMapUIManager>.Instance.worldMapApocalypseMaxLevelView.CheckIfMustToCreateCityViews();
		TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnWorldMapOpen);
	}

	public void PlayAudioClip(AudioClip audioClip)
	{
		SoundManager.PlayAudioClip(AudioSource, audioClip);
	}

	public void PlayAudioClipWithoutInterrupting(AudioClip audioClip)
	{
		SoundManager.PlayAudioClip(AudioSource, audioClip, 0f, doNotInterrupt: true);
	}

	private void OnMetaShopsButtonClicked()
	{
		if (MetaShopsManager.CanOpenShops())
		{
			TPSingleton<MetaShopsManager>.Instance.OpenShops();
		}
	}

	private void RefreshMetaShopsButtonAvailability()
	{
		metaShopsButton.gameObject.SetActive(CanAccessMetaShops);
	}

	private void Start()
	{
		metaShopsButton.onClick.AddListener(OnMetaShopsButtonClicked);
		RefreshMetaShopsButtonAvailability();
		CanvasHelper.ScaleCanvas(canvasScaler, allowDecimals: false);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		metaShopsButton.onClick.RemoveListener(OnMetaShopsButtonClicked);
	}
}
