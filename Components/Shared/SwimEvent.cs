using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace HeatSheetHelperBlazor.Components.Shared
{
    public class SwimEvent
    {
        public int EventNumber { get; set; }
        public string EventDetails { get; set; }
        public List<HeatInfo> Heats { get; set; }
        public bool IsRelay { get; set; }
        public SwimEvent()
        {
            EventNumber = 0;
            EventDetails = string.Empty;
            Heats = new List<HeatInfo>();
            IsRelay = false;
        }
    }
}
