using System.Windows;

namespace ProjectCheetos
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string[] Args;
        public static bool NoArg;
        void app_Startup(object sender, StartupEventArgs e)
        {
            // If no command line arguments were provided, don't process them if (e.Args.Length == 0) return;  
            if (e.Args.Length > 0)
            {
                Args = e.Args;
                NoArg = false;
            }
            else
            {
                NoArg = true;
            }
        }
    }
}
