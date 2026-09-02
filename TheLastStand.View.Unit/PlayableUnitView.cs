using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using PortraitAPI;
using PortraitAPI.Misc;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Controller.Unit;
using TheLastStand.Database.Unit;
using TheLastStand.Definition;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.DLC;
using TheLastStand.Manager.SDK;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Unit;
using TheLastStand.Serialization.Unit;
using TheLastStand.View.Camera;
using TheLastStand.View.HUD.UnitPortraitPanel;
using TheLastStand.View.Unit.UI;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheLastStand.View.Unit;

public class PlayableUnitView : UnitView, ISnapshotable
{
	[SerializeField]
	private SortingGroup frontSortingGroup;

	[SerializeField]
	private Material colorSwapMaterial;

	[SerializeField]
	private Material colorSwapPortraitMaterial;

	[SerializeField]
	private GameObject skillTargetingFeedbackParent;

	[SerializeField]
	private SpriteRenderer skillTargetingGroundRenderer;

	[SerializeField]
	private SpriteRenderer skillTargetingHaloRenderer;

	[SerializeField]
	private Color skillTargetingHoverColor = Color.green;

	[SerializeField]
	private Transform snapshotTarget;

	[SerializeField]
	private int deathSortingOrder = 10;

	[SerializeField]
	private ParticleSystem prepareDeathParticles;

	[SerializeField]
	private ParticleSystem deathParticles;

	[SerializeField]
	private GameObject frontShadow;

	[SerializeField]
	private GameObject executeCross;

	[SerializeField]
	private GameObject bloodPool;

	[SerializeField]
	private float delayBeforeBark = 1f;

	[SerializeField]
	private float delayAfterBark = 1f;

	[SerializeField]
	private float delayBeforeDeathSequence = 0.3f;

	[SerializeField]
	private float delayAfterDeathSequence = 0.5f;

	[SerializeField]
	private float cameraFocusHeightOffset = 1f;

	[SerializeField]
	private AudioSource deathStartAudioSource;

	[SerializeField]
	private AudioSource deathImpactAudioSource;

	[SerializeField]
	private float deathShakeDuration = 0.4f;

	[SerializeField]
	private Vector2 deathShakeStrength = new Vector2(0.3f, 0.3f);

	protected BodyPartView[] bodyPartViews;

	private Texture2D colorSwapTex;

	private bool snapshotActiveBackup;

	private Dictionary<GameObject, int> snapshotLayersBackup;

	private GameDefinition.E_Direction snapshotLookDirectionBackup = GameDefinition.E_Direction.None;

	private WaitUntil waitUntilAnimatorStateIsPrepareDie;

	public static Dictionary<Sprite, int> UsedUnitPortrait = new Dictionary<Sprite, int>();

	public static Dictionary<string, List<string>> FaceIdAvailablePortraitIds = new Dictionary<string, List<string>>();

	public static Dictionary<DataColor, int> UsedUnitPortraitColors { get; private set; } = new Dictionary<DataColor, int>();

	public Material ColorSwapPortraitMaterial => colorSwapPortraitMaterial;

	public bool DeathSequenceOver { get; private set; }

	public override float MoveSpeed => GameManager.MoveSpeedMultiplier * PlayableUnitDatabase.UnitMoveSpeed;

	public PlayableUnit PlayableUnit { get; private set; }

	public PlayableUnitHUD PlayableUnitHUD => base.UnitHUD as PlayableUnitHUD;

	public UnitPortraitPanel PortraitPanel { get; private set; }

	public override TheLastStand.Model.Unit.Unit Unit
	{
		get
		{
			return base.Unit;
		}
		set
		{
			base.Unit = value;
			if (PlayableUnit != null)
			{
				InitBodyPartViews();
				PlayableUnit.RegisterBodyPartViews(bodyPartViews, register: false);
			}
			PlayableUnit = value as PlayableUnit;
			if (PlayableUnit != null)
			{
				InitBodyPartViews();
				PlayableUnit.RegisterBodyPartViews(bodyPartViews, register: true);
			}
		}
	}

	public Transform SnapshotPosition => snapshotTarget;

	public static void AddUsedPortrait(Sprite portraitUsed)
	{
		if (!UsedUnitPortrait.ContainsKey(portraitUsed))
		{
			UsedUnitPortrait.Add(portraitUsed, 1);
		}
		else
		{
			UsedUnitPortrait[portraitUsed]++;
		}
	}

