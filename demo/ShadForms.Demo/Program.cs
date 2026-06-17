using ShadForms.Demo.Forms;

namespace ShadForms.Demo;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}