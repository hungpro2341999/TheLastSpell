using System.Collections;
using Sirenix.OdinInspector;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.View.Building;
using TheLastStand.View.Skill.SkillAction.UI;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction;

public class ExtinguishBrazierFeedback : SerializedMonoBehaviour, IDisplayableEffect
{
	private int brazierLoss;

	private int brazierRemainingAfterLoss;

	private Coroutine displayCoroutine;

	private DamageDisplay damageDisplayPrefab;

	public BuildingView BuildingView { get; set; }

	public Coroutine Display()
	{
		if (displayCoroutine == null)
		{
			displayCoroutine = TPSingleton<EffectManager>.Instance.StartCoroutine(DisplayCoroutine());
		}
		return displayCoroutine;
	}

	public void Init(BuildingView buildingView)
	{
		base.name = "Extinguish Brazier Feedback - " + buildingView.GameObject.name;
		BuildingView = buildingView;
		base.transform.SetParent(buildingView.GameObject.transform, worldPositionStays: false);
	}

	public void InitBrazierLoss(int brazierLoss, int brazierRemainingAfterLoss)
	{
		this.brazierLoss = brazierLoss;
		this.brazierRemainingAfterLoss = brazierRemainingAfterLoss;
	}

	private IEnumerator DisplayCoroutine()
	{
		CLoggerManager.Log($"Displaying ExtinguishBrazier Feedback {brazierLoss}", this, LogType.Log, CLogLevel.DETAILED, forcePrintInUnity: true, "Feedbacks");
		DamageDisplay pooledComponent = ObjectPooler.GetPooledComponent("DamageDisplay", ResourcePooler.LoadOnce<DamageDisplay>("Prefab/Displayable Effect/UI Effect Displays/DamageDisplay"), EffectManager.EffectDisplaysParent);
		pooledComponent.name = "DamageDisplay - " + BuildingView.GameObject.name;
		pooledComponent.FollowElement.ChangeTarget(BuildingView.DamageableHUD.Transform);
		pooledComponent.Init(brazierLoss);
		pooledComponent.Display();
		BuildingView.BuildingHUD.PlayBrazierLossAnim(brazierLoss, brazierRemainingAfterLoss);
		yield break;
	}
}