	public static void GeneratePortrait(PlayableUnit playableUnit, SerializedPlayableUnit playableUnitElement)
	{
		PortraitGenerationResult portraitGenerationResult;
		CodeGenerator.CodeData codeData;
		if (string.IsNullOrEmpty(playableUnitElement.Portrait.Code))
		{
			portraitGenerationResult = PortraitAPIManager.GeneratePortrait(playableUnit.Gender, playableUnit.FaceId, RandomManager.GetRandomForCaller(TPSingleton<PlayableUnitManager>.Instance.GetType().Name), playableUnit.RaceDefinition.Id, TPSingleton<DLCManager>.Instance.OwnedDLCIds);
			playableUnit.PlayableUnitController.RandomizeColors(ref portraitGenerationResult.Code);
		}
		else if (!CodeGenerator.TryDecode(playableUnitElement.Portrait.Code, out codeData))
		{
			CLoggerManager.Log("We are trying to decode an invalid portrait code ! (Code : " + playableUnitElement.Portrait.Code + ")", LogType.Error);
			portraitGenerationResult = PortraitAPIManager.GeneratePortrait(playableUnit.Gender, playableUnit.FaceId, RandomManager.GetRandomForCaller(TPSingleton<PlayableUnitManager>.Instance.GetType().Name), playableUnit.RaceDefinition.Id, TPSingleton<DLCManager>.Instance.OwnedDLCIds);
			playableUnit.PlayableUnitController.RandomizeColors(ref portraitGenerationResult.Code);
		}
		else
		{
			portraitGenerationResult = PortraitAPIManager.GeneratePortrait(codeData);
			if (playableUnitElement.Portrait.Code.Length == 12)
			{
				ColorSwapPaletteDefinition value;
				if (playableUnitElement.Portrait.SkinColorPaletteId != string.Empty)
				{
					PlayableUnitDatabase.PlayableUnitSkinColorDefinitions.TryGetValue(playableUnitElement.Portrait.SkinColorPaletteId, out value);
				}
				else
				{
					value = PlayableUnitController.RandomizeColorSwapPaletteDefinition(PlayableUnitDatabase.PlayableUnitSkinColorDefinitions, out var _);
				}
				ColorSwapPaletteDefinition value2;
				if (playableUnitElement.Portrait.HairColorPaletteId != string.Empty)
				{
					PlayableUnitDatabase.PlayableUnitHairColorDefinitions.TryGetValue(playableUnitElement.Portrait.HairColorPaletteId, out value2);
				}
				else
				{
					value2 = PlayableUnitController.GetRandomHairColorSwapPalette(value.Id, PlayableUnitDatabase.PlayableUnitHairColorDefinitions);
				}
				ColorSwapPaletteDefinition value3;
				if (playableUnitElement.Portrait.EyesColorPaletteId != string.Empty)
				{
					PlayableUnitDatabase.PlayableUnitEyesColorDefinitions.TryGetValue(playableUnitElement.Portrait.EyesColorPaletteId, out value3);
				}
				else
				{
					value3 = PlayableUnitController.RandomizeColorSwapPaletteDefinition(PlayableUnitDatabase.PlayableUnitEyesColorDefinitions, out var _);
				}
				DataColor dataColor = PlayableUnitDatabase.PortraitBackgroundColors.Find((DataColor x) => x._HexCode == playableUnitElement.Portrait.BackgroundColor);
				if (dataColor != null)
				{
					if (!UsedUnitPortraitColors.ContainsKey(dataColor))
					{
						UsedUnitPortraitColors.Add(dataColor, 0);
					}
					UsedUnitPortraitColors[dataColor]++;
				}
				else
				{
					dataColor = GetRandomPortraitBGColor(playableUnit);
				}
				CodeGenerator.EncodeColorDatas(new KeyValuePair<Commons.E_ColorTypes, int>[4]
				{
					new KeyValuePair<Commons.E_ColorTypes, int>(Commons.E_ColorTypes.Skin, PlayableUnitDatabase.PlayableUnitSkinColorDefinitions.IndexOf(value.Id)),
					new KeyValuePair<Commons.E_ColorTypes, int>(Commons.E_ColorTypes.Hair, PlayableUnitDatabase.PlayableUnitHairColorDefinitions.IndexOf(value2.Id)),
					new KeyValuePair<Commons.E_ColorTypes, int>(Commons.E_ColorTypes.Eyes, PlayableUnitDatabase.PlayableUnitEyesColorDefinitions.IndexOf(value3.Id)),
					new KeyValuePair<Commons.E_ColorTypes, int>(Commons.E_ColorTypes.Background, PlayableUnitDatabase.PortraitBackgroundColors.IndexOf(dataColor))
				}, ref codeData);
				TPSingleton<PlayableUnitManager>.Instance.Log("The code used for the portrait of " + playableUnit.PlayableUnitName + " come from an older version, this code will be converted." + $"Previous code : {playableUnitElement.Portrait.Code} => New code : {portraitGenerationResult.Code}", CLogLevel.DETAILED);
			}
			else
			{
				string hexCode = PlayableUnitDatabase.PortraitBackgroundColors[codeData.CodeColorDatas[Commons.E_ColorTypes.Background].Index]._HexCode;
				for (int num = PlayableUnitDatabase.PortraitBackgroundColors.Count - 1; num >= 0; num--)
				{
					if (PlayableUnitDatabase.PortraitBackgroundColors[num]._HexCode == hexCode)
					{
						DataColor key = PlayableUnitDatabase.PortraitBackgroundColors[num];
						if (!UsedUnitPortraitColors.ContainsKey(key))
						{
							UsedUnitPortraitColors.Add(key, 0);
						}
						UsedUnitPortraitColors[key]++;
						break;
					}
				}
			}
		}
		AddUsedPortrait(portraitGenerationResult.Front);
		playableUnit.PortraitCodeData = portraitGenerationResult.Code;
		playableUnit.PortraitSprite = portraitGenerationResult.Front;
		playableUnit.PortraitBackgroundSprite = portraitGenerationResult.Back;
	}

