using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Building;

/// <summary>
/// Định nghĩa các hướng sinh công trình ngẫu nhiên (Random Buildings Directions).
/// Ánh xạ từng hướng (N, S, E, W, v.v.) tới một định nghĩa sinh công trình ngẫu nhiên tương ứng.
/// </summary>
public class RandomBuildingsDirectionsDefinition : TheLastStand.Framework.Serialization.Definition
{
	#region Properties

	/// <summary>
	/// Mã ID định danh của định nghĩa hướng sinh công trình ngẫu nhiên.
	/// </summary>
	public string Id { get; private set; }

	/// <summary>
	/// Cờ vô hiệu hóa xoay hướng sinh.
	/// </summary>
	public bool DisableRotation { get; private set; }

	/// <summary>
	/// Từ điển ánh xạ hướng (E_Direction) tới định nghĩa sinh công trình ngẫu nhiên (RandomBuildingsGenerationDefinition).
	/// </summary>
	public Dictionary<SpawnDirectionsDefinition.E_Direction, RandomBuildingsGenerationDefinition> GenerationDefinitionByDirection { get; } = new Dictionary<SpawnDirectionsDefinition.E_Direction, RandomBuildingsGenerationDefinition>();

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa hướng sinh công trình ngẫu nhiên từ dữ liệu XML.
	/// </summary>
	public RandomBuildingsDirectionsDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Đọc và giải mã dữ liệu XML (Deserialize) cho cấu hình sinh công trình theo hướng.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		Id = xAttribute.Value;
		DisableRotation = xElement.Element("DisableRotation") != null;
		foreach (XElement item in xElement.Elements())
		{
			if (Enum.TryParse<SpawnDirectionsDefinition.E_Direction>(item.Name.LocalName, out var result))
			{
				RandomBuildingsGenerationDefinition value = BuildingDatabase.RandomBuildingsGenerationDefinitions[item.Value];
				GenerationDefinitionByDirection.Add(result, value);
			}
		}
	}

	#endregion
}

