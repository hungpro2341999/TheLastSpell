using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Định nghĩa một câu thoại kịch bản (Meta Replica Definition) của nhân vật dẫn chuyện tại Oraculum.
/// </summary>
public class MetaReplicaDefinition : TheLastStand.Framework.Serialization.Definition
{
	/// <summary>
	/// Số lượng phản hồi/lời đáp của người chơi đối với câu thoại này (mặc định là 1).
	/// </summary>
	public int AnswersCount { get; private set; }

	/// <summary>
	/// Danh sách các Id câu thoại sẽ bị khóa (chặn không cho xuất hiện) sau khi câu thoại này được kích hoạt.
	/// </summary>
	public List<string> BlockReplicas { get; private set; }

	/// <summary>
	/// Bộ điều kiện tiên quyết cần thỏa mãn để câu thoại này có thể kích hoạt.
	/// </summary>
	public MetaNarrationConditionsDefinition ConditionsDefinition { get; private set; }

	/// <summary>
	/// Mã định danh duy nhất của câu thoại (Replica Id).
	/// </summary>
	public string Id { get; private set; }

	/// <summary>
	/// Cho biết câu thoại này có bắt buộc phải hiển thị ngay khi đủ điều kiện hay không.
	/// </summary>
	public bool Mandatory { get; private set; }

	public MetaReplicaDefinition(XContainer container)
		: base(container)
	{
	}

	/// <summary>
	/// Giải tuần tự hóa câu thoại từ XML.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		Id = xAttribute.Value;

		// Đọc số lượng câu trả lời
		XAttribute xAttribute2 = xElement.Attribute("AnswersCount");
		if (xAttribute2 != null)
		{
			if (!int.TryParse(xAttribute2.Value, out var result))
			{
				CLoggerManager.Log("Could not parse " + xAttribute2.Value + " to a valid int value for replica \"" + Id + "\".", LogType.Error);
				return;
			}
			AnswersCount = result;
		}
		else
		{
			AnswersCount = 1;
		}

		// Đọc danh sách các câu thoại bị chặn
		XElement xElement2 = xElement.Element("BlockReplicas");
		if (xElement2 != null)
		{
			BlockReplicas = new List<string>();
			foreach (XElement item in xElement2.Elements("ReplicaId"))
			{
				BlockReplicas.Add(item.Value);
			}
		}

		// Đọc cờ bắt buộc (Mandatory)
		XAttribute xAttribute3 = xElement.Attribute("Mandatory");
		if (xAttribute3 != null)
		{
			if (!bool.TryParse(xAttribute3.Value, out var result2))
			{
				CLoggerManager.Log("Could not parse Mandatory Attribute " + xAttribute3.Value + " to a valid boolean value for replica \"" + Id + "\".", LogType.Error);
				return;
			}
			Mandatory = result2;
		}

		// Đọc điều kiện kích hoạt
		XElement xElement3 = xElement.Element("Conditions");
		if (xElement3 != null)
		{
			ConditionsDefinition = new MetaNarrationConditionsDefinition(xElement3);
		}
	}
}
