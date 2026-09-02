using System.Collections.Generic;

namespace TheLastStand.Serialization.Meta;

public class SerializedGlyphData : ISerializedData
{
	public string GlyphId;

	public List<SerializedMaxApoPassed> MaxApoPassedByCity;
}
