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

            SwimmerFunctions SwimmerFunctions = new();
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

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result is not null);
                Assert.That(result.SwimEvents is not null);
                Assert.That(result.SwimEvents, Has.Count.EqualTo(1));
            });
            var swimEvent = result.SwimEvents[0];
            Assert.Multiple(() =>
            {
                Assert.That(swimEvent.EventNumber, Is.EqualTo(1));
                Assert.That(swimEvent.EventDetails, Is.EqualTo("BOYS 8 & UNDER 25 YARD FREESTYLE"));
                Assert.That(swimEvent.Heats, Has.Count.EqualTo(1));
            });
            var heat = swimEvent.Heats[0];
            Assert.Multiple(() =>
            {
                Assert.That(heat.HeatNumber, Is.EqualTo(1));
                Assert.That(heat.StartTime, Is.EqualTo("9:00 AM"));
                Assert.That(heat.LaneInfos.Count(), Is.EqualTo(2));
            });
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

            var success = mock.Object.TryParseTime("9:00 AM", out DateTime actualDate);

            Assert.Multiple(() =>
            {
                Assert.That(success, Is.True);
                Assert.That(actualDate, Is.EqualTo(expectedDate));
            });
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
            SwimmerFunctions swimmerFunctions = new();
            var result = swimmerFunctions.SwapLastCommaFirstToFirstLast("Smith, John");
            Assert.That(result, Is.EqualTo("John Smith"));
        }

        [Test]
        public void FillMissingHeatStartTimes_FillsStartTimeFromPreviousHeat()
        {
            SwimmerFunctions swimmerFunctions = new();
            var swimMeet = new SwimMeet
            {
                SwimEvents =
                [
                    new SwimEvent
                    {
                        EventNumber = 1,
                        EventDetails = "Test Event",
                        Heats =
                        [
                            new HeatInfo { HeatNumber = 1, StartTime = "9:00 AM", LaneInfos = new List<LaneInfo>().AsQueryable() },
                            new HeatInfo { HeatNumber = 2, StartTime = "", LaneInfos = new List<LaneInfo>().AsQueryable() }
                        ]
                    }
                ]
            };

            swimmerFunctions.FillMissingHeatStartTimes(swimMeet);

            Assert.That(swimMeet.SwimEvents[0].Heats[1].StartTime, Is.EqualTo("9:00 AM"));
        }

        [Test]
        public void ParseRelaySwimmers_AddsSwimmersToHeat()
        {
            SwimmerFunctions swimmerFunctions = new();
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
            Assert.Multiple(() =>
            {
                Assert.That(lanes, Has.Count.EqualTo(2));
                Assert.That(lanes[0].SwimmerName, Is.EqualTo("John Smith"));
                Assert.That(lanes[1].SwimmerName, Is.EqualTo("Jane Doe"));
                Assert.That(lanes[0].LaneNumber, Is.EqualTo(2));
                Assert.That(lanes[0].TeamName, Is.EqualTo("Sharks"));
                Assert.That(lanes[0].RelayTeamLetter, Is.EqualTo("A"));
                Assert.That(lanes[0].SeedTime, Is.EqualTo("15.23"));
                Assert.That(lanes[0].SwimmerAge, Is.EqualTo(12));
                Assert.That(lanes[1].SwimmerAge, Is.EqualTo(13));
            });
        }

        [Test]
        public void CleanseTheData_ReplacesButterblyWithButterfly()
        {
            SwimmerFunctions swimmerFunctions = new();
            var result = swimmerFunctions.CleanseTheData("butterbly");
            Assert.That(result, Is.EqualTo("BUTTERFLY"));
        }

        [Test]
        public void TryParseTime_ValidTime_ReturnsTrue()
        {
            SwimmerFunctions swimmerFunctions = new();
            var success = swimmerFunctions.TryParseTime("9:00 AM", out DateTime dt);
            Assert.Multiple(() =>
            {
                Assert.That(success, Is.True);
                Assert.That(dt.Hour, Is.EqualTo(9));
                Assert.That(dt.Minute, Is.EqualTo(0));
            });
        }

        [Test]
        public void ParseRelaySwimmerEventInfo_ParsesCorrectly()
        {
            SwimmerFunctions swimmerFunctions = new();
            string line = "Sharks 15.23 1";
            string pattern = @"(?<teamName>Sharks)\s(?<seedTime>15.23)\s(?<laneNumber>1)";
            var result = swimmerFunctions.ParseRelaySwimmerEventInfo(line, pattern);
            Assert.Multiple(() =>
            {
                Assert.That(result.Item1, Is.EqualTo("15.23"));
                Assert.That(result.Item2, Is.EqualTo("Sharks"));
                Assert.That(result.Item3, Is.EqualTo("1"));
                Assert.That(result.Item4, Is.False);
            });
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