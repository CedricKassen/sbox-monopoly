namespace Monopoly;

public static class Math
{
	public static int Mod(int dividend, int divisor)
	{
		return (dividend % divisor + divisor) % divisor;
	}
}
