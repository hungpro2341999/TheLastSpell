using System;
using System.Collections.Generic;
using RedBlueGames.Tools.TextTyper;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Database;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.Dev;

public class MetaNarrationDebugger : TPSingleton<MetaNarrationDebugger>
{
	[Serializable]
	public class DialogueDebugger
	{
		[SerializeField]
		private TextTyper textTyper;

		[SerializeField]
		private TextMeshProUGUI greeting;

		[SerializeField]
		private MetaNarrationDebuggerReplicaButton[] replicas;

		[SerializeField]
		private Button generateButton;

		public TextMeshProUGUI Greeting => greeting;

		public MetaNarrationDebuggerReplicaButton[] Replicas => replicas;

		public Button GenerateButton => generateButton;

		public TextTyper TextTyper => textTyper;
	}

	[SerializeField]
	private DialogueDebugger darkDebugger;

	[SerializeField]
	private DialogueDebugger lightDebugger;

	[SerializeField]
	private RectTransform[] layoutsToRebuild;

	[SerializeField]
	private TextMeshProUGUI daysPlayedText;

	public int DaysPlayed { get; set; }

	public List<string> UnlockedMetaUpgrades { get; set; } = new List<string>();

	public void DecreaseDaysPlayed()
	{
		DaysPlayed = Mathf.Max(DaysPlayed - 1, 0);
		OnDaysPlayedChanged();
	}

	public void IncreaseDaysPlayed()
	{
		DaysPlayed++;
		OnDaysPlayedChanged();
	}

	public void UnlockMetaUpgrade(string id)
	{
		UnlockedMetaUpgrades.Add(id);
		Generate(MetaNarrationsManager.DarkNarration, darkDebugger);
		Generate(MetaNarrationsManager.LightNarration, lightDebugger);
	}

	private void Generate(MetaNarration narration, DialogueDebugger debugger)
	{
		string nextDialogueGreetingId = narration.MetaNarrationController.GetNextDialogueGreetingId();
		if (debugger.TextTyper != null)
		{
			debugger.TextTyper.Init();
			debugger.TextTyper.TypeText(Localizer.Get(narration.LocalizationGreetingPrefix + nextDialogueGreetingId));
		}
		else
		{
			debugger.Greeting.text = nextDialogueGreetingId;
		}
		if (!narration.MetaNarrationController.TryGetValidMandatoryReplica(1, out var replicas) && !narration.MetaNarrationController.TryGetValidReplicas(debugger.Replicas.Length, out replicas))
		{
			replicas = new List<MetaReplica>();
		}
		for (int i = 0; i < debugger.Replicas.Length; i++)
		{
			debugger.Replicas[i].Display(i < replicas.Count);
			if (debugger.Replicas[i].Displayed)
			{
				debugger.Replicas[i].SetReplica(replicas[i]);
			}
		}
		RebuildLayouts();
	}

	private void OnDaysPlayedChanged()
	{
		daysPlayedText.text = $"Days played: {DaysPlayed}";
		Generate(MetaNarrationsManager.DarkNarration, darkDebugger);
		Generate(MetaNarrationsManager.LightNarration, lightDebugger);
	}

	private void RebuildLayouts()
	{
		RectTransform[] array = layoutsToRebuild;
		for (int i = 0; i < array.Length; i++)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(array[i]);
		}
	}

	private void Start()
	{
		TPSingleton<MetaNarrationDatabase>.Instance.Deserialize();
		TPSingleton<RandomManager>.Instance.Deserialize();
		TPSingleton<MetaNarrationsManager>.Instance.Deserialize();
		MetaNarrationDebuggerReplicaButton[] replicas = darkDebugger.Replicas;
		foreach (MetaNarrationDebuggerReplicaButton obj in replicas)
		{
			obj.Init(MetaNarrationsManager.DarkNarration);
			obj.Button.onClick.AddListener(delegate
			{
				Generate(MetaNarrationsManager.DarkNarration, darkDebugger);
			});
		}
		replicas = lightDebugger.Replicas;
		foreach (MetaNarrationDebuggerReplicaButton obj2 in replicas)
		{
			obj2.Init(MetaNarrationsManager.LightNarration);
			obj2.Button.onClick.AddListener(delegate
			{
				Generate(MetaNarrationsManager.LightNarration, lightDebugger);
			});
		}
		Generate(MetaNarrationsManager.DarkNarration, darkDebugger);
		Generate(MetaNarrationsManager.LightNarration, lightDebugger);
		daysPlayedText.text = $"Days played: {DaysPlayed}";
	}
}
