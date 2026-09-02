using System.Collections.Generic;
using TPLib.Localization;
using TheLastStand.Controller.Meta;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.Serialization;
using TheLastStand.Serialization;

namespace TheLastStand.Model.Meta;

public class MetaNarration : ISerializable, IDeserializable
{
	public MetaNarrationController MetaNarrationController { get; private set; }

	public MetaNarrationDefinition MetaNarrationDefinition { get; private set; }

	public List<string> AlreadyUsedReplicasIds { get; set; } = new List<string>();

	public List<string> DialogueGreetingsIdsLeft { get; set; } = new List<string>();

	public string GoddessName => Localizer.Get(IsDarkOne ? "Goddess_Name_Dark" : "Goddess_Name_Light");

	public bool IsDarkOne { get; set; }

	public string LocalizationGreetingPrefix
	{
		get
		{
			if (!IsDarkOne)
			{
				return "Greeting_Light_";
			}
			return "Greeting_Dark_";
		}
	}

	public Dictionary<string, MetaReplica> MandatoryReplicas { get; private set; } = new Dictionary<string, MetaReplica>();

	public int NextDialogueGreetingIndex { get; set; }

	public Dictionary<string, MetaReplica> Replicas { get; private set; } = new Dictionary<string, MetaReplica>();

	public List<List<MetaNarrationConditionController>> VisualEvolutionsConditionControllers { get; set; }

	public MetaNarration(MetaNarrationDefinition definition, MetaNarrationController controller, SerializedNarration serializedNarration)
	{
		MetaNarrationDefinition = definition;
		MetaNarrationController = controller;
		Deserialize(serializedNarration);
	}

	public MetaNarration(MetaNarrationDefinition definition, MetaNarrationController controller)
	{
		MetaNarrationDefinition = definition;
		MetaNarrationController = controller;
		Deserialize();
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		if (container is SerializedNarration serializedNarration)
		{
			AlreadyUsedReplicasIds = serializedNarration.AlreadyUsedReplicasIds;
			DialogueGreetingsIdsLeft = serializedNarration.DialogueGreetingsIdsLeft;
			NextDialogueGreetingIndex = serializedNarration.NextDialogueGreetingIndex;
		}
		for (int i = 0; i < MetaNarrationDefinition.ReplicaDefinitions.Count; i++)
		{
			Replicas.Add(MetaNarrationDefinition.ReplicaDefinitions[i].Id, new MetaReplicaController(MetaNarrationDefinition.ReplicaDefinitions[i], this).MetaReplica);
		}
		for (int j = 0; j < MetaNarrationDefinition.MandatoryReplicaDefinitions.Count; j++)
		{
			MandatoryReplicas.Add(MetaNarrationDefinition.MandatoryReplicaDefinitions[j].Id, new MetaReplicaController(MetaNarrationDefinition.MandatoryReplicaDefinitions[j], this).MetaReplica);
		}
	}

	public ISerializedData Serialize()
	{
		return new SerializedNarration
		{
			AlreadyUsedReplicasIds = AlreadyUsedReplicasIds,
			DialogueGreetingsIdsLeft = DialogueGreetingsIdsLeft,
			NextDialogueGreetingIndex = NextDialogueGreetingIndex
		};
	}
}
