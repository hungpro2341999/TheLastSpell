using TheLastStand.Framework.EventSystem;

namespace TheLastStand.Model.Events;

public class WorkersChangeEvent : Event
{
	public int Workers { get; private set; }

	public int MaxWorkers { get; private set; }

	public WorkersChangeEvent(int workers, int maxWorkers)
	{
		Workers = workers;
		MaxWorkers = maxWorkers;
	}
}
