using System;
using Jacobi.Vst.Interop.Host;
using System.Diagnostics;
using System.Windows.Forms;

namespace JmsVstHost
{
    class Program
    {
        static void Main(string[] args)
        {
            string pluginPath = args[1];
            var Plugin = new VSTiInterface(pluginPath);

            var stream = Console.OpenStandardInput();
            try
            {
                while (true)
                {
                    Plugin.ReadCommand(stream);
                }
            }
            catch (Exception e) {
            }
        }
    }
}
