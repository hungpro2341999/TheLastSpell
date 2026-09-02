using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

public class NewEnemyMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "NewEnemy";

	public string EnemyId { get; private set; }

	public NewEnemyMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		EnemyId = xElement.Value;
	}

	public override string ToString()
	{
		return "NewEnemy (" + EnemyId + ")";
	}
}
