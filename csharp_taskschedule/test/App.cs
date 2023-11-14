using Microsoft.Win32.TaskScheduler;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace test
{
    internal class App
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.Run(new FormMain());    
        }
    }
}
