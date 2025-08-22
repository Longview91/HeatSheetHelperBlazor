using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
