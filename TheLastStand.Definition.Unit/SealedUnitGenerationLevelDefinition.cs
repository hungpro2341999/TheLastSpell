using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class SealedUnitGenerationLevelDefinition : ILegacyDeserializable
{
	public Node Level { get; set; }

	public int Seal { get; set; }

	public UnitGenerationLevelDefinition UnitGenerationLevelDefinition { get; private set; }

	public SealedUnitGenerationLevelDefinition(UnitGenerationLevelDefinition unitGenerationLevelDefinition)
	{
		UnitGenerationLevelDefinition = unitGenerationLevelDefinition;
	}

	public void Deserialize(XContainer container)
	{
		XElement xElement = container.Element("Level");
		if (xElement == null)
		{
			Debug.LogError("The SealedUnitGenerationDefinition has no Level!");
		}
		else
		{
			Level = Parser.Parse(xElement.Value);
		}
	}

	public SealedUnitGenerationLevelDefinition ShallowCopy()
	{
		return (SealedUnitGenerationLevelDefinition)MemberwiseClone();
	}
}
