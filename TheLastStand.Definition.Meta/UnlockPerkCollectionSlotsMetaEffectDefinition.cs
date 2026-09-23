using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Định nghĩa hiệu ứng Meta: Mở khóa các ô trong bảng ngọc Perk (UnlockPerkCollectionSlots).
/// <para>Cho phép người chơi có thêm các ô chọn kỹ năng bị động (Perk) cho nhân vật.</para>
/// </summary>
public class UnlockPerkCollectionSlotsMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockPerkCollectionSlots";

	/// <summary>
	/// Tập hợp các chỉ số ô (slot index) trong bộ sưu tập Perk được mở khóa.
	/// </summary>
	public HashSet<int> PerkCollectionSlotsToUnlock = new HashSet<int>();

	public UnlockPerkCollectionSlotsMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in (container as XElement).Elements("CollectionSlotToUnlock"))
		{
			if (int.TryParse(item.Value, out var result))
			{
				PerkCollectionSlotsToUnlock.Add(result);
			}
		}
	}
}
