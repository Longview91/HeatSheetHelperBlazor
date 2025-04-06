using HeatSheetHelperBlazor.Components.Shared;

namespace HeatSheetHelperBlazor.Components.Pages
{
    partial class AllEvents
    {
        List<SwimEvent> anEvents = new List<SwimEvent>();
        IQueryable<SwimEvent>? allEvents;
        IQueryable<HeatInfo>? allHeats;
        IQueryable<LaneInfo>? allLaneInfos;

        protected override async Task OnInitializedAsync()
        {
            // Simulate asynchronous loading to demonstrate a loading indicator
            await Task.Delay(500);
            anEvents = new List<SwimEvent>
            {
                new SwimEvent
                {
                    EventNumber = 1,
                    EventDetails = "Boys 50m Freestyle",
                    Heats = new List<HeatInfo>
                    {
                        new HeatInfo
                        {
                            HeatNumber = 1,
                            StartTime = "10:00 AM",
                            LaneInfos = new List<LaneInfo>
                            {
                                new LaneInfo { LaneNumber = 1, SwimmerName = "John Doe", SeedTime = "26.50", SwimmerAge = 12 },
                                new LaneInfo { LaneNumber = 2, SwimmerName = "Jane Smith", SeedTime = "26.00", SwimmerAge = 14 }
                            }.AsQueryable()
                        },
                        new HeatInfo
                        {
                            HeatNumber = 2,
                            StartTime = "10:05 AM",
                            LaneInfos = new List<LaneInfo>
                            {
                                new LaneInfo { LaneNumber = 1, SwimmerName = "Jim Doe", SeedTime = "25.50", SwimmerAge = 12 },
                                new LaneInfo { LaneNumber = 2, SwimmerName = "Jean Smith", SeedTime = "25.00", SwimmerAge = 14 }
                            }.AsQueryable()

                        }
                    }
                },
                new SwimEvent
                {
                    EventNumber = 2,
                    EventDetails = "100m Butterfly",
                    Heats = new List<HeatInfo>
                    {
                        new HeatInfo
                        {
                            HeatNumber = 2,
                            StartTime = "10:30 AM",
                            LaneInfos = new List<LaneInfo>
                            {
                                new LaneInfo { LaneNumber = 1, SwimmerName = "John Doe", SeedTime = "1:00.00", SwimmerAge = 12 },
                                new LaneInfo { LaneNumber = 2, SwimmerName = "Jane Smith", SeedTime = "1:05.00", SwimmerAge = 14 }
                            }.AsQueryable()
                        }
                    }
                },
                new SwimEvent
                {
                    EventNumber = 3,
                    EventDetails = "200m Backstroke",
                    Heats = new List<HeatInfo>
                    {
                        new HeatInfo
                        {
                            HeatNumber = 3,
                            StartTime = "11:00 AM",
                            LaneInfos = new List<LaneInfo>
                            {
                                new LaneInfo { LaneNumber = 1, SwimmerName = "John Doe", SeedTime = "2:00.00", SwimmerAge = 12 },
                                new LaneInfo { LaneNumber = 2, SwimmerName = "Jane Smith", SeedTime = "2:05.00", SwimmerAge = 14 }
                            }.AsQueryable()
                        }
                    }
                }
            };
            allEvents = anEvents.AsQueryable();
        }
    }
}
