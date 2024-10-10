using System.Runtime.InteropServices;
using System;
using System.Runtime.InteropServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Application = Microsoft.Maui.Controls.Application;
using Koboldcs.Logger;

namespace Koboldcs
{
    public partial class App : Application
    {
        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();

        public App()
        {
#if WINDOWS
            AllocConsole();
            SLogger.Log(SLogger.LogType.Info, "llama.cpp doesn't provide logs. Using custom instead.");
#endif

            InitializeComponent();

            MainPage = new MainPage();
        }

    }
}
