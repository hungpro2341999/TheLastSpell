namespace System.Runtime.CompilerServices;

internal static class RuntimeHelpers
{
	public static T[] GetSubArray<T>(T[] array, Range range)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		var (sourceIndex, num) = range.GetOffsetAndLength(array.Length);
		if (default(T) != null || typeof(T[]) == array.GetType())
		{
			if (num == 0)
			{
				return Array.Empty<T>();
			}
			T[] array2 = new T[num];
			Array.Copy(array, sourceIndex, array2, 0, num);
			return array2;
		}
		T[] array3 = (T[])Array.CreateInstance(array.GetType().GetElementType(), num);
		Array.Copy(array, sourceIndex, array3, 0, num);
		return array3;
	}
}
