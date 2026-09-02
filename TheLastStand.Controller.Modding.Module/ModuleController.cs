using System.IO;
using TheLastStand.Model.Modding.Module;

namespace TheLastStand.Controller.Modding.Module;

public abstract class ModuleController
{
	protected static class ModuleControllerConstants
	{
		public const string TripleSpace = "   ";

		public const string SextupleSpace = "      ";

		public const string NonupleSpace = "         ";
	}

	protected ModdingModule module;

	public ModuleController(DirectoryInfo directory)
	{
	}
}
