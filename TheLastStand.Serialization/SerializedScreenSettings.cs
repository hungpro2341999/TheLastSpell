using System;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedScreenSettings : ISerializedData
{
	public SerializableResolution Resolution;

	public bool IsCursorRestricted;

	public bool RunInBackground;

	public int MonitorIndex;

	public float ScreenShakes;

	public float UISize;

	public SettingsManager.E_WindowMode WindowMode;

	public bool VSync;

	public int FrameRateCap;
}
