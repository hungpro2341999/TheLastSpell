using System.Collections.Generic;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Helpers;

public static class FilterHelpers
{
	public static List<T> GetFilteredElements<T>(List<T> filterableElements, string filterId = null) where T : IFilterable
	{
		if (filterableElements == null)
		{
			return null;
		}
		if (filterId == null)
		{
			return filterableElements;
		}
		List<T> list = new List<T>();
		foreach (T filterableElement in filterableElements)
		{
			if (filterableElement.IsMatchingFilter(filterId))
			{
				list.Add(filterableElement);
			}
		}
		return list;
	}

	public static bool IsMatchingFilter(this IFilterable filterable, string filterId = null)
	{
		if (filterId == null)
		{
			return true;
		}
		if (!GenericDatabase.FilterDefinitions.TryGetValue(filterId, out var value))
		{
			CLoggerManager.Log("Filter definition with id '" + filterId + "' couldn't be found, this should never happen !", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "FilterHelpers");
			return true;
		}
		foreach (string filterId2 in filterable.FilterIds)
		{
			if (value.IsAll || value.ContainedFilterIds.Contains(filterId2))
			{
				return true;
			}
		}
		return false;
	}
}
