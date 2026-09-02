using System.Collections.Generic;
using System.Xml.Serialization;
using TheLastStand.Serialization.Apocalypse;

namespace TheLastStand.Serialization;

public class SerializedCity : ISerializedData
{
	[XmlAttribute]
	public string Id;

	public int MaxApoPassed;

	public int MaxNightReached;

	public int NumberOfRuns;

	public int NumberOfWins;

	public bool CustomModeEnabled;

	public List<string> SelectedGlyphs;

	public List<SerializedApocalypseModifierStep> CompletedModifiersSteps;
}
