using Android.App;
using Android.Content;
using Android.Telephony;
using ChargerNet.Services;
using System;

namespace ChargerNet.Droid.Services
{
    public class AndroidDeviceInfo : IDeviceInfo
    {
        public string GetPhone()
        {
            var phone = string.Empty;
            try
            {
                var manager = Application.Context.GetSystemService(Context.TelephonyService) as TelephonyManager;
                phone = manager.Line1Number;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return phone;
        }
    }
}