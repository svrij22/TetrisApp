using System.Drawing;

namespace TetrisApp
{
    public class TetrisBlock
    {
        public Point position = new Point(0, 0);
        private Brush brush;

        public TetrisBlock(Color color)
        {
            brush = new SolidBrush(color);
        }
        public TetrisBlock(Color color, Point position)
        {
            brush = new SolidBrush(color);
            this.position = position;
        }

        public Point getGridPosition(Point position)
        {
            return new Point(position.X * 16, position.Y * 16);
        }

        public void draw(Graphics g)
        {
            Rectangle rectangle = new Rectangle(getGridPosition(position), new Size(16, 16));
            g.FillRectangle(brush, rectangle);
        }

        public void setColor(Color color)
        {
            brush = new SolidBrush(color);
        }
    }
}