	public static void GenerateRandomPortrait(PlayableUnit playableUnit)
	{
		if (string.IsNullOrEmpty(playableUnit.Gender) || string.IsNullOrEmpty(playableUnit.FaceId))
		{
			TPDebug.LogError("Trying to GetPortraitForUnit but Gender or FaceId is not setup --> aborting");
			return;
		}
		PortraitGenerationResult portraitGenerationResult = PortraitAPIManager.GeneratePortrait(playableUnit.Gender, playableUnit.FaceId, RandomManager.GetRandomForCaller(TPSingleton<PlayableUnitManager>.Instance.GetType().Name), playableUnit.RaceDefinition.Id, TPSingleton<DLCManager>.Instance.OwnedDLCIds);
		playableUnit.PlayableUnitController.RandomizeColors(ref portraitGenerationResult.Code);
		AddUsedPortrait(portraitGenerationResult.Front);
		playableUnit.PortraitSprite = portraitGenerationResult.Front;
		playableUnit.PortraitBackgroundSprite = portraitGenerationResult.Back;
		playableUnit.PortraitCodeData = portraitGenerationResult.Code;
		TPSingleton<PlayableUnitManager>.Instance.Log($"Portrait generated with code : {portraitGenerationResult.Code} !");
	}

	public static Sprite GetBodyPartSprite(PlayableUnit playableUnit, string bodyPartId)
	{
		if (playableUnit == null)
		{
			TPDebug.LogError("PlayableUnitView.GetBodyPartSprite() => playableUnit can't be null! Aborting...");
			return null;
		}
		return BodyPartView.GetSprite(playableUnit.BodyParts[bodyPartId], playableUnit.FaceId, playableUnit.Gender, BodyPartDefinition.E_Orientation.Front);
	}

	public static void RemoveUsedPortrait(Sprite portraitUsed)
	{
		UsedUnitPortrait[portraitUsed]--;
		if (UsedUnitPortrait[portraitUsed] == 0)
		{
			UsedUnitPortrait.Remove(portraitUsed);
		}
	}

	public static void RemoveUsedPortraitColor(DataColor colorUsed)
	{
		UsedUnitPortraitColors[colorUsed]--;
		if (UsedUnitPortraitColors[colorUsed] == 0)
		{
			UsedUnitPortraitColors.Remove(colorUsed);
		}
	}

	public static DataColor GetRandomPortraitBGColor(PlayableUnit playableUnit)
	{
		List<DataColor> list = new List<DataColor>(PlayableUnitDatabase.PortraitBackgroundColors);
		if (UsedUnitPortraitColors.Count < list.Count)
		{
			List<DataColor> usedColors = new List<DataColor>(UsedUnitPortraitColors.Keys);
			list.RemoveAll((DataColor color) => usedColors.Contains(color));
		}
		DataColor randomElement = RandomManager.GetRandomElement(TPSingleton<PlayableUnitManager>.Instance, list);
		if (!UsedUnitPortraitColors.ContainsKey(randomElement))
		{
			UsedUnitPortraitColors.Add(randomElement, 0);
		}
		UsedUnitPortraitColors[randomElement]++;
		return randomElement;
	}

