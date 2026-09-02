using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Modding.ModuleConfig;

public class ModuleConfigDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Constants
	{
		public const string ConfigFileName = "config.xml";
	}

	public ModuleConfigDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
