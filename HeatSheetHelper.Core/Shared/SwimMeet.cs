namespace HeatSheetHelper.Core.Shared
{
    public class SwimMeet
    {
        List<SwimEvent> swimEvents;

        public SwimMeet()
        {
            swimEvents = new List<SwimEvent>();
        }

        public List<SwimEvent> SwimEvents
        {
            get { return swimEvents; }
            set { swimEvents = value; }
        }
    }
}
