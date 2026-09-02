using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Manager;

namespace TheLastStand.Model;

public class FormulaInterpreterContext : DefaultInterpreterContext
{
	public FormulaInterpreterContext(object targetObject = null)
		: base(targetObject)
	{
	}

	public float Random(double a, double b)
	{
		if (a > b)
		{
			double num = b;
			double num2 = a;
			a = num;
			b = num2;
		}
		return RandomManager.GetRandomRange(this, (float)a, (float)b);
	}

	public int RandomInt(double a, double b)
	{
		if (a > b)
		{
			double num = b;
			double num2 = a;
			a = num;
			b = num2;
		}
		return RandomManager.GetRandomRange(this, (int)a, (int)b + 1);
	}
}
