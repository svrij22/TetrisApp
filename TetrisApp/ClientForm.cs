using System;
using System.Windows.Forms;

namespace TetrisApp
{
    public partial class ClientForm : Form
    {
        public ClientForm()
        {
            InitializeComponent();
        }

        public Label getServerLabel()
        {
            return label1;
        }

        public RadioButton getRadioButton()
        {
            return radioButton1;
        }
    }
}