	public void EnableBloodPool()
	{
		bloodPool.SetActive(value: true);
	}

	public Sprite GetBodyPartSprite(string bodyPartId, BodyPartDefinition.E_Orientation orientation)
	{
		return PlayableUnit.BodyParts[bodyPartId].GetBodyPartView(orientation).GetSprite();
	}

	public void Init(PlayableUnit playableUnit)
	{
		base.name = playableUnit.Name;
		Unit = playableUnit;
		InitVisuals(playSpawnAnim: true);
		UpdatePosition();
		RefreshHudPositionInstantly();
		PortraitPanel = GameView.TopScreenPanel.UnitPortraitsPanel.AddPortrait(PlayableUnit);
	}

	public void InitDeadUnit(PlayableUnit playableUnit)
	{
		base.name = playableUnit.Name;
		Unit = playableUnit;
		InitVisuals(playSpawnAnim: false);
		UpdatePosition();
		RefreshHudPositionInstantly();
	}

	public override void InitVisuals(bool playSpawnAnim)
	{
		base.InitVisuals(playSpawnAnim);
		InitColorSwapTexture();
		SwapColors();
		RefreshBodyParts();
	}

	public void OnSkillTargetHover(bool hover, bool avoidPortrait = false)
	{
		skillTargetingGroundRenderer.color = (hover ? skillTargetingHoverColor : Color.white);
		skillTargetingHaloRenderer.color = (hover ? skillTargetingHoverColor : Color.white);
		base.OnSkillTargetHover(hover);
		if (avoidPortrait)
		{
			if (hover)
			{
				GameView.TopScreenPanel.UnitPortraitsPanel.ToggleSelectedUnit(PlayableUnit);
			}
			else
			{
				GameView.TopScreenPanel.UnitPortraitsPanel.ToggleUnselectedUnit(PlayableUnit);
			}
		}
	}

	public virtual void OnSnapshotFinished()
	{
		foreach (KeyValuePair<GameObject, int> item in snapshotLayersBackup)
		{
			item.Key.layer = item.Value;
		}
		LookAtDirection(snapshotLookDirectionBackup);
		base.gameObject.SetActive(snapshotActiveBackup);
	}

	public virtual void PrepareForSnapshot()
	{
		if (snapshotLayersBackup == null)
		{
			snapshotLayersBackup = new Dictionary<GameObject, int>();
		}
		else
		{
			snapshotLayersBackup.Clear();
		}
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>(includeInactive: true);
		foreach (Renderer renderer in componentsInChildren)
		{
			snapshotLayersBackup.Add(renderer.gameObject, renderer.gameObject.layer);
			renderer.gameObject.layer = CameraSnapshot.Constants.SnaptshotLayer;
		}
		snapshotLookDirectionBackup = Unit.LookDirection;
		LookAtDirection(GameDefinition.E_Direction.South);
		snapshotActiveBackup = base.gameObject.activeSelf;
		base.gameObject.SetActive(value: true);
	}

	public void RefreshBodyParts(bool forceFullRefresh = false)
	{
		if (PlayableUnit != null)
		{
			int i = 0;
			for (int num = bodyPartViews.Length; i < num; i++)
			{
				bodyPartViews[i].Refresh(PlayableUnit.FaceId, PlayableUnit.Gender, forceFullRefresh);
			}
			RefreshSnapshot();
		}
	}

	public override void RefreshCursorFeedback()
	{
	}

	public override void RefreshHud(UnitStatDefinition.E_Stat stat)
	{
		base.RefreshHud(stat);
		if (stat == UnitStatDefinition.E_Stat.ManaTotal || stat == UnitStatDefinition.E_Stat.Mana)
		{
			RefreshMana();
		}
	}

	[ContextMenu("Refresh Mana")]
	public void RefreshMana()
	{
		(base.UnitHUD as PlayableUnitHUD).RefreshMana();
	}

	[ContextMenu("Refresh Snapshot")]
	public void RefreshSnapshot()
	{
		Unit.UiSprite = TPSingleton<CameraSnapshot>.Instance.TakeSnapshot(this);
	}

	public override void SetFrontAndBackActive(bool active)
	{
		if (!PlayableUnit.IsDead)
		{
			base.SetFrontAndBackActive(active);
		}
	}

