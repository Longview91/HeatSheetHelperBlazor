using HeatSheetHelper.Core.Shared;

namespace HeatSheetHelperBlazor.Services
{
    public class MeetDataService
    {
        public SwimMeet SwimMeet { get; set; } = new();

        public MeetDataService()
        {
            SwimMeet = new SwimMeet();
        }
    }
}