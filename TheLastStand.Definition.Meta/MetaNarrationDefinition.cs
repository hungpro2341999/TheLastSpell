using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Định nghĩa hệ thống Dẫn chuyện &amp; Hội thoại Meta (Meta Narration Definition).
/// <para>Quản lý các đoạn hội thoại, lời chào hỏi khi vào Oraculum, các câu thoại của nhân vật/vị thần và các mốc tiến hóa hình ảnh theo cốt truyện.</para>
/// </summary>
public class MetaNarrationDefinition : TheLastStand.Framework.Serialization.Definition
{
	/// <summary>
	/// Danh sách các câu thoại chào hỏi thông thường khi tương tác với NPC/vị thần tại Oraculum.
	/// </summary>
	public List<string> DialogueGreetings { get; private set; }

	/// <summary>
	/// Danh sách các câu thoại bắt buộc phải phát (Mandatory Replicas) khi đủ điều kiện cốt truyện.
	/// </summary>
	public List<MetaReplicaDefinition> MandatoryReplicaDefinitions { get; private set; }

	/// <summary>
	/// Mã định danh của đoạn hội thoại tiết lộ danh tính thật của nhân vật bí ẩn.
	/// </summary>
	public string NameRevealDialogueId { get; private set; }

	/// <summary>
	/// Danh sách các câu thoại ngẫu nhiên thông thường của nhân vật.
	/// </summary>
	public List<MetaReplicaDefinition> ReplicaDefinitions { get; private set; }

	/// <summary>
	/// Danh sách các lời chào khi người chơi mở giao diện Cửa hàng Meta (Damned Souls Shop).
	/// </summary>
	public List<string> ShopGreetings { get; private set; }

	/// <summary>
	/// Danh sách các mốc điều kiện để thay đổi/tiến hóa ngoại hình (Visual Evolution) của vị thần/Oraculum theo cốt truyện.
	/// </summary>
	public List<MetaNarrationConditionsDefinition> VisualEvolutions { get; private set; }

	public MetaNarrationDefinition(XContainer container)
		: base(container)
	{
	}

	/// <summary>
	/// Giải tuần tự hóa toàn bộ dữ liệu kịch bản dẫn chuyện từ XML.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		DialogueGreetings = new List<string>();
		ShopGreetings = new List<string>();
		ReplicaDefinitions = new List<MetaReplicaDefinition>();
		MandatoryReplicaDefinitions = new List<MetaReplicaDefinition>();
		VisualEvolutions = new List<MetaNarrationConditionsDefinition>();
		
		XElement xElement2 = xElement.Element("NameRevealDialogueId");
		NameRevealDialogueId = xElement2.Value;

		// Đọc lời chào hội thoại
		foreach (XElement item in xElement.Element("DialogueGreetings").Elements("DialogueGreeting"))
		{
			DialogueGreetings.Add(item.Value);
		}

		// Đọc lời chào cửa hàng
		foreach (XElement item2 in xElement.Element("ShopGreetings").Elements("ShopGreeting"))
		{
			ShopGreetings.Add(item2.Value);
		}

		// Đọc các câu thoại kịch bản (phân tách bắt buộc và thông thường)
		foreach (XElement item3 in xElement.Element("Replicas").Elements("Replica"))
		{
			MetaReplicaDefinition metaReplicaDefinition = new MetaReplicaDefinition(item3);
			if (metaReplicaDefinition.Mandatory)
			{
				MandatoryReplicaDefinitions.Add(metaReplicaDefinition);
			}
			else
			{
				ReplicaDefinitions.Add(metaReplicaDefinition);
			}
		}

		// Đọc điều kiện tiến hóa hình ảnh
		foreach (XElement item4 in xElement.Element("VisualEvolutions").Elements("VisualEvolution"))
		{
			VisualEvolutions.Add(new MetaNarrationConditionsDefinition(item4.Element("Conditions")));
		}
	}
}
