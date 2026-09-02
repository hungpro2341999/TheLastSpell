using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecutionTileData;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Skill.SkillAction.UI;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction;

public class AttackFeedback : SerializedMonoBehaviour, IDisplayableEffect
{
	public static class Constants
	{
		public const string ImpactFXObjectPoolName = "Impact FX";

		public const string DamageDisplayPrefabResourcePath = "Prefab/Displayable Effect/UI Effect Displays/DamageDisplay";

		public const string ImpactFxPrefabResourcePath = "Prefab/Displayable Effect/Impact FX";
	}

	[SerializeField]
	private Vector2 impactOffsetRange = new Vector2(-0.5f, 0.5f);

	private Queue<AttackSkillActionExecutionTileData> damageInstancesBeingDisplayed = new Queue<AttackSkillActionExecutionTileData>();

	private Queue<AttackSkillActionExecutionTileData> damageInstancesInPreparation = new Queue<AttackSkillActionExecutionTileData>();

	private Coroutine displayCoroutine;

	private GameObject impactFxPrefab;

	private DamageDisplay damageDisplayPrefab;

	public IDamageableView DamageableView { get; set; }

	private DamageDisplay DamageDisplayPrefab
	{
		get
		{
			if (damageDisplayPrefab == null)
			{
				damageDisplayPrefab = ResourcePooler.LoadOnce<DamageDisplay>("Prefab/Displayable Effect/UI Effect Displays/DamageDisplay");
			}
			return damageDisplayPrefab;
		}
	}

	private GameObject ImpactFxPrefab
	{
		get
		{
			if (impactFxPrefab == null)
			{
				impactFxPrefab = ResourcePooler.LoadOnce<GameObject>("Prefab/Displayable Effect/Impact FX");
			}
			return impactFxPrefab;
		}
	}

	public void AddDamageInstance(AttackSkillActionExecutionTileData attackData)
	{
		damageInstancesInPreparation.Enqueue(attackData);
	}

	public Coroutine Display()
	{
		while (damageInstancesInPreparation.Count > 0)
		{
			damageInstancesBeingDisplayed.Enqueue(damageInstancesInPreparation.Dequeue());
		}
		if (displayCoroutine == null)
		{
			displayCoroutine = TPSingleton<EffectManager>.Instance.StartCoroutine(DisplayCoroutine());
		}
		return displayCoroutine;
	}

	public void Init(IDamageableView damageableView)
	{
		base.name = "Attack Feedback - " + damageableView.GameObject.name;
		DamageableView = damageableView;
		base.transform.SetParent(damageableView.GameObject.transform, worldPositionStays: false);
	}

	private IEnumerator DisplayCoroutine()
	{
		CLoggerManager.Log("Start DisplayCoroutine", this, LogType.Log, CLogLevel.DETAILED, forcePrintInUnity: true, "Feedbacks");
		Vector3 impactOffset = Vector3.zero;
		EnemyUnit enemyUnit = DamageableView.Damageable as EnemyUnit;
		while (damageInstancesBeingDisplayed.Count > 0)
		{
			AttackSkillActionExecutionTileData attackSkillActionExecutionTileData = damageInstancesBeingDisplayed.Dequeue();
			CLoggerManager.Log($"Displaying damageInstance {attackSkillActionExecutionTileData.TotalDamage}", this, LogType.Log, CLogLevel.DETAILED, forcePrintInUnity: true, "Feedbacks");
			DamageDisplay pooledComponent = ObjectPooler.GetPooledComponent("DamageDisplay", DamageDisplayPrefab, EffectManager.EffectDisplaysParent);
			pooledComponent.name = "DamageDisplay - " + DamageableView.GameObject.name;
			pooledComponent.FollowElement.ChangeTarget(DamageableView.DamageableHUD.Transform);
			pooledComponent.Init(attackSkillActionExecutionTileData);
			pooledComponent.Display();
			if (!attackSkillActionExecutionTileData.Dodged)
			{
				SpriteSheetFx component = Object.Instantiate(ImpactFxPrefab, GameManager.ViewTransform).GetComponent<SpriteSheetFx>();
				impactOffset.x = RandomManager.GetRandomRange(this, impactOffsetRange.x, impactOffsetRange.y);
				impactOffset.y = RandomManager.GetRandomRange(this, impactOffsetRange.x, impactOffsetRange.y);
				component.Init(TileMapView.GetWorldPosition(attackSkillActionExecutionTileData.TargetTile) + impactOffset);
				component.Display();
				DamageableView.DamageableHUD.PlayDamageAnim(attackSkillActionExecutionTileData);
				DamageableView.PlayTakeDamageAnim();
			}
			if (!DamageableView.Damageable.IsDead || damageInstancesBeingDisplayed.Count != 0)
			{
				continue;
			}
			bool isFinalDeath = TPSingleton<BossManager>.Instance.ShouldTriggerBossWaveVictory;
			if (enemyUnit is BossUnit bossUnit && (isFinalDeath || bossUnit.AlwaysPlayDeathCutscene))
			{
				yield return TPSingleton<BossManager>.Instance.TryPlayDeathCutscene(bossUnit, isFinalDeath);
				if (isFinalDeath)
				{
					TPSingleton<NightTurnsManager>.Instance.StopCoroutinesExecution();
				}
			}
			else if (!(DamageableView.Damageable is TheLastStand.Model.Unit.Unit { HasBeenExiled: not false, ExileForcePlayDieAnim: false }))
			{
				DamageableView.PlayDieAnim();
			}
		}
		displayCoroutine = null;
		CLoggerManager.Log("Finished DisplayCoroutine", this, LogType.Log, CLogLevel.DETAILED, forcePrintInUnity: true, "Feedbacks");
	}
}
