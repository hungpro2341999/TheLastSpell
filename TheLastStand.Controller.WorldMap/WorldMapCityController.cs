using System.Collections.Generic;
using TheLastStand.Definition.Apocalypse;
using TheLastStand.Definition.Meta.Glyphs;
using TheLastStand.Definition.WorldMap;
using TheLastStand.Model.WorldMap;
using TheLastStand.Serialization;
using TheLastStand.View.WorldMap;

namespace TheLastStand.Controller.WorldMap;

public class WorldMapCityController
{
	public WorldMapCity WorldMapCity { get; private set; }

	public WorldMapCityController(SerializedCity cityElement, CityDefinition definition, WorldMapCityView view, int saveVersion)
	{
		WorldMapCity = new WorldMapCity(definition, this, view);
		WorldMapCity.Deserialize(cityElement, saveVersion);
	}

	public bool CanAddGlyph(GlyphDefinition glyphDefinition)
	{
		if (!WorldMapCity.GlyphsConfig.CustomModeEnabled)
		{
			return WorldMapCity.CurrentGlyphPoints + glyphDefinition.Cost <= WorldMapCity.CityDefinition.MaxGlyphPoints;
		}
		return true;
	}

	public void AddGlyph(GlyphDefinition glyphDefinition)
	{
		WorldMapCity.CurrentGlyphPoints += glyphDefinition.Cost;
		WorldMapCity.GlyphsConfig.SelectedGlyphs.Add(glyphDefinition);
	}

	public void RemoveGlyph(GlyphDefinition glyphDefinition)
	{
		if (WorldMapCity.GlyphsConfig.SelectedGlyphs.Remove(glyphDefinition))
		{
			WorldMapCity.CurrentGlyphPoints -= glyphDefinition.Cost;
		}
	}

	public void RemoveGlyphAt(int index)
	{
		WorldMapCity.CurrentGlyphPoints -= WorldMapCity.GlyphsConfig.SelectedGlyphs[index].Cost;
		WorldMapCity.GlyphsConfig.SelectedGlyphs.RemoveAt(index);
	}

	public void TryAddingCompletedApocalypseModifierStep(string modifierId, int stepIndex)
	{
		if (stepIndex != -1)
		{
			if (!WorldMapCity.CompletedApocalypseModifiersStepIndex.ContainsKey(modifierId))
			{
				WorldMapCity.CompletedApocalypseModifiersStepIndex.Add(modifierId, stepIndex);
			}
			else if (WorldMapCity.CompletedApocalypseModifiersStepIndex[modifierId] < stepIndex)
			{
				WorldMapCity.CompletedApocalypseModifiersStepIndex[modifierId] = stepIndex;
			}
		}
	}

	public void TryModifyingCompletedApocalypseModifiersSteps(List<ApocalypseModifierStepDefinition> stepDefinitions)
	{
		if (stepDefinitions == null || stepDefinitions.Count == 0)
		{
			return;
		}
		foreach (ApocalypseModifierStepDefinition stepDefinition in stepDefinitions)
		{
			(string, int) modifierIdAndStepIndex = stepDefinition.GetModifierIdAndStepIndex();
			TryAddingCompletedApocalypseModifierStep(modifierIdAndStepIndex.Item1, modifierIdAndStepIndex.Item2);
		}
	}
}
