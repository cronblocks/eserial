/*****************************************************************************************
 * Purpose:
 *     Providing lines from text file for transmission over Serial Port
 *
 *
 *****************************************************************************************
 * Author: Usama
 *
 *****************************************************************************************
 * Changes:
 *
 * Date         Changed by      Description
 * ----         ----------      -----------
 *
 *
 *
 *
 *****************************************************************************************/

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
        private readonly SerialService _serialService;

        private volatile bool _isRunning = false;
        private Thread _thread;

        public FileTransmitter(string filename, SerialService serialService)
        {
            _filename = filename;
            _serialService = serialService;
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
            int currentLineNumber = 0;
            int currentPercentage = 0;
            int lastPercentage = 0;

            foreach (string line in GetFileLines())
            {
                if (!_isRunning)
                {
                    break;
                }

                _serialService.SendTextLineWithEndings(line);
                Thread.Sleep((int)_serialService.FileInterLineTimeDelay);

                currentLineNumber++;
                currentPercentage = (int)((currentLineNumber / (float)totalLines) * 100);
                if (currentPercentage != lastPercentage)
                {
                    FileTransmissionPercentageUpdated.Invoke(currentPercentage);
                }
                lastPercentage = currentPercentage;
            }

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
