using System;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.Model.Status;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public class StatusInflictedTrophyDefinition : ValueIntHeroesTrophyConditionDefinition
{
	public const string Name = "StatusInflicted";

	public override object[] DescriptionLocalizationParameters => new object[2]
	{
		StylizeStatus(),
		base.Value
	};

	public Status.E_StatusType StatusType { get; private set; }

	public StatusInflictedTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container as XElement;
		string[] array = xElement.Attribute("Status").Value.Split('|');
		if (array[0] == "Any")
		{
			StatusType = Status.E_StatusType.Buff | Status.E_StatusType.Debuff | Status.E_StatusType.Poison | Status.E_StatusType.Stun;
		}
		else
		{
			foreach (string text in array)
			{
				if (Enum.TryParse<Status.E_StatusType>(text, out var result))
				{
					StatusType |= result;
					continue;
				}
				TPSingleton<TrophyManager>.Instance.LogError("A Status in an Element : StatusInflicted in TrophiesDefinitions doesn't exist (status : " + text + ")");
				return;
			}
		}
		if (!int.TryParse(xElement.Value, out var result2))
		{
			TPDebug.LogError("The Value of an Element : StatusInflicted in TrophiesDefinitions isn't a valid int");
		}
		else
		{
			base.Value = result2;
		}
	}

	public override string ToString()
	{
		return "StatusInflicted";
	}

	private string StylizeStatus()
	{
		string text = string.Empty;
		if ((StatusType & Status.E_StatusType.Poison) == Status.E_StatusType.Poison)
		{
			text += "<style=Poison>Poison</style>";
		}
		if ((StatusType & Status.E_StatusType.Stun) == Status.E_StatusType.Stun)
		{
			if (text != string.Empty)
			{
				text += " | ";
			}
			text += "<style=Stun>Stun</style>";
		}
		if ((StatusType & Status.E_StatusType.Debuff) == Status.E_StatusType.Debuff)
		{
			if (text != string.Empty)
			{
				text += " | ";
			}
			text += "<style=Debuff>Debuff</style>";
		}
		return text;
	}
}
