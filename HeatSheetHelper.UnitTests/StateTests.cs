using HeatSheetHelper.Core.Helpers;
using HeatSheetHelper.Core.Shared;
using HeatSheetHelper.Core.States;

namespace HeatSheetHelper.UnitTests
{
    public class StateTests
    {
        [SetUp]
        public void Setup()
        {
            // Optional: Initialize resources before each test
        }

        [Test]
        public void HandleLine_SkipLineState_DoesNotModifyReferences()
        {
            string line = "THIS IS A LINE TO BE SKIPPED";
            SwimEvent currentEvent = new SwimEvent
            {
                EventNumber = 1,
                EventDetails =
                "Freestyle"
            };
            DateTime startTime = DateTime.Now;
            HeatInfo currentHeat = new HeatInfo { HeatNumber = 1, StartTime = startTime.ToString() };
            List<SwimEvent> events = new List<SwimEvent> { currentEvent };
            StateContext context = new StateContext(new StateFactory().CreateState(line));
            context.GetCurrentState().HandleLine(line, ref currentEvent, ref currentHeat, ref events);

            Assert.Multiple(() =>
            {
                Assert.That(currentEvent.EventNumber, Is.EqualTo(1));
                Assert.That(currentEvent.EventDetails, Is.EqualTo("FREESTYLE"));
                Assert.That(currentHeat.HeatNumber, Is.EqualTo(1));
                Assert.That(events.Count, Is.EqualTo(1));
            });
        }

        [Test]
        public void HandleLine_EventState_ParsesEventCorrectly()
        {
            string line = "#12 200 FREESTYLE";
            SwimEvent currentEvent = null;
            HeatInfo currentHeat = null;
            List<SwimEvent> events = new List<SwimEvent>();
            StateContext context = new StateContext(new StateFactory().CreateState(line));
            context.GetCurrentState().HandleLine(line, ref currentEvent, ref currentHeat, ref events);

            Assert.Multiple(() =>
            {
                Assert.That(currentEvent.EventNumber, Is.EqualTo(12));
                Assert.That(currentEvent.EventDetails, Is.EqualTo("200 FREESTYLE"));
                Assert.That(currentEvent.IsRelay, Is.False);
                Assert.That(events.Count, Is.EqualTo(0)); // No previous event to add
            });
        }

        [Test]
        public void HandleLine_EventState_ParsesRelayEventCorrectly()
        {
            string line = "#12 200 FREESTYLE RELAY";
            SwimEvent currentEvent = null;
            HeatInfo currentHeat = null;
            List<SwimEvent> events = new List<SwimEvent>();
            StateContext context = new StateContext(new StateFactory().CreateState(line));
            context.GetCurrentState().HandleLine(line, ref currentEvent, ref currentHeat, ref events);

            Assert.Multiple(() =>
            {
                Assert.That(currentEvent.EventNumber, Is.EqualTo(12));
                Assert.That(currentEvent.EventDetails, Is.EqualTo("200 FREESTYLE RELAY"));
                Assert.That(currentEvent.IsRelay, Is.True);
                Assert.That(events.Count, Is.EqualTo(0)); // No previous event to add
            });
        }

    }
}
