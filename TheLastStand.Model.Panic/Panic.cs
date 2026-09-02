using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Panic;
using TheLastStand.Definition.Panic;
using TheLastStand.Manager;
using TheLastStand.View.Panic;

namespace TheLastStand.Model.Panic;

public class Panic
{
	public int ExpectedLevel
	{
		get
		{
			for (int num = PanicDefinition.PanicLevelDefinitions.Length - 1; num >= 0; num--)
			{
				if (ExpectedValue >= PanicDefinition.PanicLevelDefinitions[num].PanicValueNeeded)
				{
					return num;
				}
			}
			TPSingleton<PanicManager>.Instance.LogError($"Level not found with value {Value}", CLogLevel.DETAILED);
			return -1;
		}
	}

	public int Level
	{
		get
		{
			for (int num = PanicDefinition.PanicLevelDefinitions.Length - 1; num >= 0; num--)
			{
				if (Value >= PanicDefinition.PanicLevelDefinitions[num].PanicValueNeeded)
				{
					return num;
				}
			}
			TPSingleton<PanicManager>.Instance.LogError($"Level not found with value {Value}", CLogLevel.DETAILED);
			return -1;
		}
	}

	public PanicController PanicController { get; }

	public PanicDefinition PanicDefinition { get; }

	public PanicEvalGoldContext PanicEvalGoldContext { get; } = new PanicEvalGoldContext();

	public PanicEvalMaterialContext PanicEvalMaterialContext { get; } = new PanicEvalMaterialContext();

	public PanicReward PanicReward { get; set; }

	public PanicView PanicView { get; }

	public float ExpectedValue { get; set; }

	public float Value { get; set; }

	public bool IsAtMaxValue => Value >= PanicDefinition.ValueMax;

	public Panic(PanicDefinition panicDefinition, PanicController panicController, PanicView panicView)
	{
		PanicDefinition = panicDefinition;
		PanicController = panicController;
		PanicView = panicView;
		PanicView.Panic = this;
	}
}
