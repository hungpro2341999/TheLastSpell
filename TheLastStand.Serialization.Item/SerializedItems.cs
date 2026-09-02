using System;
using System.Collections.Generic;

namespace TheLastStand.Serialization.Item;

[Serializable]
public class SerializedItems : List<SerializedItem>, ISerializedData
{
}
