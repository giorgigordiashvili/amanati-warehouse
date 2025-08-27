using System.Management;

namespace Amanati.ge
{
    public partial class GetDeviceInfo
    {
        public partial string GetDeviceID()
        {
            var ID = string.Empty;
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_processor");
                ManagementObjectCollection objList = searcher.Get();
                foreach (ManagementObject obj in objList)
                {
                    ID = obj["ProcessorID"].ToString();
                }

                searcher = new ManagementObjectSearcher("Select * From Win32_BaseBoard");
                objList = searcher.Get();
                foreach (ManagementObject obj in objList)
                {
                    ID += obj["SerialNumber"].ToString();
                }
            }
            catch (Exception) { }

            return ID;
        }

    }
}
