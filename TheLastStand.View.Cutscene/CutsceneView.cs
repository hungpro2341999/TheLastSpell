using System;
using System.Collections;
using TPLib.Log;
using TheLastStand.Controller.Cutscene;
using TheLastStand.Database;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Framework.Extensions;
using TheLastStand.Model.Cutscene;
using UnityEngine;

namespace TheLastStand.View.Cutscene;

public abstract class CutsceneView : MonoBehaviour
{
	protected Action Callback;

	protected CutsceneData CutsceneData { get; private set; }

	public CutsceneDefinition CutsceneDefinition { get; private set; }

	public bool IsPlaying { get; protected set; }

	public WaitUntil WaitUntilIsOver { get; private set; }

	public virtual bool CanBeSkipped()
	{
		return IsPlaying;
	}

	public void Init(string unitCutsceneId, CutsceneData cutsceneData = default(CutsceneData))
	{
		CutsceneDefinition = GameDatabase.CutsceneDefinitions.GetValueOrDefault(unitCutsceneId);
		CutsceneData = cutsceneData;
		if (WaitUntilIsOver == null)
		{
			WaitUntil waitUntil = (WaitUntilIsOver = new WaitUntil(() => !IsPlaying));
		}
	}

	public abstract IEnumerator PlayCutscene(Action callback = null);

	public virtual IEnumerator Skip()
	{
		Callback?.Invoke();
		IsPlaying = false;
		yield break;
	}

	protected IEnumerator PlayCutsceneDefinition(CutsceneDefinition cutsceneDefinition, CutsceneData cutsceneData)
	{
		TheLastStand.Model.Cutscene.Cutscene cutscene = new TheLastStand.Model.Cutscene.Cutscene(cutsceneDefinition);
		CutsceneController[] sequenceControllers = cutscene.SequenceControllers;
		foreach (CutsceneController cutsceneController in sequenceControllers)
		{
			if (cutsceneController == null)
			{
				CLoggerManager.Log("Null element in cutscene, probably coming from a database deserialization error.");
			}
			else
			{
				yield return cutsceneController.Play(cutsceneData);
			}
		}
	}
}
