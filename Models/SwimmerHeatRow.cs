using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatSheetHelperBlazor.Models
{
    public class SwimmerHeatRow
    {
        public int EventNumber { get; set; }
        public int HeatNumber { get; set; }
        public int LaneNumber { get; set; }
        public string? SwimmerName { get; set; }
        public string? EventName { get; set; }
        public string? StartTime { get; set; }
        public string? SeedTime { get; set; }
    }
}
