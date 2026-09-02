using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition;

public class GameDefinition : TheLastStand.Framework.Serialization.Definition
{
	public enum E_Direction
	{
		None = -1,
		North,
		South,
		East,
		West
	}

	public GameDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
