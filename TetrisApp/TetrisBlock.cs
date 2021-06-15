using System;
using System.Drawing;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using Brush = System.Drawing.Brush;
using Brushes = System.Drawing.Brushes;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;

namespace TetrisApp
{
    public class TetrisBlock
    {

        public Point position;
        private Brush brush;
        private Color origColor;

        public TetrisBlock(Color color, Point position)
        {
            brush = new SolidBrush(color);
            origColor = color;
            this.position = position;
        }

        public void draw(Graphics g, bool opaque)
        {
            draw(g, opaque, 24);
        }
        /*Draw a block*/
        public void draw(Graphics g, bool opaque, int blockScale)
        {
            /*Get position and rectangle*/
            Point pos = new Point(position.X * blockScale, position.Y * blockScale);
            Rectangle rectangle = new Rectangle(pos, new Size(blockScale, blockScale));
            
            /*Base*/
            
            /*If opaque*/
            if (opaque)
            {
                g.FillRectangle(brush, rectangle);
                g.FillRectangle(new SolidBrush(Color.FromArgb(128, 0, 0, 0)), rectangle);
            }
            else
            {
                g.FillRectangle(brush, rectangle);
                
                //Darker color
                Color slightlyDarker = Color.FromArgb(origColor.A,
                    (int)Math.Max(origColor.R * 0.8, 0), (int)Math.Max(origColor.G * 0.8, 0), (int)Math.Max(origColor.B * 0.8, 0));
                Color slightlyLighter = Color.FromArgb(origColor.A,
                    (int)Math.Min(origColor.R * 1.2, 255), (int)Math.Min(origColor.G * 1.2, 255), (int)Math.Min(origColor.B * 1.2, 255));
                
                //Top
                Point topLeft = new Point(position.X * blockScale + 1, position.Y * blockScale + 1);
                Point downLeft = new Point(topLeft.X + 1, topLeft.Y + blockScale - 1);
                Point topRight = new Point(topLeft.X + blockScale - 1, topLeft.Y + 1);
                Point downRight = new Point(topLeft.X + blockScale -1, topLeft.Y + blockScale -1);
                
                g.DrawLine(new Pen(slightlyLighter, 2), topLeft, topRight);
                g.DrawLine(new Pen(slightlyLighter, 2), topLeft, downLeft);
                
                g.DrawLine(new Pen(slightlyDarker, 2), downRight, topRight);
                g.DrawLine(new Pen(slightlyDarker, 2), downRight, downLeft);
            }
            
            //Shading
            
            /*Borders*/
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