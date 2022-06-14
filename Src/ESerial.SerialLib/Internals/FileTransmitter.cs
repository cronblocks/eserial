using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace ESerial.SerialLib.Internals
{
    internal class FileTransmitter
    {
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

        }

        private void TransmissionExecutor(object _)
        {

        }

        public void QuitTransmission()
        {

        }
    }
}
