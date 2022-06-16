using System;
using System.Collections.Generic;
using System.Text;

namespace ESerial.SerialLib
{
    public interface ISettingsProvider
    {
        void StoreString(string key, string value);
        string RetrieveString(string key);
    }
}
