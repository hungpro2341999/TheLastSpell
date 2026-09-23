using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

/// <summary>
/// Định nghĩa điều kiện Meta (Meta Condition Definition).
/// <para>Dùng để xác định điều kiện cần thỏa mãn để hiển thị/mở khóa hoặc kích hoạt một nâng cấp trong Oraculum.</para>
/// </summary>
public class MetaConditionDefinition : TheLastStand.Framework.Serialization.Definition
{
	/// <summary>
	/// Danh sách các đối số truyền vào cho hàm kiểm tra điều kiện.
	/// <para>Được parse tự động từ các thuộc tính XML có tên từ 'A' đến 'Z'.</para>
	/// </summary>
	public List<string> Arguments { get; private set; } = new List<string>();

	/// <summary>
	/// Chỉ số (index) của nhóm điều kiện mà điều kiện này thuộc về.
	/// </summary>
	public int ConditionsGroupIndex { get; }

	/// <summary>
	/// Xác định điều kiện này có bị ẩn khỏi giao diện người chơi hay không.
	/// </summary>
	public bool Hidden { get; }

	/// <summary>
	/// Key định danh chuỗi bản địa hóa (localization) để hiển thị mô tả điều kiện trên UI.
	/// </summary>
	public string LocalizationKey { get; private set; }

	/// <summary>
	/// Tên của hàm kiểm tra điều kiện (tương ứng với thẻ XML, ví dụ: "SurviveNight", "KillEnemies"...).
	/// </summary>
	public string Name { get; private set; }

	/// <summary>
	/// Số lần mục tiêu cần đạt được để hoàn thành điều kiện (mặc định là 1).
	/// </summary>
	public int Occurences { get; private set; } = 1;

	/// <summary>
	/// Khởi tạo định nghĩa điều kiện với cờ ẩn và chỉ số nhóm điều kiện.
	/// </summary>
	public MetaConditionDefinition(XContainer container, bool hidden, int conditionsGroupIndex)
		: base(container)
	{
		ConditionsGroupIndex = conditionsGroupIndex;
		Hidden = hidden;
	}

	/// <summary>
	/// Đọc và kiểm tra tính hợp lệ của điều kiện từ thẻ XML.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		XElement metaConditionElement = container as XElement;
		Name = metaConditionElement.Name.LocalName;

		// Kiểm tra hàm điều kiện có được đăng ký trong thư viện MetaConditionManager hay không
		if (!TPSingleton<MetaConditionManager>.Instance.ConditionsLibrary.ContainsKey(Name))
		{
			throw new Exception("Invalid condition function " + Name + " - Please pick a function in the following list: " + string.Join(", ", TPSingleton<MetaConditionManager>.Instance.ConditionsLibrary.Keys));
		}

		// Đọc số lần yêu cầu (Occurences)
		XAttribute xAttribute = metaConditionElement.Attribute("Occurences");
		if (xAttribute != null)
		{
			if (int.TryParse(xAttribute.Value, out var result))
			{
				Occurences = result;
			}
			else
			{
				CLoggerManager.Log("Occurences attribute has an invalid value " + xAttribute.Value + ". Setting it to 1.", LogType.Error);
			}
		}

		LocalizationKey = metaConditionElement.Attribute("LocalizationKey")?.Value;

		// Đọc các tham số định danh theo bảng chữ cái A-Z (ASCII 65 -> 90)
		Arguments = (from index in Enumerable.Range(65, 26)
			select metaConditionElement.Attribute(((char)index).ToString())?.Value.Trim() into value
			where value != null
			select value).ToList();
	}

	public override string ToString()
	{
		return string.Format("{0} ({1}) (Occurences={2})", Name, string.Join(", ", Arguments), Occurences);
	}
}