	public void ToggleHelmetDisplay()
	{
		if (PlayableUnit.EquipmentSlots.TryGetValue(ItemSlotDefinition.E_ItemSlotId.Head, out var value) && value != null && value.Count > 0 && value[0].Item != null)
		{
			BodyPart bodyPart = PlayableUnit.BodyParts["Head"];
			if (PlayableUnit.HelmetDisplayed)
			{
				PlayableUnit.PlayableUnitController.OverrideBodyParts(value[0].Item.ItemDefinition.BodyPartsDefinitions);
			}
			else
			{
				bodyPart.BodyPartDefinitionOverride = null;
			}
			BodyPartView bodyPartView;
			if ((bodyPartView = bodyPart.GetBodyPartView(BodyPartDefinition.E_Orientation.Front)) != null)
			{
				bodyPartView.IsDirty = true;
				bodyPartView.Refresh(PlayableUnit.FaceId, PlayableUnit.Gender);
			}
			if ((bodyPartView = bodyPart.GetBodyPartView(BodyPartDefinition.E_Orientation.Back)) != null)
			{
				bodyPartView.IsDirty = true;
				bodyPartView.Refresh(PlayableUnit.FaceId, PlayableUnit.Gender);
			}
			RefreshSnapshot();
		}
	}

	public override void ToggleSkillTargeting(bool show)
	{
		if (show && TPSingleton<GameManager>.Instance.Game.Cursor.Tile != PlayableUnit.OriginTile)
		{
			OnSkillTargetHover(hover: false);
		}
		skillTargetingFeedbackParent.SetActive(show);
		PortraitPanel.ToggleSkillTargeting(show);
		base.ToggleSkillTargeting(show);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		colorSwapMaterial = new Material(colorSwapMaterial);
		colorSwapMaterial.name += " (Copy)";
		colorSwapPortraitMaterial = new Material(colorSwapPortraitMaterial);
		colorSwapPortraitMaterial.name += " (Copy)";
		InitBodyPartViews();
	}

