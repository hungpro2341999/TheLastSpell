using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TPLib.Log;
using TPLib.Yield;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction;

public class GainArmorFeedback : SerializedMonoBehaviour, IDisplayableEffect
{
	[SerializeField]
	private float delayBetweenArmorInstances = 0.15f;

	private Queue<Tuple<float, float>> armorInstancesBeingDisplayed = new Queue<Tuple<float, float>>();

	private Queue<Tuple<float, float>> armorInstancesInPreparation = new Queue<Tuple<float, float>>();

	private Coroutine displayCoroutine;

	public IDamageableView DamageableView { get; set; }

	public void AddArmorGainInstance(float armorAmount, float armorthAfterArmor)
	{
		armorInstancesInPreparation.Enqueue(new Tuple<float, float>(armorAmount, armorthAfterArmor));
	}

	public Coroutine Display()
	{
		while (armorInstancesInPreparation.Count > 0)
		{
			armorInstancesBeingDisplayed.Enqueue(armorInstancesInPreparation.Dequeue());
		}
		if (displayCoroutine == null)
		{
			displayCoroutine = StartCoroutine(DisplayCoroutine());
		}
		return displayCoroutine;
	}

	public void Init(IDamageableView damageableView)
	{
		base.name = "Gain Armor Feedback - " + damageableView.GameObject.name;
		DamageableView = damageableView;
		base.transform.SetParent(damageableView.GameObject.transform, worldPositionStays: false);
	}

	private IEnumerator DisplayCoroutine()
	{
		CLoggerManager.Log("Start DisplayCoroutine", this, LogType.Log, CLogLevel.DETAILED, forcePrintInUnity: true, "Feedbacks");
		_ = Vector3.zero;
		while (armorInstancesBeingDisplayed.Count > 0)
		{
			Tuple<float, float> tuple = armorInstancesBeingDisplayed.Dequeue();
			CLoggerManager.Log($"Displaying damageInstance {tuple}", this, LogType.Log, CLogLevel.DETAILED, forcePrintInUnity: true, "Feedbacks");
			DamageableView.DamageableHUD.PlayArmorGainAnim(tuple.Item1, tuple.Item2);
			yield return SharedYields.WaitForSeconds(delayBetweenArmorInstances);
		}
		displayCoroutine = null;
		CLoggerManager.Log("Finished DisplayCoroutine", this, LogType.Log, CLogLevel.DETAILED, forcePrintInUnity: true, "Feedbacks");
	}
}
