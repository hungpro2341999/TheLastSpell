using System.Collections.Generic;
using TPLib;
using TheLastStand.Definition.Meta;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Meta;
using TheLastStand.Serialization;

namespace TheLastStand.Controller.Meta;

public class MetaNarrationController
{
	public MetaNarration MetaNarration { get; private set; }

	public MetaNarrationController(MetaNarrationDefinition definition, SerializedNarration serializedNarration)
	{
		MetaNarration = new MetaNarration(definition, this, serializedNarration);
		CreateVisualEvolutionsConditions();
	}

	public MetaNarrationController(MetaNarrationDefinition definition)
	{
		MetaNarration = new MetaNarration(definition, this);
		for (int i = 0; i < MetaNarration.MetaNarrationDefinition.DialogueGreetings.Count; i++)
		{
			MetaNarration.DialogueGreetingsIdsLeft.Add(MetaNarration.MetaNarrationDefinition.DialogueGreetings[i]);
		}
		MetaNarration.DialogueGreetingsIdsLeft.Shuffle();
		CreateVisualEvolutionsConditions();
	}

	public bool CanDisplayGoddessName()
	{
		return CanDisplayGoddessName(MetaNarration.AlreadyUsedReplicasIds);
	}

	public bool CanDisplayGoddessName(List<string> alreadyUsedReplicasIds)
	{
		return alreadyUsedReplicasIds.Contains(MetaNarration.MetaNarrationDefinition.NameRevealDialogueId);
	}

	public string GetNextDialogueGreetingId()
	{
		MetaNarration.NextDialogueGreetingIndex %= MetaNarration.DialogueGreetingsIdsLeft.Count;
		string result = MetaNarration.DialogueGreetingsIdsLeft[MetaNarration.NextDialogueGreetingIndex];
		MetaNarration.NextDialogueGreetingIndex++;
		MetaNarration.NextDialogueGreetingIndex %= MetaNarration.DialogueGreetingsIdsLeft.Count;
		return result;
	}

	public string GetRandomShopGreetingId()
	{
		return MetaNarration.MetaNarrationDefinition.ShopGreetings.RandomElement();
	}

	public int GetHighestAvailableVisualEvolutionIndex()
	{
		int num = 0;
		for (int i = 0; i < MetaNarration.VisualEvolutionsConditionControllers.Count; i++)
		{
			for (int num2 = MetaNarration.VisualEvolutionsConditionControllers[i].Count - 1; num2 >= 0; num2--)
			{
				if (!MetaNarration.VisualEvolutionsConditionControllers[i][num2].Evaluate())
				{
					return num;
				}
			}
			num++;
		}
		return num;
	}

	public bool TryGetValidMandatoryReplica(int count, out List<MetaReplica> replicas)
	{
		replicas = GetValidReplicas(count, MetaNarration.MandatoryReplicas);
		return replicas.Count > 0;
	}

	public bool TryGetValidReplicas(int count, out List<MetaReplica> replicas)
	{
		replicas = GetValidReplicas(count, MetaNarration.Replicas);
		return replicas.Count > 0;
	}

	public void MarkReplicaAsUsed(MetaReplica replica)
	{
		MetaNarration.AlreadyUsedReplicasIds.Add(replica.Id);
		if (replica.MetaReplicaDefinition.BlockReplicas != null)
		{
			for (int num = replica.MetaReplicaDefinition.BlockReplicas.Count - 1; num >= 0; num--)
			{
				string item = replica.MetaReplicaDefinition.BlockReplicas[num];
				MetaNarrationsManager.DarkNarration.AlreadyUsedReplicasIds.Add(item);
				MetaNarrationsManager.LightNarration.AlreadyUsedReplicasIds.Add(item);
			}
		}
	}

	private void CreateVisualEvolutionsConditions()
	{
		MetaNarration.VisualEvolutionsConditionControllers = new List<List<MetaNarrationConditionController>>();
		for (int i = 0; i < MetaNarration.MetaNarrationDefinition.VisualEvolutions.Count; i++)
		{
			List<MetaNarrationConditionController> list = new List<MetaNarrationConditionController>();
			for (int num = MetaNarration.MetaNarrationDefinition.VisualEvolutions[i].Conditions.Count - 1; num >= 0; num--)
			{
				MetaReplicaConditionDefinition metaReplicaConditionDefinition = MetaNarration.MetaNarrationDefinition.VisualEvolutions[i].Conditions[num];
				if (!(metaReplicaConditionDefinition is MetaReplicaDaysPlayedConditionDefinition definition))
				{
					if (!(metaReplicaConditionDefinition is MetaReplicaMetaUpgradeUnlockedConditionDefinition definition2))
					{
						if (!(metaReplicaConditionDefinition is MetaReplicaUsedReplicaConditionDefinition definition3))
						{
							TPSingleton<MetaNarrationsManager>.Instance.LogError("Could not create a narration condition for definition type " + metaReplicaConditionDefinition.GetType().Name + ".");
							return;
						}
						list.Add(new MetaReplicaUsedReplicaConditionController(definition3));
					}
					else
					{
						list.Add(new MetaNarrationMetaUpgradeUnlockedConditionController(definition2));
					}
				}
				else
				{
					list.Add(new MetaNarrationDaysPlayedConditionController(definition));
				}
			}
			MetaNarration.VisualEvolutionsConditionControllers.Add(list);
		}
	}

	private List<MetaReplica> GetValidReplicas(int count, Dictionary<string, MetaReplica> replicasDictionary)
	{
		List<MetaReplica> list = new List<MetaReplica>();
		foreach (KeyValuePair<string, MetaReplica> item in replicasDictionary)
		{
			if (!MetaNarration.AlreadyUsedReplicasIds.Contains(item.Key) && item.Value.MetaReplicaController.CheckConditions())
			{
				list.Add(item.Value);
				if (list.Count == count)
				{
					break;
				}
			}
		}
		return list;
	}
}
