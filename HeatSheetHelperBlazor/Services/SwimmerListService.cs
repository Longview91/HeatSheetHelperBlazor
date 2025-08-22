using HeatSheetHelper.Core.Shared;
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
        public string? SwimmerFilter { get; set; } = string.Empty;
        public HashSet<string> SelectedSwimmers { get; set; } = new();
        public List<string> FilteredSwimmerNames =>
            SwimmerNameList
                .Where(name =>
                    (string.IsNullOrWhiteSpace(SwimmerFilter) || name.Contains(SwimmerFilter, StringComparison.OrdinalIgnoreCase))
                    || SelectedSwimmers.Contains(name))
                .Distinct()
                .OrderBy(name => name)
                .ToList();
        public SwimmerListService()
        {
            SwimmerNameList = new();
        }
    }
}