	protected override bool InitAnimations()
	{
		bool flag = false;
		if (PlayableUnit != null && PlayableUnit.RaceDefinition != null)
		{
			if (PlayableUnit.RaceDefinition.OverrideUnitAnimator)
			{
				animator.runtimeAnimatorController = ResourcePooler.LoadOnce<RuntimeAnimatorController>("Animators/Units/PlayableUnits/" + PlayableUnit.RaceDefinition.AnimatorName);
			}
			else
			{
				animator.runtimeAnimatorController = PlayableUnitDatabase.RaceDefaultAnimatorController;
			}
			flag = true;
		}
		if (!base.InitAnimations())
		{
			return false;
		}
		if (flag)
		{
			animator.Update(0f);
		}
		if (waitUntilAnimatorStateIsPrepareDie == null)
		{
			waitUntilAnimatorStateIsPrepareDie = new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Constants.Animation.AnimatorPrepareDeathStateHash);
		}
		return true;
	}

	[ContextMenu("Init Swap Texture")]
	protected void InitColorSwapTexture()
	{
		if (colorSwapTex == null)
		{
			colorSwapTex = new Texture2D(100, 1, TextureFormat.RGBA32, mipChain: false, linear: false)
			{
				filterMode = FilterMode.Point
			};
		}
		for (int i = 0; i < colorSwapTex.width; i++)
		{
			colorSwapTex.SetPixel(i, 0, new Color(0f, 0f, 0f, 0f));
		}
		colorSwapTex.Apply();
		colorSwapMaterial.SetTexture("_SwapTex", colorSwapTex);
		colorSwapPortraitMaterial.SetTexture("_SwapTex", colorSwapTex);
	}

	protected override IEnumerator PlayDieAnimCoroutine()
	{
		deathStartAudioSource.Play();
		yield return new WaitForSeconds(delayBeforeDeathSequence);
		if (EnemyUnitDatabase.HitByEnemySoundDefinitions.TryGetValue("PlayableUnits", out var value))
		{
			string soundId = value.GetSoundId(TPSingleton<EnemyUnitManager>.Instance.TotalCasters);
			ObjectPooler.GetPooledComponent("HitsSFX", TPSingleton<PlayableUnitManager>.Instance.HitSFXPrefab).Play(ResourcePooler.LoadOnce<AudioClip>("Sounds/SFX/PlayableUnitHits/" + soundId));
		}
		else
		{
			TPSingleton<PlayableUnitManager>.Instance.LogWarning("No sound found for PlayableUnits on hit");
		}
		bool previousAllowUserPan = ACameraView.AllowUserPan;
		ACameraView.MoveTo(base.transform.position + new Vector3(0f, cameraFocusHeightOffset));
		ACameraView.AllowUserPan = false;
		SetOrientation(front: true);
		animator.SetTrigger("PrepareDie");
		prepareDeathParticles.Play();
		yield return waitUntilAnimatorStateIsPrepareDie;
		yield return new WaitForSeconds(delayBeforeBark);
		base.UnitHUD.gameObject.SetActive(value: false);
		TPSingleton<BarkManager>.Instance.AddPotentialBark("PlayableUnitSelfDeath", PlayableUnit, 0f, -1, forceSucceed: false, ignoreDeathCheck: true);
		TPSingleton<BarkManager>.Instance.Display();
		yield return SharedYields.WaitForSeconds(0.1f);
		yield return new WaitUntil(() => BarkManager.DisplayedBarksCount == 0);
		yield return new WaitForSeconds(delayAfterBark);
		deathImpactAudioSource.Play();
		executeCross.gameObject.SetActive(value: true);
		yield return SharedYields.WaitForSeconds(0.05f);
		prepareDeathParticles.Stop();
		ACameraView.Shake(deathShakeDuration, deathShakeStrength, 10, 0.1f, 0f, Vector3.zero, 0f);
		deathParticles.Play();
		frontShadow.SetActive(value: false);
		frontSortingGroup.sortingOrder = deathSortingOrder;
		animator.SetTrigger("Die");
		TPSingleton<LightningSDKManager>.Instance.TriggerFlashEffect(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count > 1);
		yield return CameraView.CameraVisualEffects.InvertImageCoroutine(0.05f, Ease.Linear);
		CameraView.CameraVisualEffects.StartCoroutine(CameraView.CameraVisualEffects.ResetInvertCoroutine(0.05f, Ease.Linear));
		CameraView.CameraVisualEffects.StartCoroutine(CameraView.CameraVisualEffects.RedFlashCoroutine());
		yield return waitUntilAnimatorStateIsDie;
		yield return waitUntilAnimatorStateIsDead;
		ACameraView.AllowUserPan = previousAllowUserPan;
		yield return SharedYields.WaitForSeconds(delayAfterDeathSequence);
		DeathSequenceOver = true;
	}

	private void InitBodyPartViews()
	{
		if (bodyPartViews == null)
		{
			bodyPartViews = base.OrientationRootTransform.GetComponentsInChildren<BodyPartView>(includeInactive: true);
			int i = 0;
			for (int num = bodyPartViews.Length; i < num; i++)
			{
				bodyPartViews[i].Init();
				bodyPartViews[i].SetMaterial(colorSwapMaterial);
			}
		}
	}

	private void SwapColor(int index, Color color)
	{
		colorSwapTex.SetPixel(index, 0, color);
	}

	private void SwapColors()
	{
		SwapColorsForPalette(PlayableUnit.HairColorPalette);
		SwapColorsForPalette(PlayableUnit.SkinColorPalette);
		SwapColorsForPalette(PlayableUnit.EyesColorPalette);
		colorSwapTex.Apply();
	}

	private void SwapColorsForPalette(ColorSwapPaletteDefinition paletteDefinition)
	{
		if (paletteDefinition != null)
		{
			int i = 0;
			for (int count = paletteDefinition.ColorSwapDefinitions.Count; i < count; i++)
			{
				SwapColor(paletteDefinition.ColorSwapDefinitions[i].Index, paletteDefinition.ColorSwapDefinitions[i].OutputColor);
			}
		}
	}

	public void DebugChangeAnimatorController(RuntimeAnimatorController newAnimatorController)
	{
		InitAnimatorController(newAnimatorController);
	}

	public string DebugGetCurrentAnimatorControllerName()
	{
		return animator.runtimeAnimatorController.name;
	}

	[ContextMenu("Refresh ColorSwap")]
	public void DebugRefreshColorSwapping()
	{
		InitColorSwapTexture();
		SwapColors();
		RefreshSnapshot();
	}

	[ContextMenu("Refresh BodyParts")]
	private void DebugForceRefreshBodyParts()
	{
		RefreshBodyParts(forceFullRefresh: true);
	}

	[ContextMenu("Debug Lifetime Stats")]
	private void DebugLifetimeStats()
	{
		TPSingleton<PlayableUnitManager>.Instance.Log(PlayableUnit.LifetimeStats.LifetimeStatsController.DebugGetLifetimeStatsLog(PlayableUnit.PlayableUnitName));
	}
}
