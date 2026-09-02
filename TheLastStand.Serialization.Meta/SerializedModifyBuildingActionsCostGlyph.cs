using System;
using System.Xml.Serialization;

namespace TheLastStand.Serialization.Meta;

[Serializable]
public class SerializedModifyBuildingActionsCostGlyph : ISerializedData
{
	[XmlAttribute]
	public string GlyphParentId;

	[XmlAttribute]
	public int EffectIndex;

	[XmlAttribute]
	public int CurrentModifiersDailyLimit;
}
