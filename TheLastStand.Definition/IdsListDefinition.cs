using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TheLastStand.Database;
using TheLastStand.Framework.Maths;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition;

public class IdsListDefinition : TheLastStand.Framework.Serialization.Definition, ITopologicSortItem<IdsListDefinition>
{
	private XElement idsListElement;

	public string Id { get; private set; }

	public List<string> Ids { get; private set; } = new List<string>();

	public List<string> IncludedListsIds { get; private set; } = new List<string>();

	public IdsListDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		idsListElement = container as XElement;
		XAttribute xAttribute = idsListElement.Attribute("Id");
		Id = xAttribute.Value;
		foreach (XElement item in idsListElement.Elements("IncludeList"))
		{
			IncludedListsIds.Add(item.Value);
		}
	}

	public void DeserializeAfterDependencySorting()
	{
		foreach (XElement item in idsListElement.Elements("IncludeList"))
		{
			foreach (string id in GenericDatabase.IdsListDefinitions[item.Value].Ids)
			{
				if (!Ids.Contains(id))
				{
					Ids.Add(id);
				}
			}
		}
		foreach (XElement item2 in idsListElement.Elements("Id"))
		{
			if (!Ids.Contains(item2.Value))
			{
				Ids.Add(item2.Value);
			}
		}
	}

	public IEnumerable<IdsListDefinition> GetDependencies()
	{
		return GenericDatabase.IdsListDefinitions.Values.Where((IdsListDefinition o) => IncludedListsIds.Contains(o.Id));
	}

	public override string ToString()
	{
		return GetType().Name + " Id=" + Id;
	}
}
