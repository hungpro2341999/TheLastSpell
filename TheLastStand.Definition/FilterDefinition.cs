using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Localization;
using TheLastStand.Database;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition;

public class FilterDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Constants
	{
		public const string ApocalypseFilterIconPath = "View/Sprites/UI/Filters/Apocalypse/";

		public const string ApocalypseOffFilterIconNameFormat = "Icons_Apocalypse_{0}_Modifier_Off";

		public const string ApocalypseOnFilterIconNameFormat = "Icons_Apocalypse_{0}_Modifier_On";

		public const string FilterIconPath = "View/Sprites/UI/Filters/";

		public const string FilterIconNameFormat = "Icons_Filters_{0}_On";
	}

	private HashSet<string> includedFiltersIds;

	private Sprite cachedFilterSprite;

	private Sprite apocalypseOffCachedFilterSprite;

	private Sprite apocalypseOnCachedFilterSprite;

	public Sprite ApocalypseOffFilterSprite
	{
		get
		{
			if ((object)apocalypseOffCachedFilterSprite == null)
			{
				apocalypseOffCachedFilterSprite = GetApocalypseIcon(isOn: false);
			}
			return apocalypseOffCachedFilterSprite;
		}
	}

	public Sprite ApocalypseOnFilterSprite
	{
		get
		{
			if ((object)apocalypseOnCachedFilterSprite == null)
			{
				apocalypseOnCachedFilterSprite = GetApocalypseIcon(isOn: true);
			}
			return apocalypseOnCachedFilterSprite;
		}
	}

	public HashSet<string> ContainedFilterIds { get; private set; } = new HashSet<string>();

	public Sprite FilterSprite
	{
		get
		{
			if ((object)cachedFilterSprite == null)
			{
				cachedFilterSprite = GetIcon();
			}
			return cachedFilterSprite;
		}
	}

	public string Id { get; private set; }

	public bool IsAll { get; private set; }

	public string Name => Localizer.Get("FilterName_" + Id);

	public FilterDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute.IsNullOrEmpty())
		{
			TPDebug.LogError("FilterDefinition must have an Id.");
			return;
		}
		Id = xAttribute.Value;
		XAttribute xAttribute2 = xElement.Attribute("IsAll");
		if (xAttribute2 != null)
		{
			if (bool.TryParse(xAttribute2.Value, out var result))
			{
				IsAll = result;
			}
			else
			{
				TPDebug.LogError("Couldn't parse IsAll value " + xAttribute2.Value + " into a bool for filter: " + Id + ".");
			}
		}
		else
		{
			IsAll = false;
		}
		includedFiltersIds = new HashSet<string>();
		XElement xElement2 = xElement.Element("IncludedFiltersIds");
		if (xElement2 == null)
		{
			return;
		}
		foreach (XElement item in xElement2.Elements("FilterId"))
		{
			if (item.IsNullOrEmpty())
			{
				TPDebug.LogError("FilterDefinition has an empty FilterId for filter with id: " + Id + ".");
				continue;
			}
			string value = item.Value;
			includedFiltersIds.Add(value);
		}
	}

	public void ComputeIncludedFilters()
	{
		ContainedFilterIds.Clear();
		ComputeAllFiltersIds(Id);
	}

	private void ComputeAllFiltersIds(string filterId)
	{
		if (!GenericDatabase.FilterDefinitions.TryGetValue(filterId, out var value))
		{
			return;
		}
		ContainedFilterIds.Add(value.Id);
		foreach (string includedFiltersId in value.includedFiltersIds)
		{
			ComputeAllFiltersIds(includedFiltersId);
		}
	}

	private Sprite GetApocalypseIcon(bool isOn)
	{
		return ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Filters/Apocalypse/" + string.Format(isOn ? "Icons_Apocalypse_{0}_Modifier_On" : "Icons_Apocalypse_{0}_Modifier_Off", Id));
	}

	private Sprite GetIcon()
	{
		return ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Filters/" + $"Icons_Filters_{Id}_On");
	}
}
