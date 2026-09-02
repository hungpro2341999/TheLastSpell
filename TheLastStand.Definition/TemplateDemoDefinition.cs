using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using TheLastStand.Database;
using TheLastStand.Framework.Maths;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition;

public class TemplateDemoDefinition : TheLastStand.Framework.Serialization.Definition, ITopologicSortItem<TemplateDemoDefinition>
{
	private XElement demoElement;

	public string Id { get; private set; }

	public string TemplateId { get; private set; }

	public float TestFloat { get; private set; }

	public int TestInt { get; private set; }

	public string TestString { get; private set; }

	public TemplateDemoDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		DeserializeBeforeOrdering(container);
	}

	public void DeserializeBeforeOrdering(XContainer container)
	{
		demoElement = container as XElement;
		Id = demoElement.Attribute("Id").Value;
		TemplateId = demoElement.Attribute("TemplateId")?.Value ?? string.Empty;
	}

	public void DeserializeAfterTemplatesOrdering()
	{
		TemplateDemoDefinition templateDemoDefinition = ((!string.IsNullOrEmpty(TemplateId)) ? TemplateDemoDatabase.DemoDefinitions[TemplateId] : null);
		XElement xElement = demoElement.Element("TestInt");
		TestInt = ((xElement != null) ? int.Parse(xElement.Value) : templateDemoDefinition.TestInt);
		TestString = demoElement.Element("TestString")?.Value ?? templateDemoDefinition.TestString;
		XElement xElement2 = demoElement.Element("TestSequence")?.Element("TestFloat");
		TestFloat = ((xElement2 != null) ? float.Parse(xElement2.Value, NumberStyles.Float, CultureInfo.InvariantCulture) : templateDemoDefinition.TestFloat);
	}

	public IEnumerable<TemplateDemoDefinition> GetDependencies()
	{
		return TemplateDemoDatabase.DemoDefinitions.Values.Where((TemplateDemoDefinition o) => o.Id == TemplateId);
	}

	public override string ToString()
	{
		return $"{typeof(TemplateDemoDefinition).Name} Id {Id} : [{TestInt},{TestString},{TestFloat}]";
	}
}
