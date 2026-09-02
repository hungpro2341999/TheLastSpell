using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition;

public class GroundDefinition : TheLastStand.Framework.Serialization.Definition
{
	public enum E_GroundCategory
	{
		None = -1,
		Outside,
		City,
		NoBuilding
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct GroundCategoryComparer : IEqualityComparer<E_GroundCategory>
	{
		public bool Equals(E_GroundCategory x, E_GroundCategory y)
		{
			return x == y;
		}

		public int GetHashCode(E_GroundCategory obj)
		{
			return (int)obj;
		}
	}

	public static class Constants
	{
		public static class Ids
		{
			public const string Crater = "Crater";

			public const string Dirt = "Dirt";

			public const string Ground = "Ground";

			public const string Stone = "Stone";
		}
	}

	public static readonly GroundCategoryComparer SharedGroundCategoryComparer;

	public E_GroundCategory GroundCategory { get; private set; } = E_GroundCategory.None;

	public string Id { get; private set; }

	public bool IsCrossable { get; private set; }

	public GroundDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		Id = xElement.Attribute("Id").Value;
		if (Enum.TryParse<E_GroundCategory>(xElement.Element("GroundCategory").Value, out var result))
		{
			GroundCategory = result;
		}
		else
		{
			CLoggerManager.Log("Error while parsing GroundType in GroundDefinition, keeping None value.", LogType.Error, CLogLevel.NORMAL, forcePrintInUnity: true, GetType().Name);
		}
		IsCrossable = bool.Parse(xElement.Element("IsCrossable").Value);
	}
}
