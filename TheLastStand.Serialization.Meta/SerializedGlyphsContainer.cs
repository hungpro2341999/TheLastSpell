using System;
using System.Collections.Generic;

namespace TheLastStand.Serialization.Meta;

[Serializable]
public class SerializedGlyphsContainer : ISerializedData
{
	public List<SerializedModifyBuildingActionsCostGlyph> SerializedModifyBuildingActionsCostGlyphs = new List<SerializedModifyBuildingActionsCostGlyph>();
}
