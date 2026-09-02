using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Cutscene;

public class IncreaseFogCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public class Constants
	{
		public const string Id = "IncreaseFog";
	}

	public int Value { get; private set; } = 1;

	public IncreaseFogCutsceneDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (int.TryParse((container as XElement).Attribute("Value")?.Value, out var result))
		{
			Value = result;
		}
	}
}
