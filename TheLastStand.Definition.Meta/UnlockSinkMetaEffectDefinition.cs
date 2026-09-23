using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Định nghĩa hiệu ứng Meta: Mở khóa bồn nước phục hồi (UnlockSink).
/// <para>Mở khóa tính năng bồn giếng nước/bể tiêu hao mana &amp; máu trong thành phố.</para>
/// </summary>
public class UnlockSinkMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockSink";

	public UnlockSinkMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
