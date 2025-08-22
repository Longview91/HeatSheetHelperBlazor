using HeatSheetHelper.Core.Helpers;
using HeatSheetHelper.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatSheetHelper.Core.States
{
    public abstract class State
    {
        protected StateContext _context;

        public void SetContext(StateContext context)
        {
            this._context = context;
        }

        public abstract void HandleLine(string line, ref SwimEvent currentEvent, ref HeatInfo currentHeat, ref List<SwimEvent> events);
    }
}
