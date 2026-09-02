using TheLastStand.Definition.Meta.Glyphs.GlyphEffects;
using TheLastStand.Framework.Serialization;
using TheLastStand.Serialization.Meta;

namespace TheLastStand.Model.Meta.Glyphs.GlyphEffects;

public class GlyphModifyBuildingActionsCostEffect : ISerializable, IDeserializable
{
	public int CurrentModifiersDailyLimit;

	public int EffectIndex { get; }

	public string GlyphParentId { get; }

	public GlyphModifyBuildingActionsCostEffectDefinition GlyphModifyBuildingActionsCostEffectDefinition { get; }

	public GlyphModifyBuildingActionsCostEffect(GlyphModifyBuildingActionsCostEffectDefinition glyphModifyBuildingActionsCostEffectDefinition, string glyphParentId, int effectIndex)
	{
		GlyphModifyBuildingActionsCostEffectDefinition = glyphModifyBuildingActionsCostEffectDefinition;
		CurrentModifiersDailyLimit = glyphModifyBuildingActionsCostEffectDefinition.ModifiersDailyLimit;
		GlyphParentId = glyphParentId;
		EffectIndex = effectIndex;
	}

	public bool CanUseModifiers()
	{
		if (CurrentModifiersDailyLimit <= 0)
		{
			return CurrentModifiersDailyLimit == -1;
		}
		return true;
	}

	public void ResetCurrentModifiersDailyLimit()
	{
		CurrentModifiersDailyLimit = GlyphModifyBuildingActionsCostEffectDefinition.ModifiersDailyLimit;
	}

	public void UseModifiers()
	{
		if (CurrentModifiersDailyLimit > 0)
		{
			CurrentModifiersDailyLimit--;
		}
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedModifyBuildingActionsCostGlyph serializedModifyBuildingActionsCostGlyph = container as SerializedModifyBuildingActionsCostGlyph;
		CurrentModifiersDailyLimit = serializedModifyBuildingActionsCostGlyph.CurrentModifiersDailyLimit;
	}

	public ISerializedData Serialize()
	{
		return new SerializedModifyBuildingActionsCostGlyph
		{
			GlyphParentId = GlyphParentId,
			EffectIndex = EffectIndex,
			CurrentModifiersDailyLimit = CurrentModifiersDailyLimit
		};
	}
}
