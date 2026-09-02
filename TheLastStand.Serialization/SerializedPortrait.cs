using System;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedPortrait : ISerializedData
{
	public string Gender;

	public string FaceId;

	public string SkinColorPaletteId;

	public string HairColorPaletteId;

	public string EyesColorPaletteId;

	public string BackgroundColor;

	public string Code;
}
