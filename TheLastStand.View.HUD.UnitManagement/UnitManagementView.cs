using TMPro;
using TPLib;
using TPLib.Localization.Fonts;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using TheLastStand.View.Skill.UI;
using TheLastStand.View.Unit.Injury;
using TheLastStand.View.Unit.Stat;
using UnityEngine;

namespace TheLastStand.View.HUD.UnitManagement;

public abstract class UnitManagementView<T> : TPSingleton<T> where T : UnitManagementView<T>
{
	[SerializeField]
	protected TextMeshProUGUI unitName;

	[SerializeField]
	protected UnitStatDisplay armorDisplay;

	[SerializeField]
	protected UnitStatDisplay movePointsDisplay;

	[SerializeField]
	protected UnitStatDisplay blockDisplay;

	[SerializeField]
	protected UnitInjuriesDisplay injuriesDisplay;

	[SerializeField]
	protected ModifiersLayoutView modifiersLayoutView;

	[SerializeField]
	protected GameObject armorDisplayParent;

	[SerializeField]
	protected SkillBar skillBar;

	[SerializeField]
	protected SimpleFontLocalizedParent simpleFontLocalizedParent;

	[SerializeField]
	private HUDJoystickSimpleTarget joystickTarget;

	protected Canvas canvas;

	protected bool initialized;

	public HUDJoystickSimpleTarget JoystickTarget => joystickTarget;

	public bool Displayed
	{
		get
		{
			if (canvas != null)
			{
				return canvas.enabled;
			}
			return false;
		}
	}

	public SkillBar SkillBar => skillBar;

	public static void Refresh()
	{
		if (!TPSingleton<T>.Instance.initialized)
		{
			TPSingleton<T>.Instance.Init();
		}
		TPSingleton<T>.Instance.RefreshView();
	}

	public virtual void Init()
	{
		initialized = true;
		canvas = TPSingleton<T>.Instance.GetComponent<Canvas>();
		armorDisplay.Init();
		blockDisplay.Init();
		movePointsDisplay.Init();
		armorDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Armor];
		armorDisplay.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ArmorTotal];
		blockDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Block];
		movePointsDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.MovePoints];
		movePointsDisplay.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.MovePointsTotal];
	}

	protected override void Awake()
	{
		base.Awake();
		Init();
	}

	protected static void DisplayCanvas(bool display)
	{
		TPSingleton<T>.Instance.canvas.enabled = display && UIManager.DebugToggleUI != false;
		if (!display && TileObjectSelectionManager.HasUnitSelected)
		{
			TPSingleton<T>.Instance.skillBar.JoystickSkillBar.DeselectCurrentSkill();
		}
	}

	protected virtual void RefreshView()
	{
		DisplayCanvas(display: true);
		simpleFontLocalizedParent?.RegisterChildren();
		if (TileObjectSelectionManager.SelectedUnit != null && TileObjectSelectionManager.SelectedUnit.GetClampedStatValue(armorDisplay.SecondaryStatDefinition.Id) != 0f)
		{
			armorDisplay.Refresh();
			if (!armorDisplayParent.activeSelf)
			{
				armorDisplayParent.SetActive(value: true);
			}
		}
		else
		{
			armorDisplayParent.SetActive(value: false);
		}
		movePointsDisplay.Refresh();
		blockDisplay.Refresh();
		modifiersLayoutView.Refresh();
	}
}
