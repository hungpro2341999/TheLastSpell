using TheLastStand.Manager.Modding;

namespace TheLastStand.Model.LaunchArgument;

public static class DevArgument
{
	public const string Argument = "-dev";

	public static void ActivateLaunchArgument()
	{
		ModManager.IsModder = true;
	}
}
