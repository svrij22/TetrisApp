using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace TetrisApp
{
    static class TetrisApp
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var clientForm = new ClientForm();
            Application.Run(clientForm);
            
            //var form = new TetrisForm();
            //new TetrisEngine(form);
            //Application.Run(form);
        }
    }
}
