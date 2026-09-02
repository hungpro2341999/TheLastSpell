using System.Collections.Generic;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Unit;
using TheLastStand.Model.Unit;

namespace TheLastStand.Controller.Unit;

public class LineOfSightController
{
	public LineOfSight LineOfSight { get; private set; }

	public LineOfSightController(LineOfSightDefinition definition)
	{
		LineOfSight = new LineOfSight(definition, this)
		{
			BuildingsBlocking = new List<string>(BuildingDatabase.BuildingDefinitions.Keys)
		};
		foreach (string buildingBlockingException in definition.BuildingBlockingExceptions)
		{
			if (LineOfSight.BuildingsBlocking.Contains(buildingBlockingException))
			{
				LineOfSight.BuildingsBlocking.Remove(buildingBlockingException);
			}
		}
	}
}
