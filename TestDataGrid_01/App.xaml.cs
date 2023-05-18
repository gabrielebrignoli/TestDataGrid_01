using Syncfusion.Licensing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace TestDataGrid_01
{
    public partial class App : Application
    {
        //public Editing_Database Database { get; set; }
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
        protected override void OnStartup(StartupEventArgs e)
        {
            SyncfusionLicenseProvider.RegisterLicense(
                "Mgo+DSMBaFt+QHFqVkFrXVNbdV5dVGpAd0N3RGlcdlR1fUUmHVdTRHRcQlljS39VdU1jWHxddnw=;Mgo+DSMBPh8sVXJ1S0d+X1ZPd11dXmJWd1p/THNYflR1fV9DaUwxOX1dQl9gSXpTdERlWndecXdRTmg=;ORg4AjUWIQA/Gnt2VFhhQlJDfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hSn5XdkdiX3xWcHVWQGFU;MTU4ODUwOEAzMjMxMmUzMTJlMzMzN0JBTjlmYXhiT1hmU1hJTitibGFQWHlPcm1uNmlkWlJCSGNWbFlOWGM5aG89;MTU4ODUwOUAzMjMxMmUzMTJlMzMzN1BaYjF5NWhOcEd0RHl6RE9xcFpUdUt0OEllekl1YjRhUWYzRENteU5SMzg9;NRAiBiAaIQQuGjN/V0d+XU9HcVRDX3xKf0x/TGpQb19xflBPallYVBYiSV9jS31TckVhWHldeHRUQmRdWQ==;MTU4ODUxMUAzMjMxMmUzMTJlMzMzN0JQZlpFMXpQbG5sMDZtcUVBeFlsemJTUUVNR2JETUxlVk42ZnZ6ZEtMVjQ9;MTU4ODUxMkAzMjMxMmUzMTJlMzMzN0V5ZW5oMmN4ZHN2ZVhmbldRamhqdlBBbndSNi9pYjlUZm0zUnJCdXRPdk09;Mgo+DSMBMAY9C3t2VFhhQlJDfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hSn5XdkdiX3xWcHVQRmJU;MTU4ODUxNEAzMjMxMmUzMTJlMzMzN1Y1TXZIUXRDN2lHWDcrMHN6L0s0eFk0aHVaaE5yZTU2MUEzakprNVlSK0E9;MTU4ODUxNUAzMjMxMmUzMTJlMzMzN1g2dzdkUThPdExCTm9ZYkZIa3FoNUtrei9iZjZTS3dvSFJDS1pFRE4vSmM9;MTU4ODUxNkAzMjMxMmUzMTJlMzMzN0JQZlpFMXpQbG5sMDZtcUVBeFlsemJTUUVNR2JETUxlVk42ZnZ6ZEtMVjQ9");
            base.OnStartup(e);
            AllocConsole();
            base.OnStartup(e);
        }
    }
}