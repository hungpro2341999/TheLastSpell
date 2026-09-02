using System;
using System.Xml.Serialization;
using TheLastStand.Model.Meta;

namespace TheLastStand.Serialization.Meta;

[Serializable]
public class SerializedMetaCondition : ISerializedData
{
	[XmlAttribute]
	public MetaCondition.E_MetaConditionCategory Category;

	[XmlAttribute]
	public string MetaUpgradeId;

	[XmlAttribute]
	public string Id;

	public SerializedMetaConditionsContext LocalContext;

	[XmlAttribute]
	public int OccurenceProgression;

	[XmlAttribute]
	public bool HasBeenChecked;
}
