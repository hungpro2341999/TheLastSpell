using System.Xml.Linq;
using TPLib;

namespace TheLastStand.Definition.Skill;

public class OnlyDuringPhaseConditionDefinition : SkillConditionDefinition
{
	public const string OnlyDuringPhaseName = "OnlyDuringPhase";

	public bool DuringDeployment { get; set; }

	public bool DuringNight { get; set; }

	public bool DuringProduction { get; set; }

	public override string Name => "OnlyDuringPhase";

	public OnlyDuringPhaseConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in (container as XElement).Elements())
		{
			if (item.Name == "Night")
			{
				DuringNight = true;
			}
			else if (item.Name == "Production")
			{
				DuringProduction = true;
			}
			else if (item.Name == "Deployment")
			{
				DuringDeployment = true;
			}
			else
			{
				TPDebug.LogError($"{item.Name} is not a valid phase name");
			}
		}
	}
}
