using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Item;

/// <summary>
/// Bảng tra cứu xác suất Affix Level theo Item Level.
/// 
/// Khi sinh Affix cho item, hệ thống cần biết:
/// "Item level 3 thì nên random Affix level nào?"
/// → Tra bảng AffixLevelsProbas[3] → { Level1: 60%, Level2: 30%, Level3: 10% }
/// → Random theo weight → chọn Affix level.
/// 
/// Đồng thời tự động sinh AffixMalusLevelsProbas (bảng xác suất malus level)
/// bằng cách cast Affix level → E_MalusLevel (1=Small, 2=Medium, 3=Big).
/// 
/// Ví dụ XML:
/// <code>
/// &lt;AffixLevels&gt;
///   &lt;ItemLevel Id="0"&gt;
///     &lt;AffixLevelProba Id="1"&gt;70&lt;/AffixLevelProba&gt;
///     &lt;AffixLevelProba Id="2"&gt;25&lt;/AffixLevelProba&gt;
///     &lt;AffixLevelProba Id="3"&gt;5&lt;/AffixLevelProba&gt;
///   &lt;/ItemLevel&gt;
///   &lt;ItemLevel Id="5"&gt;
///     &lt;AffixLevelProba Id="1"&gt;30&lt;/AffixLevelProba&gt;
///     &lt;AffixLevelProba Id="2"&gt;40&lt;/AffixLevelProba&gt;
///     &lt;AffixLevelProba Id="3"&gt;30&lt;/AffixLevelProba&gt;
///   &lt;/ItemLevel&gt;
/// &lt;/AffixLevels&gt;
/// </code>
/// </summary>
public class AffixLevelsDefinition : TheLastStand.Framework.Serialization.Definition
{
	/// <summary>
	/// Bảng xác suất Affix Level theo Item Level.
	/// Key ngoài = Item Level (0-10).
	/// Key trong = Affix Level (1-3).
	/// Value = xác suất (weight) để random.
	/// 
	/// Ví dụ: AffixLevelsProbas[3] = { 1: 60, 2: 30, 3: 10 }
	/// → Item level 3: 60% Affix level 1, 30% level 2, 10% level 3.
	/// </summary>
	public Dictionary<int, Dictionary<int, float>> AffixLevelsProbas { get; private set; } = new Dictionary<int, Dictionary<int, float>>();

	/// <summary>
	/// Bảng xác suất Malus Level theo Item Level.
	/// Tự động sinh từ AffixLevelsProbas bằng cách cast int → E_MalusLevel.
	/// (1 → Small, 2 → Medium, 3 → Big)
	/// 
	/// Dùng trong ApplyMaluses() để random mức độ phạt.
	/// </summary>
	public Dictionary<int, Dictionary<AffixMalusDefinition.E_MalusLevel, float>> AffixMalusLevelsProbas { get; private set; } = new Dictionary<int, Dictionary<AffixMalusDefinition.E_MalusLevel, float>>();

	public AffixLevelsDefinition(XContainer container)
		: base(container)
	{
	}

	/// <summary>
	/// Đọc bảng xác suất từ XML.
	/// Bước 1: Đọc AffixLevelsProbas từ các element ItemLevel/AffixLevelProba.
	/// Bước 2: Tự động sinh AffixMalusLevelsProbas bằng cách cast key → E_MalusLevel.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		// Bước 1: Đọc AffixLevelsProbas
		foreach (XElement item in (container as XElement).Elements("ItemLevel"))
		{
			XAttribute xAttribute = item.Attribute("Id");
			if (xAttribute.IsNullOrEmpty())
			{
				Debug.LogError("AffixLevelsDefinition ItemLevel must have an Id");
				continue;
			}
			if (!int.TryParse(xAttribute.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
			{
				CLoggerManager.Log("AffixLevelsDefinition ItemLevel must have a valid Id (int)", LogType.Error);
				continue;
			}
			if (AffixLevelsProbas.ContainsKey(result))
			{
				CLoggerManager.Log($"AffixLevelsDefinition already have this Id : {result}", LogType.Error);
				continue;
			}
			AffixLevelsProbas.Add(result, new Dictionary<int, float>());
			// Đọc từng AffixLevelProba bên trong ItemLevel
			foreach (XElement item2 in item.Elements("AffixLevelProba"))
			{
				XAttribute xAttribute2 = item2.Attribute("Id");
				if (xAttribute2.IsNullOrEmpty())
				{
					CLoggerManager.Log($"AffixLevelsDefinition {result}'s AffixLevelProba must have an Id", LogType.Error);
					continue;
				}
				if (!int.TryParse(xAttribute2.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result2) && result2 > 0 && result2 < 4)
				{
					CLoggerManager.Log($"AffixLevelsDefinition {result}'s AffixLevelProba {HasAnInvalidInt(xAttribute2.Value)}", LogType.Error);
					continue;
				}
				if (AffixLevelsProbas[result].ContainsKey(result2))
				{
					CLoggerManager.Log($"AffixLevelsDefinition ItemLevel with id {result} already have an AffixLevelProba this Id : {result2}", LogType.Error);
					continue;
				}
				if (item2.IsEmpty || !float.TryParse(item2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result3))
				{
					CLoggerManager.Log("AffixLevelsDefinition AffixLevelProba must be a valid float", LogType.Error);
					return;
				}
				AffixLevelsProbas[result].Add(result2, result3);
			}
		}
		// Bước 2: Tự động sinh AffixMalusLevelsProbas từ AffixLevelsProbas
		// Cast: Affix Level 1 → E_MalusLevel.Small, 2 → Medium, 3 → Big
		foreach (KeyValuePair<int, Dictionary<int, float>> affixLevelsProba in AffixLevelsProbas)
		{
			AffixMalusLevelsProbas.Add(affixLevelsProba.Key, new Dictionary<AffixMalusDefinition.E_MalusLevel, float>(AffixMalusDefinition.SharedMalusLevelComparer));
			foreach (KeyValuePair<int, float> item3 in affixLevelsProba.Value)
			{
				AffixMalusLevelsProbas[affixLevelsProba.Key].Add((AffixMalusDefinition.E_MalusLevel)item3.Key, item3.Value);
			}
		}
	}
}
