using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Status;

namespace TheLastStand.Definition.Unit.Enemy.Affix;

public class EnemyHealthChunksAffixEffectDefinition : EnemyAffixEffectDefinition
{
	public Dictionary<Status.E_StatusType, int> ApplyStatusesWithDuration = new Dictionary<Status.E_StatusType, int>();

	public List<Status.E_StatusType> RemoveStatuses = new List<Status.E_StatusType>();

	public override E_EnemyAffixEffect EnemyAffixEffect => E_EnemyAffixEffect.HealthChunks;

	public Node StepPercentage { get; private set; }

	public EnemyHealthChunksAffixEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XElement xElement2 = xElement.Element("Step");
		StepPercentage = Parser.Parse(xElement2.Value, base.TokenVariables);
		foreach (XElement item in xElement.Elements("ApplyStatus"))
		{
			XAttribute xAttribute = item.Attribute("Status");
			if (!Enum.TryParse<Status.E_StatusType>(xAttribute.Value, out var result))
			{
				TPSingleton<EnemyUnitManager>.Instance.LogError("Can not parse the apply status type (" + xAttribute.Value + ") in HealthChunks EnemyAffixEffect");
				continue;
			}
			XAttribute xAttribute2 = item.Attribute("TurnsCount");
			if (!int.TryParse(xAttribute2.Value, out var result2))
			{
				TPSingleton<EnemyUnitManager>.Instance.LogError("Can not parse the turns count (" + xAttribute2.Value + ") in HealthChunks EnemyAffixEffect");
			}
			else
			{
				ApplyStatusesWithDuration.Add(result, result2);
			}
		}
		foreach (XElement item2 in xElement.Elements("RemoveStatus"))
		{
			string text = item2.Attribute("Status").Value.Replace(base.TokenVariables);
			if (!Enum.TryParse<Status.E_StatusType>(text, out var result3))
			{
				TPSingleton<EnemyUnitManager>.Instance.LogError("Can not parse the remove status type (" + text + ") in HealthChunks EnemyAffixEffect");
			}
			else
			{
				RemoveStatuses.Add(result3);
			}
		}
	}
}
