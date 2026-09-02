using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using TheLastStand.Definition.Unit;
using TheLastStand.Model.Unit;

namespace TheLastStand.Serialization.Unit;

[Serializable]
public class SerializedLevelUpBonuses : ISerializedData
{
	public class SerializedLevelUpPotentialBonus
	{
		[XmlAttribute]
		public UnitStatDefinition.E_Stat Stat;

		[XmlAttribute]
		public UnitLevelUp.E_StatLevelUpRarity BonusIndex;
	}

	public int CommonNbReroll;

	public int MainNbReroll;

	public int SecondaryNbReroll;

	public int SinkNbReroll;

	public List<SerializedLevelUpPotentialBonus> AvailableMainBonuses = new List<SerializedLevelUpPotentialBonus>();

	public List<SerializedLevelUpPotentialBonus> AvailableSecondaryBonuses = new List<SerializedLevelUpPotentialBonus>();
}
