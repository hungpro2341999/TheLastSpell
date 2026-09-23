using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Định nghĩa hiệu ứng Meta: Mở khóa chủng tộc nhân vật (UnlockRaces).
/// <para>Cho phép người chơi mở khóa các chủng tộc mới như Elf, Dwarf... cho tướng tuyển mộ.</para>
/// </summary>
public class UnlockRacesMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockRaces";

	public const string ChildName = "Race";

	/// <summary>
	/// Danh sách các Id chủng tộc được mở khóa.
	/// </summary>
	public List<string> RacesToUnlock = new List<string>();

	public UnlockRacesMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in (container as XElement).Elements("Race"))
		{
			if (item.Value != string.Empty)
			{
				RacesToUnlock.Add(item.Value);
			}
		}
	}
}
