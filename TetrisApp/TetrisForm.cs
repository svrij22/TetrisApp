using System;
using System.Drawing;
using System.Windows.Forms;
using TetrisApp.Properties;

namespace TetrisApp
{
    public partial class TetrisForm : Form
    {
        public PictureBox localBox;
        public PictureBox mpBox;

        public int blockScale = 24;
        
        public TetrisForm()
        {
            InitializeComponent();
        }
        
        public void setGameSize(Size size, bool multiplayer)
        {
     
            
            /*Picture box for drawing*/
            localBox = new PictureBox
            {
                Name = "Tetris Drawing Box",
                Size = new Size(size.Width * blockScale, size.Height * blockScale),
                Location = new Point(16, 16),
            };
            localBox.BackgroundImage = Resources.BackG;
            Controls.Add(localBox);

            /*Dynamic size*/
            panel1.Width = 124;
            Size = new Size((size.Width * blockScale) + 64 + panel1.Width, (size.Height * blockScale) + 72);
            panel1.Left = size.Width * blockScale + 32;
            panel1.Top = 16;
            panel1.Height = Size.Height - 72;
            
            //Kill application when closing
            Closing += (sender, args) =>
            {
                Application.Exit();
            };
            
            /*Multiplayer other box*/
            if (multiplayer)
            {
                int tempWidth = Size.Width;
                mpBox = new PictureBox
                {
                    Name = "Tetris Drawing Box 2",
                    Size = new Size(size.Width * blockScale, size.Height * blockScale),
                    Location = new Point(tempWidth - 16, 16),
                };
                Size = new Size((size.Width * blockScale) + tempWidth + 16, (size.Height * blockScale) + 72);
                Controls.Add(mpBox);
            }
            
            Show();
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

        public PictureBox getLocalPlayerBox()
        {
            return localBox;
        }

        public PictureBox getOtherPlayerBox()
        {
            return mpBox;
        }
    }
}
