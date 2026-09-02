using TheLastStand.Framework.EventSystem;

namespace TheLastStand.Model.Events;

public class BuildingConstructedEvent : Event
{
	public string Id { get; private set; }

	public BuildingConstructedEvent(string id)
	{
		Id = id;
	}
}
