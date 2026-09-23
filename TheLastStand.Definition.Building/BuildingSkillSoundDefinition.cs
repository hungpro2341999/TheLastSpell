using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Building;

/// <summary>
/// Định nghĩa âm thanh khi công trình thi triển kỹ năng (Building Skill Sound).
/// Quản lý danh sách âm thanh, độ trễ và số âm thanh tối đa phát đồng thời.
/// </summary>
public class BuildingSkillSoundDefinition : TheLastStand.Framework.Serialization.Definition
{
	#region Properties

	/// <summary>
	/// Danh sách ID các âm thanh kỹ năng.
	/// </summary>
	public List<string> SoundsIds = new List<string>();

	/// <summary>
	/// Độ trễ khi phát âm thanh (dưới dạng cây biểu thức Node).
	/// </summary>
	public Node Delay { get; private set; }

	/// <summary>
	/// ID template công trình liên quan.
	/// </summary>
	public string BuildingTemplateDefinitionId { get; private set; }

	/// <summary>
	/// Số lượng âm thanh phát đồng thời tối đa.
	/// </summary>
	public int MaximumSimultaneousSounds { get; private set; }

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa âm thanh kỹ năng công trình từ dữ liệu XML.
	/// </summary>
	public BuildingSkillSoundDefinition(XContainer container)
		: base(container)
	{
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Đọc và giải mã dữ liệu XML (Deserialize) cho cấu hình âm thanh kỹ năng công trình.
	/// </summary>
	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XElement xElement = obj.Element("BuildingTemplateDefinitionId");
		XElement xElement2 = obj.Element("MaximumSimultaneousSounds");
		XElement xElement3 = obj.Element("Sounds");
		if (xElement != null && xElement.Attribute("Value") != null)
		{
			BuildingTemplateDefinitionId = xElement.Attribute("Value").Value;
		}
		if (xElement2 != null)
		{
			if (!int.TryParse(xElement2.Value, out var result))
			{
				CLoggerManager.Log("MaximumSimultaneousSounds should be of type int !");
				return;
			}
			MaximumSimultaneousSounds = result;
		}
		if (xElement3 == null)
		{
			return;
		}
		foreach (XElement item in xElement3.Elements("SoundId"))
		{
			if (item.Attribute("Value") != null)
			{
				SoundsIds.Add(item.Attribute("Value").Value);
			}
		}
		XAttribute xAttribute = xElement3.Attribute("Delay");
		Delay = ((xAttribute != null) ? Parser.Parse(xAttribute.Value) : new NodeNumber(0.0));
	}

	#endregion
}

