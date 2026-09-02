using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Building.BuildingUpgrade;

public class SwapSkillDefinition : BuildingUpgradeEffectDefinition
{
	public const string Name = "SwapSkill";

	public string OldSkillId { get; private set; } = string.Empty;

	public int OverallUsesCount { get; private set; }

	public string NewSkillId { get; private set; } = string.Empty;

	public SwapSkillDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		base.Deserialize(xContainer);
		XElement obj = xContainer as XElement;
		XAttribute xAttribute = obj.Attribute("OldSkillId");
		XAttribute xAttribute2 = obj.Attribute("NewSkillId");
		if (!xAttribute.IsNullOrEmpty())
		{
			OldSkillId = xAttribute.Value;
		}
		else
		{
			TPDebug.LogError(base.Id + " UpgradeEffect must have a valid Attribute OldSkillId (token)");
		}
		if (!xAttribute2.IsNullOrEmpty())
		{
			NewSkillId = xAttribute2.Value;
		}
		else
		{
			TPDebug.LogError(base.Id + " UpgradeEffect must have a valid Attribute NewSkillId (token)");
		}
		XAttribute xAttribute3 = obj.Attribute("NightUsesCount");
		if (!xAttribute3.IsNullOrEmpty())
		{
			if (!int.TryParse(xAttribute3.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
			{
				TPDebug.LogError("SwapSkillDefinition " + base.Id + "'s NightUsesCount " + HasAnInvalidInt(xAttribute3.Value));
			}
			else
			{
				OverallUsesCount = result;
			}
		}
	}
}
