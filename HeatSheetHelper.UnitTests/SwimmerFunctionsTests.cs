using HeatSheetHelper.Core.Helpers;
using HeatSheetHelper.Core.Interfaces;
using HeatSheetHelper.Core.Shared;
using Moq;

namespace HeatSheetHelper.UnitTests
{
    public class SwimmerFunctionsTests
    {
        [SetUp]
        public void Setup()
        {
            // Optional: Initialize resources before each test
        }

        [Test]
        public void ParseHeatSheetToEvents_ReturnsExpectedSwimMeet()
        {
            ISwimmerFunctions SwimmerFunctions = new SwimmerFunctions();
            // Arrange
            var heatSheet = new List<string>
            {
                "EVENT 1 Boys 8 & Under 25 Yard Freestyle",
                "HEAT 1 9:00 AM",
                "Sharks 15.23 8 Smith, John 1",
                "Dolphins 16.45 7 Doe, Jake 2"
            };

            // Act
            var result = SwimmerFunctions.ParseHeatSheetToEvents(heatSheet);

            // Assert
            Assert.That(result is not null);
            Assert.That(result.SwimEvents is not null);
            Assert.That(result.SwimEvents.Count.Equals(1));
            var swimEvent = result.SwimEvents[0];
            Assert.That(swimEvent.EventNumber.Equals(1));
            Assert.That("BOYS 8 & UNDER 25 YARD FREESTYLE".Equals(swimEvent.EventDetails));
            Assert.That(1.Equals(swimEvent.Heats.Count));
            var heat = swimEvent.Heats[0];
            Assert.That(1.Equals(heat.HeatNumber));
            Assert.That("9:00 AM".Equals(heat.StartTime));
            Assert.That(2.Equals(heat.LaneInfos.Count()));
        }
        [Test]
        public void CleanseTheData_Moq_ReturnsMockedValue()
        {
            var mock = new Mock<ISwimmerFunctions>();
            mock.Setup(x => x.CleanseTheData(It.IsAny<string>())).Returns("MOCKED");

            var result = mock.Object.CleanseTheData("dirty data");

            Assert.That(result, Is.EqualTo("MOCKED"));
        }

        [Test]
        public void TryParseTime_Moq_ReturnsTrueAndSetsOutParameter()
        {
            var mock = new Mock<ISwimmerFunctions>();
            var expectedDate = new DateTime(2025, 8, 20, 9, 0, 0);
            mock.Setup(x => x.TryParseTime("9:00 AM", out expectedDate)).Returns(true);

            DateTime actualDate;
            var success = mock.Object.TryParseTime("9:00 AM", out actualDate);

            Assert.That(success, Is.True);
            Assert.That(actualDate, Is.EqualTo(expectedDate));
        }

        [Test]
        public void ParseRelaySwimmerEventInfo_Moq_ReturnsTuple()
        {
            var mock = new Mock<ISwimmerFunctions>();
            var expectedTuple = new Tuple<string, string, string, bool>("15.23", "Sharks", "1", false);
            mock.Setup(x => x.ParseRelaySwimmerEventInfo(It.IsAny<string>(), It.IsAny<string>())).Returns(expectedTuple);

            var result = mock.Object.ParseRelaySwimmerEventInfo("line", "pattern");

            Assert.That(result, Is.EqualTo(expectedTuple));
        }

        [Test]
        public void SwapLastCommaFirstToFirstLast_ReturnsCorrectFormat()
        {
            ISwimmerFunctions swimmerFunctions = new SwimmerFunctions();
            var result = swimmerFunctions.SwapLastCommaFirstToFirstLast("Smith, John");
            Assert.That(result, Is.EqualTo("John Smith"));
        }

