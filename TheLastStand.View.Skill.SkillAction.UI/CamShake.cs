using System.Collections;
using TheLastStand.View.Camera;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class CamShake : EffectDisplay
{
	[SerializeField]
	private string shakePresetId;

	public string ShakePresetId
	{
		get
		{
			return shakePresetId;
		}
		set
		{
			shakePresetId = value;
		}
	}

	protected override IEnumerator DisplayCoroutine()
	{
		if (!string.IsNullOrEmpty(shakePresetId))
		{
			ACameraView.Shake(shakePresetId);
			yield return base.DisplayCoroutine();
		}
	}
}
