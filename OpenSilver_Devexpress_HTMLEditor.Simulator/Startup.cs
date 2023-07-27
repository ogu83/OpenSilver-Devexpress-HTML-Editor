using OpenSilver.Simulator;
using System;

namespace OpenSilver_Devexpress_HTMLEditor.Simulator
{
    internal static class Startup
    {
        [STAThread]
        static int Main(string[] args)
        {
            return SimulatorLauncher.Start(typeof(App));
        }
    }
}