        [Test]
        public void FillMissingHeatStartTimes_FillsStartTimeFromPreviousHeat()
        {
            ISwimmerFunctions swimmerFunctions = new SwimmerFunctions();
            var swimMeet = new SwimMeet
            {
                SwimEvents = new List<SwimEvent>
                {
                    new SwimEvent
                    {
                        EventNumber = 1,
                        EventDetails = "Test Event",
                        Heats = new List<HeatInfo>
                        {
                            new HeatInfo { HeatNumber = 1, StartTime = "9:00 AM", LaneInfos = new List<LaneInfo>().AsQueryable() },
                            new HeatInfo { HeatNumber = 2, StartTime = "", LaneInfos = new List<LaneInfo>().AsQueryable() }
                        }
                    }
                }
            };

            swimmerFunctions.FillMissingHeatStartTimes(swimMeet);

            Assert.That(swimMeet.SwimEvents[0].Heats[1].StartTime, Is.EqualTo("9:00 AM"));
        }

        [Test]
        public void ParseRelaySwimmers_AddsSwimmersToHeat()
        {
            ISwimmerFunctions swimmerFunctions = new SwimmerFunctions();
            var heat = new HeatInfo
            {
                HeatNumber = 1,
                LaneInfos = new List<LaneInfo>().AsQueryable()
            };

            string line = "Smith, John 12 Doe, Jane 13";
            int lane = 2;
            string teamName = "Sharks";
            string teamLetter = "A";
            string seedTime = "15.23";
            string pattern = @"(?<name1>Smith, John)\s(?<age1>12)\s(?<name2>Doe, Jane)\s(?<age2>13)";

            swimmerFunctions.ParseRelaySwimmers(line, heat, lane, teamName, teamLetter, seedTime, pattern);

            var lanes = heat.LaneInfos.ToList();
            Assert.That(lanes.Count, Is.EqualTo(2));
            Assert.That(lanes[0].SwimmerName, Is.EqualTo("John Smith"));
            Assert.That(lanes[1].SwimmerName, Is.EqualTo("Jane Doe"));
            Assert.That(lanes[0].LaneNumber, Is.EqualTo(2));
            Assert.That(lanes[0].TeamName, Is.EqualTo("Sharks"));
            Assert.That(lanes[0].RelayTeamLetter, Is.EqualTo("A"));
            Assert.That(lanes[0].SeedTime, Is.EqualTo("15.23"));
            Assert.That(lanes[0].SwimmerAge, Is.EqualTo(12));
            Assert.That(lanes[1].SwimmerAge, Is.EqualTo(13));
        }

        [Test]
        public void CleanseTheData_ReplacesButterblyWithButterfly()
        {
            ISwimmerFunctions swimmerFunctions = new SwimmerFunctions();
            var result = swimmerFunctions.CleanseTheData("butterbly");
            Assert.That(result, Is.EqualTo("BUTTERFLY"));
        }

        [Test]
        public void TryParseTime_ValidTime_ReturnsTrue()
        {
            ISwimmerFunctions swimmerFunctions = new SwimmerFunctions();
            DateTime dt;
            var success = swimmerFunctions.TryParseTime("9:00 AM", out dt);
            Assert.That(success, Is.True);
            Assert.That(dt.Hour, Is.EqualTo(9));
            Assert.That(dt.Minute, Is.EqualTo(0));
        }

        [Test]
        public void ParseRelaySwimmerEventInfo_ParsesCorrectly()
        {
            ISwimmerFunctions swimmerFunctions = new SwimmerFunctions();
            string line = "Sharks 15.23 1";
            string pattern = @"(?<teamName>Sharks)\s(?<seedTime>15.23)\s(?<laneNumber>1)";
            var result = swimmerFunctions.ParseRelaySwimmerEventInfo(line, pattern);
            Assert.That(result.Item1, Is.EqualTo("15.23"));
            Assert.That(result.Item2, Is.EqualTo("Sharks"));
            Assert.That(result.Item3, Is.EqualTo("1"));
            Assert.That(result.Item4, Is.False);
        }

        [Test]
        public void FillMissingHeatStartTimes_Moq_VerifiesMethodCalled()
        {
            var mock = new Mock<ISwimmerFunctions>();
            var swimMeet = new SwimMeet();
            mock.Setup(x => x.FillMissingHeatStartTimes(swimMeet));
            mock.Object.FillMissingHeatStartTimes(swimMeet);
            mock.Verify(x => x.FillMissingHeatStartTimes(swimMeet), Times.Once);
        }
    }
}