using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Định nghĩa hiệu ứng Meta: Mở khóa reroll trong Cửa hàng (UnlockShopReroll).
/// <para>Cho phép người chơi làm mới danh sách hàng hóa trong Shop bằng Vàng.</para>
/// </summary>
public class UnlockShopRerollMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockShopReroll";

	public UnlockShopRerollMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
