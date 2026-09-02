using System.Collections.Generic;
using TheLastStand.Manager.WorldMap;

namespace TheLastStand.Serialization;

public class SerializedCities : ISerializedData
{
	public string SelectedCityId;

	public List<SerializedCity> Cities = new List<SerializedCity>();

	public LastRunInfo LastRunInfo;
}
