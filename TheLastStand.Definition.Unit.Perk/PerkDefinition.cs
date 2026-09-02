using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Definition.Tooltip.Compendium;
using TheLastStand.Definition.Unit.Perk.PerkDataCondition;
using TheLastStand.Framework;
using TheLastStand.Framework.ExpressionInterpreter;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk;

public class PerkDefinition : LocalizableDefinition
{
	public class ModdedPerkDefinitionData
	{
		public readonly string IconFolderExternalPath;

		public readonly string IconFileName;

		public static readonly (int width, int height) IconDimensions = (width: 42, height: 42);

		public string IconExternalFullPath => Path.Combine(IconFolderExternalPath, IconFileName);

		public ModdedPerkDefinitionData(string iconFolderExternalPath, string iconFileName)
		{
			IconFolderExternalPath = iconFolderExternalPath;
			IconFileName = iconFileName;
		}
	}

	private Sprite cachedPerkSprite;

	public static string CurrentPerkId { get; private set; }

	public bool DisplayBonusBeforePurchase { get; private set; }

	public bool DisplayHavenArea { get; private set; }

	public bool DisplayInHUD { get; private set; }

	public Color? HavenAreaColor { get; private set; }

	public Node HudBonus { get; private set; }

	public Node HudBuffer { get; private set; }

	public Node HudMalus { get; private set; }

	public List<Node> HoverRanges { get; private set; }

	public bool IsModded => ModdingData != null;

	public ModdedPerkDefinitionData ModdingData { get; set; }

	public string ColorizedName => "<style=Perk>" + Name + "</style>";

	public string Name => Localizer.Get("PerkName_" + Id);

	public string Id { get; private set; }

	public PerkDataConditionsDefinition GreyOutConditionsDefinition { get; private set; }

	public PerkDataConditionsDefinition HighlightConditionsDefinition { get; private set; }

	public PerkDataConditionsDefinition FeedbackActivationConditionsDefinition { get; private set; }

	public string PerkEffectsInformations => "PerkEffectInformations_" + Id;

	public bool PerkEffectsInformationsExist { get; private set; }

	public List<APerkModuleDefinition> PerkModuleDefinitions { get; private set; }

	public Sprite PerkSprite
	{
		get
		{
			if ((object)cachedPerkSprite == null)
			{
				cachedPerkSprite = GetIcon();
			}
			return cachedPerkSprite;
		}
	}

	public HashSet<CompendiumEntryDefinition> CompendiumEntries { get; private set; }

	public List<Tuple<string, int>> SkillsToShow { get; private set; }

