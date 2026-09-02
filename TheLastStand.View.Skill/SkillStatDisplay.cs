using TMPro;
using TPLib;
using TheLastStand.Definition.Unit;
using TheLastStand.Model.Skill;
using TheLastStand.View.Unit.Stat;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Skill;

public class SkillStatDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI statValueText;

	[SerializeField]
	private bool showName;

	[SerializeField]
	private TextMeshProUGUI[] textsToColor;

	[SerializeField]
	private bool useStatColor;

	[SerializeField]
	private DataColorDictionary statColors;

	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private UnitStatDisplay.E_IconSize iconSize = UnitStatDisplay.E_IconSize.Small;

	protected Color[] textsOriginalColor;

	private Color? colorOverride;

	private bool fullRefreshNeeded;

	private UnitStatDefinition statDefinition;

	public Color? ColorOverride
	{
		get
		{
			return colorOverride;
		}
		set
		{
			if (!(colorOverride == value))
			{
				colorOverride = value;
				RefreshColor();
			}
		}
	}

	public TheLastStand.Model.Skill.Skill Skill { get; set; }

	public UnitStatDefinition StatDefinition
	{
		get
		{
			return statDefinition;
		}
		set
		{
			if (statDefinition != value)
			{
				statDefinition = value;
				fullRefreshNeeded = true;
			}
		}
	}

	public void Display(bool display)
	{
		base.gameObject.SetActive(display);
	}

	public void Refresh(bool forceFullRefresh = false)
	{
		if (StatDefinition != null && Skill != null)
		{
			fullRefreshNeeded |= forceFullRefresh;
			RefreshValue();
			if (fullRefreshNeeded)
			{
				RefreshIcon();
				RefreshColor();
				fullRefreshNeeded = false;
			}
		}
	}

	private void Awake()
	{
		BackupOriginalColors();
	}

	private void BackupOriginalColors()
	{
		if (textsToColor == null || textsToColor.Length == 0)
		{
			return;
		}
		textsOriginalColor = new Color[textsToColor.Length];
		int i = 0;
		for (int num = textsToColor.Length; i < num; i++)
		{
			if (textsToColor[i] != null)
			{
				textsOriginalColor[i] = textsToColor[i].color;
			}
		}
	}

	private void RefreshColor()
	{
		if (textsToColor == null || textsToColor.Length == 0)
		{
			return;
		}
		Color? color = null;
		if (ColorOverride.HasValue)
		{
			color = ColorOverride.Value;
		}
		else if (useStatColor)
		{
			color = statColors.GetColorById(StatDefinition.Id.ToString());
		}
		int i = 0;
		for (int num = textsToColor.Length; i < num; i++)
		{
			if (textsToColor[i] != null)
			{
				textsToColor[i].color = color ?? textsOriginalColor[i];
			}
		}
	}

	private void RefreshIcon()
	{
		if (!(iconImage == null))
		{
			iconImage.sprite = UnitStatDisplay.GetStatIconSprite(StatDefinition.Id, iconSize);
			iconImage.enabled = iconImage.sprite != null;
		}
	}

	private void RefreshValue()
	{
		if (!(statValueText == null))
		{
			string text = string.Empty;
			string text2 = string.Empty;
			if (showName)
			{
				text2 = StatDefinition.Name + ": ";
			}
			switch (StatDefinition.Id)
			{
			case UnitStatDefinition.E_Stat.ActionPoints:
				text = Skill.SkillAction.SkillActionController.ComputeActionPointsCost(Skill.OwnerOrSelected).ToString();
				break;
			case UnitStatDefinition.E_Stat.MovePoints:
				text = Skill.SkillAction.SkillActionController.ComputeMovePointsCost(Skill.OwnerOrSelected).ToString();
				break;
			case UnitStatDefinition.E_Stat.Mana:
				text = Skill.SkillAction.SkillActionController.ComputeManaCost(Skill.OwnerOrSelected).ToString();
				break;
			case UnitStatDefinition.E_Stat.Health:
				text = Skill.SkillAction.SkillActionController.ComputeHealthCost(Skill.OwnerOrSelected).ToString();
				break;
			}
			statValueText.text = text2 + text;
		}
	}

	private void Start()
	{
		Refresh(forceFullRefresh: true);
	}

	[ContextMenu("Force Full Refresh")]
	private void DBG_ForceFullRefresh()
	{
		BackupOriginalColors();
		Refresh(forceFullRefresh: true);
	}
}
