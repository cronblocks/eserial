namespace ESerial.SerialLib
{
    public interface ISettingsProvider
    {
        void SetString(string key, string value);
        string GetString(string key);
    }
}
