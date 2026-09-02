using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TheLastStand.Serialization;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct SaveTypeComparer : IEqualityComparer<E_SaveType>
{
	public bool Equals(E_SaveType x, E_SaveType y)
	{
		return x == y;
	}

	public int GetHashCode(E_SaveType obj)
	{
		return (int)obj;
	}
}
