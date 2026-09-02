using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Serialization;
using TheLastStand.View;

namespace TheLastStand.Model.Unit;

public class BodyPart : ILegacySerializable, ILegacyDeserializable
{
	public HashSet<string> AdditionalConstraints { get; private set; } = new HashSet<string>();

	public BodyPartDefinition BodyPartDefinition { get; private set; }

	public BodyPartDefinition BodyPartDefinitionOverride { get; set; }

	private BodyPartView BodyPartViewBack { get; set; }

	private BodyPartView BodyPartViewFront { get; set; }

	public BodyPart(XContainer container)
	{
		Deserialize(container);
	}

	public BodyPart(BodyPartDefinition definition, BodyPartView viewFront = null, BodyPartView viewBack = null)
	{
		BodyPartDefinition = definition;
		BodyPartViewFront = viewFront;
		BodyPartViewBack = viewBack;
	}

	public void ChangeAdditionalConstraint(string constraintId, bool add)
	{
		if ((add && AdditionalConstraints.Add(constraintId)) || AdditionalConstraints.Remove(constraintId))
		{
			if (BodyPartViewFront != null)
			{
				BodyPartViewFront.IsDirty = true;
			}
			if (BodyPartViewBack != null)
			{
				BodyPartViewBack.IsDirty = true;
			}
		}
	}

	public virtual void Deserialize(XContainer container = null)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		BodyPartDefinition = PlayableUnitDatabase.PlayableUnitNakedBodyPartsDefinitions[xAttribute.Value];
		if (xElement.Element("BodyPartDefinitionOverride") != null)
		{
			XElement xElement2 = xElement.Element("BodyPartDefinitionOverride");
			if (xElement2.HasAttributes)
			{
				XAttribute xAttribute2 = xElement2.Attribute("Id");
				BodyPartDefinitionOverride = PlayableUnitDatabase.PlayableUnitNakedBodyPartsDefinitions[xAttribute2.Value];
			}
		}
		foreach (XElement item in xElement.Elements("AdditionalConstraints"))
		{
			if (item.HasAttributes)
			{
				AdditionalConstraints.Add(item.Attribute("Value").Value);
			}
		}
	}

	public BodyPartView GetBodyPartView(BodyPartDefinition.E_Orientation orientation)
	{
		if (orientation != BodyPartDefinition.E_Orientation.Back)
		{
			return BodyPartViewFront;
		}
		return BodyPartViewBack;
	}

	public string GetSpritePath(string faceId, string gender, BodyPartDefinition.E_Orientation orientation)
	{
		string text = null;
		text = BodyPartDefinitionOverride?.GetSpritePath(faceId, gender, orientation);
		if (string.IsNullOrEmpty(text))
		{
			text = BodyPartDefinition?.GetSpritePath(faceId, gender, orientation);
		}
		return text;
	}

	public virtual XContainer Serialize()
	{
		XElement xElement = new XElement("BodyPart");
		xElement.Add(new XAttribute("Id", BodyPartDefinition.Id));
		if (BodyPartDefinitionOverride != null)
		{
			xElement.Add(new XElement("BodyPartDefinitionOverride", new XAttribute("Id", BodyPartDefinitionOverride.Id)));
		}
		XElement xElement2 = new XElement("AdditionalConstraints");
		foreach (string additionalConstraint in AdditionalConstraints)
		{
			xElement2.Add(new XElement(additionalConstraint));
		}
		xElement.Add(xElement2);
		return xElement;
	}

	public void SetBodyPartView(BodyPartDefinition.E_Orientation orientation, BodyPartView bodyPartView)
	{
		if ((orientation & BodyPartDefinition.E_Orientation.Front) == BodyPartDefinition.E_Orientation.Front)
		{
			BodyPartViewFront = bodyPartView;
		}
		else if ((orientation & BodyPartDefinition.E_Orientation.Back) == BodyPartDefinition.E_Orientation.Back)
		{
			BodyPartViewBack = bodyPartView;
		}
	}
}
