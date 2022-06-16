namespace ESerial.SerialLib
{
    public interface ISettingsProvider
    {
        void StoreString(string key, string value);
        string RetrieveString(string key);
    }
}
