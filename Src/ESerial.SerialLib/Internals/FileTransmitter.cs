using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ESerial.SerialLib.Internals
{
    internal class FileTransmitter
    {
        public event Action FileTransmissionStarted;
        public event Action FileTransmissionFinished;
        public event Action<int> FileTransmissionPercentageUpdated;

        private readonly string _filename;
        private readonly PortCommunicator _portCommunicator;

        private volatile bool _isRunning = false;
        private Thread _thread;

        public FileTransmitter(string filename, PortCommunicator portCommunicator)
        {
            _filename = filename;
            _portCommunicator = portCommunicator;
        }

        public void StartTransmission()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                _thread = new Thread(TransmissionExecutor);
                _thread.Start();
            }
        }

        private void TransmissionExecutor(object _)
        {
            FileTransmissionStarted?.Invoke();

            int totalLines = GetTotalLinesInFile();
            FileTransmissionPercentageUpdated.Invoke(0);

            _isRunning = false;
            _thread = null;
            FileTransmissionFinished?.Invoke();
        }

        private int GetTotalLinesInFile()
        {
            int totalLines = 0;

            foreach (string _ in GetFileLines())
            {
                totalLines++;
            }

            return totalLines;
        }

        private IEnumerable<string> GetFileLines()
        {
            using (StreamReader sr = new StreamReader(_filename))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        public void QuitTransmission()
        {
            if (_isRunning)
            {
                _isRunning = false;
                _thread.Join(500);
                _thread = null;
            }
        }
    }
}
