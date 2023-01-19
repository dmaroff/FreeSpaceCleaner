using System.Text.RegularExpressions;

namespace FreeSpaceCleaner
{
    public class Drive
    {
        private DriveInfo _drive;
        private readonly int _blockSize = 512 * 1024;
        private readonly byte[] _buffer;
        private int _currentFileNumber;
        private readonly List<string> _files = new();
        private long _initialFreeSpace;
        private int _lastReportedPercentCompleted;

        public delegate void ProgressDelegate(double percentCompleted, long driveSize, long freeSpace);
        public event ProgressDelegate Progress;
        public delegate void LogDelegate(string logText);
        public event LogDelegate Logger;

        public Drive(DriveInfo drive)
        {
            _drive = drive;
            _buffer = new byte[_blockSize];
            SetCurrentFileNumber(_drive.RootDirectory.Name);
        }

        public void FillDrive()
        {
            _initialFreeSpace = _drive.AvailableFreeSpace;
            for (; ; _currentFileNumber++)
            {
                string file = Path.Combine(_drive.RootDirectory.Name, $"__file{_currentFileNumber}__.dat");
                _files.Add(file);
                FillTempFile(file);
                if (_drive.AvailableFreeSpace == 0)
                    break;
            }
            DeleteCreatedFiles();
        }

        private void FillTempFile(string file)
        {
            var fileWriter = File.OpenWrite(file);
            try
            {
                for (int i = 0; ;i++)
                {
                    FillByteArray();
                    fileWriter.Write(_buffer, 0, _blockSize);
                    if (i % 100 == 99)
                    {
                        int percentageCompleted = (int)(100 - ((double)_drive.AvailableFreeSpace / (double)_initialFreeSpace) * 100);
                        if (_lastReportedPercentCompleted != percentageCompleted)
                        {
                            _lastReportedPercentCompleted = percentageCompleted;
                            Progress?.Invoke(percentageCompleted, _drive.TotalSize, _drive.AvailableFreeSpace);
                        }
                    }
                }
            }
            catch (IOException) { }
            finally
            {
                fileWriter.Close();
            }
        }

        private void FillByteArray()
        {
            Random rnd = new();
            rnd.NextBytes(_buffer);
        }

        private void DeleteCreatedFiles()
        {
            Logger?.Invoke("\nDeleting temporary files ...");
            _files.ForEach(f => File.Delete(f));
            Logger?.Invoke("\nDone");
        }

        private void SetCurrentFileNumber(string folder)
        {
            int lastFileNumber = 0;
            foreach (var file in Directory.GetFiles(folder, "*.dat"))
            {
                var match = Regex.Match(Path.GetFileNameWithoutExtension(file), $"^__file[0-9]+__$");
                if (!match.Success)
                    continue;
                _files.Add(file);
                int number = int.Parse(match.Value.Trim("__".ToCharArray()).Substring(4));
                if (number > lastFileNumber)
                    lastFileNumber = number;
            }
            if (lastFileNumber > 0)
                _currentFileNumber = lastFileNumber + 1;
            else
                _currentFileNumber = 1;
        }
    }
}