using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Tutorial;

public class IsWeaponRestrictionAvailableTutorialConditionDefinition : TutorialConditionDefinition
{
	public static class Constants
	{
		public const string Name = "IsWeaponRestrictionAvailable";
	}

	public IsWeaponRestrictionAvailableTutorialConditionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}
}
