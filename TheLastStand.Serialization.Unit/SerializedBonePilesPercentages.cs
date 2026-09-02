using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace TheLastStand.Serialization.Unit;

public class SerializedBonePilesPercentages : ISerializedData
{
	[Serializable]
	public struct Percentage
	{
		public string Id;

		public int Value;
	}

	[XmlAttribute]
	public int TileX;

	[XmlAttribute]
	public int TileY;

	public List<Percentage> Percentages = new List<Percentage>();
}
