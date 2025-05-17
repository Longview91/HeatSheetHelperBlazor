using HeatSheetHelper.Helpers;
using Microsoft.Data.Sqlite;

namespace HeatSheetHelperBlazor
{
    public partial class App : Application
    {
        public App()
        {
            try
            {
                SQLitePCL.Batteries.Init();

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
