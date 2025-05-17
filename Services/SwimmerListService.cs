using HeatSheetHelperBlazor.Components.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatSheetHelperBlazor.Services
{
    public class SwimmerListService
    {
        public List<string> SwimmerNameList { get; set; } = new();
        public string? SelectedSwimmer { get; set; }
        public string? SwimmerFilter { get; set; } = string.Empty;
        public List<string> FilteredSwimmerNames => string.IsNullOrWhiteSpace(SwimmerFilter)
            ? SwimmerNameList
            : SwimmerNameList.Where(name => name.Contains(SwimmerFilter, StringComparison.OrdinalIgnoreCase)).ToList();

        public SwimmerListService()
        {
            SwimmerNameList = new();
        }
    }
}