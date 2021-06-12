using System;
using System.Drawing;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using Brush = System.Drawing.Brush;
using Color = System.Drawing.Color;

namespace TetrisApp
{
    public class TetrisBlock
    {

        public Point position;
        private Brush brush;

        public TetrisBlock(Color color, Point position)
        {
            brush = new SolidBrush(color);
            this.position = position;
        }

        public void draw(Graphics g, bool opaque)
        {
            Point pos = new Point(position.X * 16, position.Y * 16);
            Rectangle rectangle = new Rectangle(pos, new Size(16, 16));
            g.FillRectangle(brush, rectangle);
            if (opaque)
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(128, 0, 0, 0)), rectangle);
            }
        }

        public void setColor(Color color)
        {
            brush = new SolidBrush(color);
        }
        
        /*
         * Serialization for optimal game sync
         */

        public string serialize()
        {
            return "||"+position.X+","+position.Y+","+((SolidBrush) brush).Color.ToArgb();
        }

        public static TetrisBlock fromString(string serializedBlock)
        {
            var splitted = serializedBlock.Split(',');
            Color color = Color.FromArgb(Convert.ToInt32(splitted[2]));
            Point point = new Point(Convert.ToInt32(splitted[0]), Convert.ToInt32(splitted[1]));
            return new TetrisBlock(color, point);
        }
    }
}