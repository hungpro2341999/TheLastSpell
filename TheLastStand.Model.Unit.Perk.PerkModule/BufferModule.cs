using TPLib.Log;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Serialization.Perk;
using UnityEngine;

namespace TheLastStand.Model.Unit.Perk.PerkModule;

public class BufferModule : APerkModule
{
	public BufferModuleDefinition BufferModuleDefinition => base.PerkModuleDefinition as BufferModuleDefinition;

	public int Buffer { get; set; }

	public int Buffer2 { get; set; }

	public int Buffer3 { get; set; }

	public int Buffer4 { get; set; }

	public int Buffer5 { get; set; }

	public int Buffer6 { get; set; }

	public int Buffer7 { get; set; }

	public int Buffer8 { get; set; }

	public int Buffer9 { get; set; }

	public BufferModule(BufferModuleDefinition perkModuleDefinition, Perk perk)
		: base(perkModuleDefinition, perk)
	{
		Buffer = BufferModuleDefinition.DefaultBufferValue;
	}

	public int GetBuffer(double bufferIndex)
	{
		if (bufferIndex != 1.0)
		{
			if (bufferIndex != 2.0)
			{
				if (bufferIndex != 3.0)
				{
					if (bufferIndex != 4.0)
					{
						if (bufferIndex != 5.0)
						{
							if (bufferIndex != 6.0)
							{
								if (bufferIndex != 7.0)
								{
									if (bufferIndex != 8.0)
									{
										if (bufferIndex == 9.0)
										{
											return Buffer9;
										}
										CLoggerManager.Log($"Unable to GetBuffer buffer: {bufferIndex}. Used the default Buffer instead.", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "Perk");
										return Buffer;
									}
									return Buffer8;
								}
								return Buffer7;
							}
							return Buffer6;
						}
						return Buffer5;
					}
					return Buffer4;
				}
				return Buffer3;
			}
			return Buffer2;
		}
		return Buffer;
	}

	public override void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedBufferModule serializedBufferModule = container as SerializedBufferModule;
		Buffer = serializedBufferModule.Buffer;
		Buffer2 = serializedBufferModule.Buffer2;
		Buffer3 = serializedBufferModule.Buffer3;
		Buffer4 = serializedBufferModule.Buffer4;
		Buffer5 = serializedBufferModule.Buffer5;
		Buffer6 = serializedBufferModule.Buffer6;
		Buffer7 = serializedBufferModule.Buffer7;
		Buffer8 = serializedBufferModule.Buffer8;
		Buffer9 = serializedBufferModule.Buffer9;
	}

	public override ISerializedData Serialize()
	{
		return new SerializedBufferModule
		{
			Buffer = Buffer,
			Buffer2 = Buffer2,
			Buffer3 = Buffer3,
			Buffer4 = Buffer4,
			Buffer5 = Buffer5,
			Buffer6 = Buffer6,
			Buffer7 = Buffer7,
			Buffer8 = Buffer8,
			Buffer9 = Buffer9
		};
	}

	public override void ResetDynamicData()
	{
		base.ResetDynamicData();
		Buffer = BufferModuleDefinition.DefaultBufferValue;
		Buffer2 = 0;
		Buffer3 = 0;
		Buffer4 = 0;
		Buffer5 = 0;
		Buffer6 = 0;
		Buffer7 = 0;
		Buffer8 = 0;
		Buffer9 = 0;
	}
}
