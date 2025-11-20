using System;
using System.Windows.Forms;

namespace MotorSimEngine
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var simulator = new MotorSimulator();
            Application.Run(new MotorForm(simulator));
        }
    }
}
