using TheLastStand.Framework.EventSystem;
using TheLastStand.Model.Building;

namespace TheLastStand.Model.Events;

public class BuildingDestroyedEvent : Event
{
	public TheLastStand.Model.Building.Building Building { get; private set; }

	public string Id => Building.Id;

	public BuildingDestroyedEvent(TheLastStand.Model.Building.Building building)
	{
		Building = building;
	}
}
