using System.Globalization;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.CastFx;

public class SkillCastFxDefinition : CastFxDefinition
{
	public class CasterAnimDefinition : TheLastStand.Framework.Serialization.Definition
	{
		public Node Delay { get; private set; }

		public string Path { get; private set; } = "Hero_CastSkill_Default_";

		public CasterAnimDefinition(XContainer container)
			: base(container)
		{
		}

		public CasterAnimDefinition()
			: this(null)
		{
		}

		public override void Deserialize(XContainer container)
		{
			XElement xElement = container?.Element("Path");
			if (xElement != null)
			{
				Path = xElement.Value;
			}
			XElement xElement2 = container?.Element("Delay");
			Delay = ((xElement2 != null) ? Parser.Parse(xElement2.Value) : new NodeNumber(0.0));
		}
	}

	public CasterAnimDefinition CasterAnimDef { get; private set; }

	public ManeuverFxDefinition ManeuverFxDefinition { get; private set; }

	public FollowFxDefinition FollowFxDefinition { get; private set; }

	public float MultiHitDelay { get; private set; }

	public float PropagationDelay { get; private set; }

	public float RepeatDelay { get; private set; }

	public SkillCastFxDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		base.Deserialize(xContainer);
		if (xContainer is XElement xElement)
		{
			XAttribute xAttribute = xElement.Attribute("MultiHitDelay");
			MultiHitDelay = ((xAttribute != null) ? float.Parse(xAttribute.Value, CultureInfo.InvariantCulture) : 0f);
			XAttribute xAttribute2 = xElement.Attribute("RepeatDelay");
			RepeatDelay = ((xAttribute2 != null) ? float.Parse(xAttribute2.Value, CultureInfo.InvariantCulture) : 0f);
			XAttribute xAttribute3 = xElement.Attribute("PropagationDelay");
			PropagationDelay = ((xAttribute3 != null) ? float.Parse(xAttribute3.Value, CultureInfo.InvariantCulture) : 0f);
			XElement xElement2 = xElement.Element("ManeuverFx");
			if (xElement2 != null)
			{
				ManeuverFxDefinition = new ManeuverFxDefinition(xElement2);
			}
			XElement xElement3 = xElement.Element("FollowFx");
			if (xElement3 != null)
			{
				FollowFxDefinition = new FollowFxDefinition(xElement3);
			}
			XElement xElement4 = xElement.Element("CasterAnim");
			if (xElement4 != null)
			{
				CasterAnimDef = new CasterAnimDefinition(xElement4);
			}
		}
		if (CasterAnimDef == null)
		{
			CasterAnimDef = new CasterAnimDefinition();
		}
	}
}
