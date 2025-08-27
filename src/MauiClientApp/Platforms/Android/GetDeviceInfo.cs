using static Android.Provider.Settings;
using AndroidApp = Android.App.Application;
using Setting = Android.Provider.Settings;

namespace Amanati.ge
{
    public partial class GetDeviceInfo
    {
        public partial string GetDeviceID()
        {
            var context = AndroidApp.Context;

            string id = Setting.Secure.GetString(context.ContentResolver, Secure.AndroidId);

            return id;
        }
    }
}
