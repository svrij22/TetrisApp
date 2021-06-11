using System;
using System.Windows.Forms;

namespace TetrisApp
{
    static class TetrisApp
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //var clientForm = new ClientForm();
            //new TetrisClient(clientForm);
            //Application.Run(clientForm);

            var form = new TetrisForm();
            new TetrisEngine(form, true);
            Application.Run(form);
        }
    }
}
