using Dapper;
using HeatSheetHelper.Interfaces;
using Microsoft.Data.Sqlite;
using System.Collections;

namespace HeatSheetHelper.Helpers
{
    public class DbHelper : IDbHelper
    {
        private SqliteConnection InMemoryDbConnection = null;
        public DbHelper()
        {

        }
        public SqliteConnection GetInMemoryDBConnection()
        {
            if (InMemoryDbConnection == null)
            {
                var connectionString = new SqliteConnectionStringBuilder("DataSource=:memory:")
                {
                    Mode = SqliteOpenMode.ReadWriteCreate,
                    ForeignKeys = true
            }.ToString();
                InMemoryDbConnection = new SqliteConnection(connectionString);
                InMemoryDbConnection.Open();
                string commandLine = "CREATE TABLE Event (eventId integer primary key AUTOINCREMENT, eventNumber integer, eventName varchar(50), eventType varchar(50), unitType varchar(5), isRelay boolean);\r\n";
                    commandLine += "CREATE TABLE Heat (heatId integer primary key autoincrement, eventId integer, heatNumber integer, startTime varchar(50), FOREIGN KEY (eventId) references Event(eventId));\r\n";
                    commandLine += "CREATE TABLE Swimmer (swimmerId integer primary key AUTOINCREMENT, heatId integer, name varchar(100), age varchar(3), laneNumber varchar(4), teamName varchar(50), seedTime varchar(50), FOREIGN KEY (heatId) references Heat(heatId));";
                CommandDefinition commandDef = new CommandDefinition(commandLine);
                InMemoryDbConnection.Execute(commandDef);
            }
            return InMemoryDbConnection;
        }
        internal async static void TruncateTables(SqliteConnection connection)
        {
            string commandLine = "DELETE FROM Swimmer;\r\n";
            commandLine += "DELETE FROM Heat;\r\n";
            commandLine += "DELETE FROM Event;";
            CommandDefinition commandDef = new CommandDefinition(commandLine);
            await connection.ExecuteAsync(commandDef);
        }
        internal static List<string> PopulateEventPicker(SqliteConnection connection)
        {
            string commandLine = "SELECT eventNumber FROM Event";
            CommandDefinition commandDef = new CommandDefinition(commandLine);
            List<string> eventNums = connection.Query<string>(commandDef).ToList();
            return eventNums;
        }
        internal static async Task<IList> PopulateHeatPicker(SqliteConnection connection, string eventNum)
        {
            var parameter = new DynamicParameters();
            parameter.Add("@eventNum", eventNum);

            string commandLine = "SELECT h.heatNumber FROM Heat h ";
            commandLine += "JOIN Event e ON h.eventId = e.eventId WHERE e.eventNumber = @eventNum;";
            List<int> heatNums = (await connection.QueryAsync<int>(commandLine, param: parameter)).ToList();
            return heatNums;
        }

        internal static async Task<string> CountHeats(SqliteConnection connection, string eventFrom, string heatFrom, string eventTo, string heatTo)
        {
            //Use eventFrom and eventTo - 1 to get all of the heats between. Then subtract the heatTo - 1 and add the heatFrom to get the right count
            if (int.Parse(eventFrom) == int.Parse(eventTo))
            {
                var heatCount = int.Parse(heatTo) - int.Parse(heatFrom);
                return (heatCount > 0 ? heatCount.ToString() : "0");
            }
            else if (int.Parse(eventFrom) > int.Parse(eventTo))
            {
                return "0";
            }
            else
            {
                var parameter = new DynamicParameters();
                parameter.Add("@eventFrom", eventFrom);
                parameter.Add("@eventTo", int.Parse(eventTo) - 1);
    
                string commandLine = "SELECT COUNT(h.heatNumber) FROM Heat h ";
                commandLine += "JOIN Event e ON h.eventId = e.eventId WHERE e.eventNumber BETWEEN @eventFrom AND @eventTo;";
                int heatCount = await connection.ExecuteScalarAsync<int>(commandLine, param: parameter);
                return (heatCount - int.Parse(heatFrom) + int.Parse(heatTo) - 1).ToString();
            }
        }
    }
}
