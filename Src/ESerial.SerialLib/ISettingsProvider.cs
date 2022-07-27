/*****************************************************************************************
 * Purpose:
 *     Describing interface requirement for settings provision
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

namespace ESerial.SerialLib
{
    public interface ISettingsProvider
    {
        void SetString(string key, string value);
        string GetString(string key);
        void SaveSettings();
    }
}
