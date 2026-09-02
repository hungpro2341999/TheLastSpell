using TMPro;
using TPLib;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Definition.Unit;
using TheLastStand.Model.Status;
using TheLastStand.Model.Status.Immunity;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.UI;

public class UnitStatusDisplay : MonoBehaviour
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image bg;

	[SerializeField]
	private DataSpriteTable bgSprites;

	[SerializeField]
	private DataSpriteDictionary skillEffectIcons;

	[SerializeField]
	private DataColorDictionary skillEffectColors;

	[SerializeField]
	protected TextMeshProUGUI titleText;

	[SerializeField]
	private GameObject turnPanel;

	[SerializeField]
	protected TextMeshProUGUI turnCount;

	[SerializeField]
	protected GameObject turnInfiniteIcon;

	private Status Status { get; set; }

	public virtual void Init(Status status, bool isSurrounding = false)
	{
		Status = status;
		icon.sprite = skillEffectIcons.GetSpriteById(Status.StatusType.ToString());
		RefreshTitle();
		turnPanel.SetActive(value: true);
		if ((float)Status.RemainingTurnsCount == -1f)
		{
			turnInfiniteIcon.SetActive(value: true);
			turnCount.gameObject.SetActive(value: false);
		}
		else
		{
			turnInfiniteIcon.SetActive(value: false);
			turnCount.gameObject.SetActive(value: true);
			turnCount.text = Status.RemainingTurnsCount.ToString();
		}
		bg.sprite = bgSprites.GetSpriteAt(1);
	}

	private string ColoredName(string name)
	{
		if (Status == null)
		{
			return string.Empty;
		}
		Color? colorById = skillEffectColors.GetColorById(Status.StatusTypeString);
		if (colorById.HasValue)
		{
			return "<color=#" + ColorUtility.ToHtmlStringRGBA(colorById.Value) + ">" + name + "</color>";
		}
		return name;
	}

	private void RefreshTitle()
	{
		string text = ColoredName(Status.Name);
		if (Status is StatModifierStatus statModifierStatus)
		{
			UnitStatDefinition.E_Stat stat;
			string text2;
			if (Status is BuffStatus)
			{
				stat = statModifierStatus.Stat;
				text2 = $"+{statModifierStatus.ModifierValue}";
			}
			else
			{
				stat = statModifierStatus.Stat;
				text2 = $"{statModifierStatus.ModifierValue}";
			}
			bool flag = stat.ShownAsPercentage();
			text = ColoredName(string.Format("<size=125%>{0}</size>{1} <sprite name={2}>{3}", text2, flag ? "%" : string.Empty, (Status as StatModifierStatus).Stat, UnitDatabase.UnitStatDefinitions[stat].Name));
		}
		if (Status is ImmunityStatus immunityStatus)
		{
			text = ColoredName(text + " <sprite name=" + ImmuneToNegativeStatusEffectDefinition.GetImmuneStatusIconName(immunityStatus.StatusType) + ">");
		}
		titleText.text = text + " " + ((Status is PoisonStatus poisonStatus) ? $"<color=#FF0000>({poisonStatus.DamagePerTurn})</color>" : string.Empty);
	}
}
