using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphToggleSkillProgressionFlagEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "ToggleSkillProgressionFlag";

	public GlyphManager.E_SkillProgressionFlag SkillProgressionFlag { get; private set; }

	public GlyphToggleSkillProgressionFlagEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("Flag");
		string text = xAttribute.Value.Replace(base.TokenVariables);
		if (!Enum.TryParse<GlyphManager.E_SkillProgressionFlag>(text, out var result))
		{
			CLoggerManager.Log("ToggleSkillProgressionFlag Unable to parse " + text + " (" + xAttribute.Value + ") into a E_SkillProgressionFlag", LogType.Error, CLogLevel.MAJOR);
		}
		SkillProgressionFlag = result;
	}
}
