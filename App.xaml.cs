using HeatSheetHelper.Helpers;
using Microsoft.Data.Sqlite;

namespace HeatSheetHelperBlazor
{
    public partial class App : Application
    {
        internal static SqliteConnection InMemoryConnection;
        public App()
        {
            try
            {
                SQLitePCL.Batteries.Init();
                DbHelper dbHelper = new DbHelper();
                InMemoryConnection = dbHelper.GetInMemoryDBConnection();

                InitializeComponent();

                MainPage = new MainPage();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
