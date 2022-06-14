using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        private FileStream _fileStream;
        private Thread _thread;

        public FileTransmitter(string filename, PortCommunicator portCommunicator)
        {
            _filename = filename;
            _portCommunicator = portCommunicator;

            _fileStream = new FileStream(_filename, FileMode.Open, FileAccess.Read);
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

            FileTransmissionPercentageUpdated.Invoke(0);

            _isRunning = false;
            _thread = null;
            FileTransmissionFinished?.Invoke();
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
