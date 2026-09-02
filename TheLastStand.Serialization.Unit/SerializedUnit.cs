using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using TheLastStand.Definition;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Serialization.Unit;

[Serializable]
public class SerializedUnit : ISerializedData
{
	[XmlAttribute]
	public int RandomId;

	public SerializableVector2Int? Position;

	public List<SerializedUnitStatus> Status = new List<SerializedUnitStatus>();

	public GameDefinition.E_Direction LookDirection;

	public AttackSkillActionDefinition.E_AttackType LastSkillType;
}
