namespace FreeSpaceCleaner
{
    public static class Application
    {
        public static void Run(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine($"Usage: FreeSpaceCleaner.exe <drive letter> [/noprompt]");
                Environment.Exit(-1);
            }
            string driveLetter = args[0].Substring(0, 2).ToUpper();
            bool skipPrompt = args.Length == 2 && args[1].ToLower() == "/noprompt";
            var drive = DriveInfo.GetDrives().FirstOrDefault(drive => drive.RootDirectory.Name.StartsWith(driveLetter, StringComparison.OrdinalIgnoreCase));
            if (drive == null)
            {
                Console.Error.WriteLine($"Could not locate drive {driveLetter}");
                Environment.Exit(-2);
            }
            Console.WriteLine("\nFree Space Cleaner (2023 by Dan Maroff)");
            Console.Write($"[Drive {driveLetter}, {drive.DriveType.ToString()}, ");
            Console.Write($"{drive.TotalFreeSpace.ToPercentage()}GB free of ");
            Console.WriteLine($"{drive.TotalSize.ToPercentage()}GB]");
            if (!skipPrompt)
            {
                Console.Write("\nPress C to continue, any key to quit : ");
                if (Console.ReadKey().Key != ConsoleKey.C)
                {
                    Environment.Exit(0);
                }
            }
            Console.Write("\nCleaning ...");
            var app = new Drive(drive);
            app.Progress += (percentage, driveSize, freeSpace) =>
            {
                ClearCurrentConsoleLine();
                Console.Write($"{percentage}% completed ({freeSpace.ToPercentage()}GB/{driveSize.ToPercentage()}GB)");
            };
            app.Logger += (message) =>
            {
                Console.Write(message);
            };
            app.FillDrive();
            Environment.Exit(0);
        }

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }
    }
}