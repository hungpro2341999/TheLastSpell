using Sirenix.OdinInspector;
using TheLastStand.View.HUD;
using UnityEngine;

namespace TheLastStand.View.Skill.UI;

public abstract class SkillBar : SerializedMonoBehaviour
{
	[SerializeField]
	protected JoystickSkillBar joystickSkillBar;

	public JoystickSkillBar JoystickSkillBar => joystickSkillBar;

	public abstract void SelectNextSkill(bool next);
}
