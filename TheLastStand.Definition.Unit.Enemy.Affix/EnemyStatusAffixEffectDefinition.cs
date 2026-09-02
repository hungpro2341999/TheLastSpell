using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Model.Status;

namespace TheLastStand.Definition.Unit.Enemy.Affix;

public abstract class EnemyStatusAffixEffectDefinition : EnemyAffixEffectDefinition
{
	public Status.E_StatusType StatusType { get; private set; }

	protected EnemyStatusAffixEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (Enum.TryParse<Status.E_StatusType>((container as XElement).Attribute("StatusType").Value.Replace(base.TokenVariables), out var result))
		{
			StatusType = result;
		}
	}
}
