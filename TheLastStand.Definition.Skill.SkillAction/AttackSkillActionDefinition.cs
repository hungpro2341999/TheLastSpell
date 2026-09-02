using System;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Skill.SkillAction;

public class AttackSkillActionDefinition : SkillActionDefinition
{
	public enum E_AttackType : byte
	{
		None,
		Physical,
		Magical,
		Ranged,
		Adaptative
	}

	public const string Name = "Attack";

	public Node MinDamageNode;

	public Node MaxDamageNode;

	public E_AttackType AttackType { get; private set; }

	public float CriticProbability { get; private set; }

	public float DamageMultiplier { get; private set; }

	public AttackSkillActionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container.Element("Attack");
		XElement xElement2 = xElement.Element("BaseDamage");
		if (xElement2 != null)
		{
			MinDamageNode = Parser.Parse(xElement2.Attribute("Min").Value);
			MaxDamageNode = Parser.Parse(xElement2.Attribute("Max").Value);
		}
		DamageMultiplier = float.Parse(xElement.Element("DamageMultiplier").Value, NumberStyles.Float, CultureInfo.InvariantCulture);
		if (Enum.TryParse<E_AttackType>(xElement.Element("AttackType")?.Value, out var result))
		{
			AttackType = result;
		}
		else
		{
			CLoggerManager.Log("Error while parsing AttackType of Skill " + xElement.Parent.Parent.Attribute("Id")?.Value + " \"skillActionSpecificElement.Element(\"AttackType\")?.Value\" to a valid E_AttackType value.");
		}
		if (xElement.Element("CriticProbability") != null)
		{
			if (float.TryParse(xElement.Element("CriticProbability").Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
			{
				CriticProbability = result2;
			}
			else
			{
				Debug.LogError("CriticProbability of AttackDefinition value is not a float: " + xElement.Element("CriticProbability").Value);
			}
		}
	}

	public Vector2 GetBaseDamage(FormulaInterpreterContext formulaInterpreterContext)
	{
		if (MinDamageNode != null && MaxDamageNode != null)
		{
			return new Vector2(MinDamageNode.EvalToFloat(formulaInterpreterContext), MaxDamageNode.EvalToFloat(formulaInterpreterContext));
		}
		return Vector2.zero;
	}
}
