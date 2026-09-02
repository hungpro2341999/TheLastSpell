using TheLastStand.Definition;
using TheLastStand.Framework.Command;
using TheLastStand.Framework.Command.Conversation;
using TheLastStand.Framework.Extensions;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.Model.LevelEditor.Conversation;

public class SetGroundColorCommand : ICompensableCommand, ICommand
{
	private GroundDefinition groundDefinition;

	private Color color = Color.white;

	private Color previousColor = Color.white;

	public SetGroundColorCommand(GroundDefinition groundDefinition, Color color)
	{
		this.groundDefinition = groundDefinition;
		this.color = color;
		switch (this.groundDefinition.GroundCategory)
		{
		case GroundDefinition.E_GroundCategory.City:
			previousColor = TileMapView.GroundCityTilemap.color;
			break;
		case GroundDefinition.E_GroundCategory.NoBuilding:
			previousColor = TileMapView.GroundCraterTilemap.color;
			break;
		}
	}

	public void Compensate()
	{
		SetGroundColor(previousColor);
	}

	public bool Execute()
	{
		SetGroundColor(color);
		return true;
	}

	private void SetGroundColor(Color color)
	{
		switch (groundDefinition.GroundCategory)
		{
		case GroundDefinition.E_GroundCategory.City:
			TileMapView.GroundCityTilemap.color = color.WithA(TileMapView.GroundCityTilemap.color.a);
			break;
		case GroundDefinition.E_GroundCategory.NoBuilding:
			TileMapView.GroundCraterTilemap.color = color.WithA(TileMapView.GroundCraterTilemap.color.a);
			break;
		}
	}

	public override string ToString()
	{
		return "Set ground " + groundDefinition.Id + " color";
	}
}
