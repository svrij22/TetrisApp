using System;
using System.Drawing;
using System.Windows.Forms;

namespace TetrisApp
{
    public partial class TetrisForm : Form
    {
        public TetrisForm()
        {
            InitializeComponent();
        }
        
        public PictureBox setGameSize(Size size)
        {
            var pictureBox = new PictureBox
            {
                Name = "Tetris Drawing Box",
                Size = new Size(size.Width* 16, size.Height*16),
                Location = new Point(16, 16),
            };
            Controls.Add(pictureBox);
            
            /*Dynamic size*/
            panel1.Width = 124;
            Size = new Size((size.Width * 16) + 64 + panel1.Width, (size.Height * 16) + 72);
            panel1.Left = size.Width * 16 + 32;
            panel1.Top = 16;
            panel1.Height = Size.Height - 72;
            
            Show();
            return pictureBox;
        }

        public void setPanelInfo(int score, TetrisState state, int steps)
        {
            label1.Text = "Score: " + score;
            label2.Text = state + " " + steps;
        }

        public Panel getPreviewPanel()
        {
            return panel2;
        }
    }
}
