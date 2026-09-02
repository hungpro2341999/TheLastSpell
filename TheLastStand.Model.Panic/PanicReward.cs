using TPLib;
using TheLastStand.Controller.Panic;
using TheLastStand.Manager;
using TheLastStand.Model.Item;

namespace TheLastStand.Model.Panic;

public class PanicReward
{
	private int baseNbRerollReward;

	public int BaseNbRerollReward
	{
		get
		{
			return baseNbRerollReward;
		}
		set
		{
			if (BaseNbRerollReward != value)
			{
				RemainingNbRerollReward += value - BaseNbRerollReward;
				baseNbRerollReward = value;
			}
		}
	}

	public int DamnedSoulsRerollCost => TPSingleton<SinkManager>.Instance.ItemRewardSinkData.GetFinalPrice();

	public int Gold { get; set; }

	public bool HasAtLeastOneItem
	{
		get
		{
			if (Items != null)
			{
				return ItemsCount > 0;
			}
			return false;
		}
	}

	public TheLastStand.Model.Item.Item[] Items { get; set; }

	public int ItemsCount { get; set; }

	public int Materials { get; set; }

	public Panic Panic { get; }

	public PanicRewardController PanicRewardController { get; }

	public int RemainingNbRerollReward { get; set; }

	public PanicReward(Panic panic, PanicRewardController panicRewardController)
	{
		Panic = panic;
		PanicRewardController = panicRewardController;
	}
}
