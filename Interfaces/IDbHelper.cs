using Microsoft.Data.Sqlite;

namespace HeatSheetHelper.Interfaces
{
    public interface IDbHelper
    {
        SqliteConnection GetInMemoryDBConnection();
    }
}
