using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Maths;
using UnityEngine;

namespace TheLastStand.Database;

public class TemplateDemoDatabase : Database<TemplateDemoDatabase>
{
	[SerializeField]
	private TextAsset demoTextAsset;

	public static Dictionary<string, TemplateDemoDefinition> DemoDefinitions { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		CLoggerManager.Log("Deserializing " + typeof(TemplateDemoDatabase).Name + ", make sure it is for testing purpose, because this prefab should NOT be in Database.", LogType.Warning);
		DemoDefinitions = new Dictionary<string, TemplateDemoDefinition>();
		DeserializeUsingGenericTopologicSortMethod();
		foreach (KeyValuePair<string, TemplateDemoDefinition> demoDefinition in DemoDefinitions)
		{
			CLoggerManager.Log(demoDefinition.Value.ToString());
		}
	}

	private void DeserializeUsingTwoStepsDeserialization()
	{
		foreach (XElement item in XDocument.Parse(demoTextAsset.text, LoadOptions.SetBaseUri).Element("TemplateDemoDefinitions").Elements("DemoDefinition"))
		{
			TemplateDemoDefinition templateDemoDefinition = new TemplateDemoDefinition(item);
			DemoDefinitions.Add(templateDemoDefinition.Id, templateDemoDefinition);
		}
		foreach (TemplateDemoDefinition item2 in TopologicSorter.Sort(DemoDefinitions.Values).ToList())
		{
			item2.DeserializeAfterTemplatesOrdering();
		}
	}

	private void DeserializeUsingGenericTopologicSortMethod()
	{
		IEnumerable<XElement> elements = XDocument.Parse(demoTextAsset.text, LoadOptions.SetBaseUri).Element("TemplateDemoDefinitions").Elements("DemoDefinition");
		foreach (XElement item in SortElementsByDependencies(elements))
		{
			TemplateDemoDefinition templateDemoDefinition = new TemplateDemoDefinition(item);
			DemoDefinitions.Add(templateDemoDefinition.Id, templateDemoDefinition);
		}
	}
}