	public PerkDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XElement xTokenVariables = obj.Element("TokenVariables");
		DeserializeTokenVariables(xTokenVariables);
		XElement container2 = obj.Element("LocArguments");
		base.Deserialize((XContainer)container2);
		XAttribute xAttribute = obj.Attribute("Id");
		Id = xAttribute.Value;
		CurrentPerkId = Id;
		PerkEffectsInformationsExist = Localizer.Exists(PerkEffectsInformations);
		CompendiumEntries = new HashSet<CompendiumEntryDefinition>();
		HoverRanges = new List<Node>();
		SkillsToShow = new List<Tuple<string, int>>();
		XElement xElement = obj.Element("View");
		if (xElement != null)
		{
			DeserializeView(xElement);
		}
		XElement xModules = obj.Element("Modules");
		DeserializeModules(xModules);
		CurrentPerkId = string.Empty;
	}

	private void DeserializeModules(XElement xModules)
	{
		PerkModuleDefinitions = new List<APerkModuleDefinition>();
		foreach (XElement item in xModules.Elements())
		{
			switch (item.Name.LocalName)
			{
			case "BufferModule":
				PerkModuleDefinitions.Add(new BufferModuleDefinition(item, base.TokenVariables));
				break;
			case "GaugeModule":
				PerkModuleDefinitions.Add(new GaugeModuleDefinition(item, base.TokenVariables));
				break;
			default:
				CLoggerManager.Log("Tried to Deserialize an unimplemented PerkModule: " + item.Name.LocalName, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PerkDefinition");
				break;
			}
		}
	}

	private void DeserializeView(XElement xView)
	{
		XElement xElement = xView.Element("CompendiumEntries");
		if (xElement != null)
		{
			foreach (XElement item in xElement.Elements("CompendiumEntry"))
			{
				CompendiumEntries.Add(new CompendiumEntryDefinition(item));
			}
		}
		XElement xElement2 = xView.Element("SkillsToShow");
		if (xElement2 != null)
		{
			foreach (XElement item2 in xElement2.Elements("SkillToShow"))
			{
				XAttribute xAttribute = item2.Attribute("Id");
				XAttribute xAttribute2 = item2.Attribute("OverallUses");
				int result = -1;
				if (xAttribute2 != null && !int.TryParse(xAttribute2.Value, out result))
				{
					CLoggerManager.Log("Could not parse OverallUses attribute into an int", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PerkDefinition");
					result = -1;
				}
				SkillsToShow.Add(new Tuple<string, int>(xAttribute.Value, result));
			}
		}
		DisplayBonusBeforePurchase = xView.Element("DisplayBonusBeforePurchase") != null;
		XElement xElement3 = xView.Element("DisplayInHUD");
		DisplayInHUD = xElement3 != null;
		if (xElement3 != null)
		{
			XElement xElement4 = xElement3.Element("Bonus");
			if (xElement4 != null)
			{
				XAttribute xAttribute3 = xElement4.Attribute("Value");
				HudBonus = Parser.Parse(xAttribute3.Value, base.TokenVariables);
			}
			XElement xElement5 = xElement3.Element("Malus");
			if (xElement5 != null)
			{
				XAttribute xAttribute4 = xElement5.Attribute("Value");
				HudMalus = Parser.Parse(xAttribute4.Value, base.TokenVariables);
			}
			XElement xElement6 = xElement3.Element("Buffer");
			if (xElement6 != null)
			{
				XAttribute xAttribute5 = xElement6.Attribute("Value");
				HudBuffer = Parser.Parse(xAttribute5.Value, base.TokenVariables);
			}
			GreyOutConditionsDefinition = new PerkDataConditionsDefinition(xElement3.Element("GreyOut"), base.TokenVariables);
			HighlightConditionsDefinition = new PerkDataConditionsDefinition(xElement3.Element("Highlight"), base.TokenVariables);
			XElement xElement7 = xElement3.Element("HoverDisplay");
			if (xElement7 != null)
			{
				foreach (XElement item3 in xElement7.Elements("HoverRange"))
				{
					XAttribute xAttribute6 = item3.Attribute("Range");
					HoverRanges.Add(Parser.Parse(xAttribute6.Value, base.TokenVariables));
				}
				XElement xElement8 = xElement7.Element("HavenArea");
				DisplayHavenArea = false;
				HavenAreaColor = null;
				if (xElement8 != null)
				{
					XAttribute xAttribute7 = xElement8.Attribute("Color");
					if (xAttribute7 != null)
					{
						if (!ColorUtility.TryParseHtmlString(xAttribute7.Value, out var color))
						{
							CLoggerManager.Log("The Color attribute of element 'HavenArea' in perk '" + Id + "' has an invalid color of '" + xAttribute7.Value + "'", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PerkDefinition");
						}
						else
						{
							HavenAreaColor = color;
						}
					}
					DisplayHavenArea = true;
				}
			}
		}
		FeedbackActivationConditionsDefinition = new PerkDataConditionsDefinition(xView.Element("FeedbackActivationConditions"), base.TokenVariables);
	}

	public string GetAdditionDescription(InterpreterContext interpreterContext)
	{
		if (base.LocArguments == null)
		{
			return Localizer.Get(PerkEffectsInformations);
		}
		return Localizer.Format(PerkEffectsInformations, GetArguments(interpreterContext));
	}

	public string GetDescription(InterpreterContext interpreterContext)
	{
		if (base.LocArguments == null)
		{
			return Localizer.Get("PerkDescription_" + Id);
		}
		return Localizer.Format("PerkDescription_" + Id, GetArguments(interpreterContext));
	}

	public Sprite GetIcon()
	{
		Sprite sprite = null;
		if (IsModded)
		{
			if (File.Exists(ModdingData.IconExternalFullPath))
			{
				byte[] data = File.ReadAllBytes(ModdingData.IconExternalFullPath);
				Texture2D texture2D = new Texture2D(ModdedPerkDefinitionData.IconDimensions.width, ModdedPerkDefinitionData.IconDimensions.height, TextureFormat.ARGB32, mipChain: false);
				texture2D.LoadImage(data);
				texture2D.name = Path.GetFileNameWithoutExtension(ModdingData.IconFileName);
				sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
			}
		}
		else
		{
			sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Perks/" + Id);
		}
		if (sprite == null)
		{
			sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Perks/Default");
		}
		return sprite;
	}
}
