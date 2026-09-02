using System.Collections.Generic;

namespace TheLastStand.Model;

public interface IFilterable
{
	HashSet<string> FilterIds { get; }
}
