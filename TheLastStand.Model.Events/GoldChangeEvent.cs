using TheLastStand.Framework.EventSystem;

namespace TheLastStand.Model.Events;

public class GoldChangeEvent : Event
{
	public int Gold { get; private set; }

	public GoldChangeEvent(int gold)
	{
		Gold = gold;
	}
}
