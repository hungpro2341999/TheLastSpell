using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Database;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Apocalypse.ApocalypseEffects;

public class MultiplyEnemyUnitSpawnWaveWeightApocalypseEffectDefinition : ApocalypseEffectDefinition
{
	public List<string> EnemyUnitIds { get; private set; }

	public float Multiplier { get; set; }

	public MultiplyEnemyUnitSpawnWaveWeightApocalypseEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container as XElement;
		EnemyUnitIds = new List<string>();
		foreach (XElement item in xElement.Elements("EnemyList"))
		{
			string key = item.Value.Replace(base.TokenVariables);
			foreach (string id in GenericDatabase.IdsListDefinitions[key].Ids)
			{
				AddEnemyUnitId(id);
			}
		}
		foreach (XElement item2 in xElement.Elements("EnemyId"))
		{
			AddEnemyUnitId(item2.Value.Replace(base.TokenVariables));
		}
		XAttribute xAttribute = xElement.Attribute("Multiplier");
		Multiplier = Parser.Parse(xAttribute.Value, base.TokenVariables).EvalToFloat();
	}

	private void AddEnemyUnitId(string id)
	{
		if (!EnemyUnitIds.Contains(id))
		{
			EnemyUnitIds.Add(id);
		}
	}
}
