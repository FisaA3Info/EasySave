using System;

public class ConsoleProgressDisplay : IStateObserver
{
	public void OnStateChanged(StateEntry entry)
	{
	
	}

    private ConsoleColor GetProgressColor(int progress)
    {
        if (progress < 33) return ConsoleColor.Red;
        if (progress < 66) return ConsoleColor.Yellow;
        return ConsoleColor.Green;
    }

    private char GetSpinner()
    {
        char[] spinner = { '|', '/', '─', '\\' };
        return spinner[(DateTime.Now.Millisecond / 100) % 4];
    }

    private void DrawProgressBar(StateEntry entry)
	{
		int barWidth = 50;
		int progress = entry.Progress;

		int filled =(int)(progress/100 *barWidth);
		int empty = barWidth - filled;

		ConsoleColor barColor = GetProgressColor(progress);

		Console.Write(" [");

		Console.ForegroundColor = barColor;
		Console.Write(new string(' ', filled));

		Console.ResetColor();
		Console.Write($"} {progress}%");

		if (progress > 0 && progress < 100)
		{
			Console.Write($" {GetSpinner()}")
		}
		else if (progress == 100)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write(" ✓");
			Console.ResetCOlor();
		}

		Console.WriteLine();
	}
}
