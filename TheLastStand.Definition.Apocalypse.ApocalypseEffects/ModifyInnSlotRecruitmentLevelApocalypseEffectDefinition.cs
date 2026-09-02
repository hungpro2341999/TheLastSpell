using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Apocalypse.ApocalypseEffects;

public class ModifyInnSlotRecruitmentLevelApocalypseEffectDefinition : ApocalypseEffectDefinition
{
	public int SlotIndex { get; private set; }

	public string LevelId { get; private set; }

	public ModifyInnSlotRecruitmentLevelApocalypseEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container != null)
		{
			base.Deserialize(container);
			XElement obj = container as XElement;
			XAttribute xAttribute = obj.Attribute("SlotIndex");
			SlotIndex = Parser.Parse(xAttribute.Value, base.TokenVariables).EvalToInt();
			XAttribute xAttribute2 = obj.Attribute("LevelId");
			LevelId = xAttribute2.Value.Replace(base.TokenVariables);
		}
	}
}
