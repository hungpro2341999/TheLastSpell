using System.Collections.Generic;
using TheLastStand.Controller.Meta;
using TheLastStand.Definition.Meta;

namespace TheLastStand.Model.Meta;

public class MetaReplica
{
	public List<MetaNarrationConditionController> ConditionControllers { get; set; }

	public MetaNarration Context { get; }

	public string Id => MetaReplicaDefinition.Id;

	public string LocalizationKey => (Context.IsDarkOne ? "Replica_Dark_" : "Replica_Light_") + Id;

	public string LocalizationAnswerFormat
	{
		get
		{
			if (!Context.IsDarkOne)
			{
				return "Replica_Light_{0}_Answer{1}";
			}
			return "Replica_Dark_{0}_Answer{1}";
		}
	}

	public MetaReplicaDefinition MetaReplicaDefinition { get; }

	public MetaReplicaController MetaReplicaController { get; }

	public MetaReplica(MetaReplicaDefinition definition, MetaReplicaController controller, MetaNarration context)
	{
		MetaReplicaDefinition = definition;
		MetaReplicaController = controller;
		Context = context;
	}
}
