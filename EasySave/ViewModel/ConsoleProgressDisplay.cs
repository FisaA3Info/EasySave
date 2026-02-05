using EasySave.Model;
using System;
using System.Runtime.CompilerServices;

namespace EasySave.ViewModel
{
	internal class ConsoleProgressDisplay : IStateObserver
	{
		private int lastProgress = -1;
		private string lastJobname = "";
		public void OnStateChanged(StateEntry entry)
		{
			if (entry.JobName != lastJobname)
			{
				lastJobname = entry.JobName;
				lastProgress = -1;
			}
			Console.WriteLine();//Empty line in start because you know, we never know uh
			
			DrawProgressBar(entry);
			
			Console.WriteLine($" Files: {entry.TotalFiles - entry.FilesRemaining}/{entry.TotalFiles} | " + 
				$"Size: {BytesConvert(entry.TotalSize - entry.SizeRemaining)}/{BytesConvert(entry.TotalSize)}");

			if (!string.IsNullOrWhiteSpace(entry.CurrentSourceFile))
			{
				//Path.GetFileName is to get the file at the end of the path like C:\Test\notepad.txt return notepad.txt
				string fileName = Path.GetFileName(entry.CurrentSourceFile);
				Console.WriteLine($" Current: {TruncateString(fileName, 55)}");
			}

			Console.WriteLine(); //Yeah empty line again to separate

			lastProgress = entry.Progress;
			
		}

		private ConsoleColor GetProgressColor(int progress)
			//return a color depending of the actual progression
		{
			if (progress < 33) return ConsoleColor.Red;
			if (progress < 66) return ConsoleColor.Yellow;
			return ConsoleColor.Green;
		}

		private char GetSpinner()
		{
			char[] spinner = { '|', '/', '─', '\\' };
			//Millisecond has a value between 0 and 999, that we divide by 100 for it to be 9ms et modulo 4 because of our 4 different caracters and then it repeat.
			return spinner[(DateTime.Now.Millisecond / 100) % 4];
		}

		private void DrawProgressBar(StateEntry entry)
		{
			int barWidth = 50;
			int progress = entry.Progress;

			int filled = (int)(progress / 100 * barWidth);
			int empty = barWidth - filled;

			ConsoleColor barColor = GetProgressColor(progress);

			Console.Write(" [");

			//Filled part of the bar
			Console.ForegroundColor = barColor;
			Console.Write(new string('█', filled));

			//Empty part of the bar
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Write(new string('▒', empty));

			//Here just to reset the color in order to close the bar
			Console.ResetColor();
			Console.Write($"] {progress}%");

			//If the copy is still running than we get the spinner, if it's done we get the check marked
			if (progress > 0 && progress < 100)
			{
				Console.Write($" {GetSpinner()}");
			}
			else if (progress == 100)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write(" ✓");
				Console.ResetColor();
			}

			Console.WriteLine();
		}

		private string BytesConvert(long bytes)
			//Basically just find out which size is the files
		{
			string[] sizes = { "B", "KB", "MB", "GB", "TB" };
			double len = bytes;
			int indice = 0;

			while (len >= 1024 && indice < sizes.Length - 1)
			{
				indice++;
				len /= 1024;
			}
			//0 make sure there's atleast 1 number before the coma and # just round number after the coma without useless 0
			return $"{len:0.##} {sizes[indice]}";
		}

		private string TruncateString(string str, int maxLenght)
			//If file names is too big it just puts dots instand of the whole thing
		{
			if (string.IsNullOrEmpty(str) || str.Length <= maxLenght)
			{
				return str;
			}
			return str.Substring(0, maxLenght-3) + "...";
		}
	}
}