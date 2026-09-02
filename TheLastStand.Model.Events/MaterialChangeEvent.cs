using TheLastStand.Framework.EventSystem;

namespace TheLastStand.Model.Events;

public class MaterialChangeEvent : Event
{
	public int Material { get; private set; }

	public MaterialChangeEvent(int materials)
	{
		Material = materials;
	}
}
