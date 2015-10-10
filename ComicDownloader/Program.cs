using log4net;
using System;
using System.Windows.Forms;

namespace ComicDownloader
{
    static class Program
    {
        private static ILog _Log;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo("./log4net.xml"));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _Log = LogManager.GetLogger("MyLogger");

            _Log.Info("App Start");

            Application.ApplicationExit += Application_ApplicationExit;

            Application.Run(new Form1(_Log));
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            _Log.Info("App Exit");
        }
    }
}
