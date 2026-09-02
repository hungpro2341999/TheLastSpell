using System;
using System.Collections.Generic;
using TPLib;
using TheLastStand.Manager.Meta;
using TheLastStand.Model;
using TheLastStand.Model.WorldMap;

namespace TheLastStand.Manager.WorldMap;

[Serializable]
public class LastRunInfo : ISerializedData
{
	public string CityId;

	public List<string> NewlyCompletedGlyphsId;

	private int gameOverCause;

	public Game.E_GameOverCause GameOverCause => (Game.E_GameOverCause)gameOverCause;

	public LastRunInfo(WorldMapCity city, Game.E_GameOverCause gameOverCause)
	{
		CityId = city.CityDefinition.Id;
		NewlyCompletedGlyphsId = new List<string>();
		this.gameOverCause = (int)gameOverCause;
		for (int num = city.GlyphsConfig.SelectedGlyphs.Count - 1; num >= 0; num--)
		{
			string id = city.GlyphsConfig.SelectedGlyphs[num].Id;
			if (!TPSingleton<GlyphManager>.Instance.MaxApoPassedByCityByGlyph.ContainsKey(id))
			{
				NewlyCompletedGlyphsId.Add(id);
			}
		}
	}

	public LastRunInfo(string cityId, List<string> newlyCompletedGlyphsId, Game.E_GameOverCause gameOverCause)
	{
		CityId = cityId;
		NewlyCompletedGlyphsId = newlyCompletedGlyphsId;
		this.gameOverCause = (int)gameOverCause;
	}

	public LastRunInfo()
	{
	}
}
