using System.Collections.Generic;
using TheLastStand.Controller.Unit;
using TheLastStand.Definition.Unit;

namespace TheLastStand.Model.Unit;

public class LineOfSight
{
	public List<string> BuildingsBlocking { get; set; }

	public LineOfSightController LineOfSightController { get; private set; }

	public LineOfSightDefinition LineOfSightDefinition { get; private set; }

	public LineOfSight(LineOfSightDefinition definition, LineOfSightController lineOfSightController)
	{
		LineOfSightDefinition = definition;
		LineOfSightController = lineOfSightController;
	}
}
