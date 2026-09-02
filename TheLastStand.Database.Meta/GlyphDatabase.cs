using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Meta.Glyphs;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database.Meta;

public class GlyphDatabase : Database<GlyphDatabase>
{
	[SerializeField]
	private TextAsset[] glyphDefinitions;

	public static Dictionary<string, GlyphDefinition> GlyphDefinitions { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		Queue<XElement> queue = GatherElements(glyphDefinitions, null, "GlyphDefinition");
		GlyphDefinitions = new Dictionary<string, GlyphDefinition>();
		while (queue.Count > 0)
		{
			GlyphDefinition glyphDefinition = new GlyphDefinition(queue.Dequeue());
			try
			{
				GlyphDefinitions.Add(glyphDefinition.Id, glyphDefinition);
			}
			catch (ArgumentException)
			{
				CLoggerManager.Log("Duplicate GlyphDefinition found for Id " + glyphDefinition.Id + ".", LogType.Error);
			}
		}
	}
}
