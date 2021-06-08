using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static TetrisApp.TetrisState;

namespace TetrisApp
{
    public class TetrisEngine
    {
        private readonly TetrisForm form;
        private PictureBox drawingBox;
        
        private Font fnt = new Font("Arial", 10);
        private System.Timers.Timer aTimer;
        
        private List<TetrisBlock> tetrisBlocks = new List<TetrisBlock>();
        private int steps;
        private Size gameSize;

        private int defaultInterval = 600;
        
        private List<Tetrimino> tetriminoList = new List<Tetrimino>();

        private int score;
        public readonly Random rnd = new Random();
        
        public TetrisState state = Started;
        public int lineRemove = -1;

        public TetrisEngine(TetrisForm form)
        {
            
            //Form
            this.form = form;
            gameSize = new Size(10, 20); // sideways, downwards
            drawingBox = form.setGameSize(gameSize);
            drawingBox.Paint += paint;
            form.getPreviewPanel().Paint += paintPreview;
            form.KeyPress += keyBoardHandler;
            
            //Game Logic
            tetriminoList.Add(new Tetrimino(this));
            tetriminoList.Add(new Tetrimino(this));
            startTimer();
        }

        public void startTimer()
        {
            // Create a timer and set a two second interval.
            aTimer = new System.Timers.Timer();
            aTimer.Interval = defaultInterval;
            aTimer.Elapsed += this.gameStep;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        //Gamestep
        public void gameStep(object source, System.Timers.ElapsedEventArgs e)
        {
            removeWhiteBlocks();
            if (state != GameOver)
            {
                if (!thisPiece().isLocked) thisPiece().move();
                steps++;
            }
            checkLines();
            redraw();
            aTimer.Interval = defaultInterval;
        }
        
        //Line mutations
        public void checkLines()
        {
            for(var n = 0; n < gameSize.Height; n++)
            {
                var blocksPerLine = 0;
                foreach (var block in tetrisBlocks)
                {
                    if (block.position.Y == n) blocksPerLine += 1;
                    if (blocksPerLine == gameSize.Width)
                    {
                        turnBlocksWhite(n);
                        lineRemove = n;
                    }
                }
            }
        }
        
        //Removes the white blocks when done
        public void removeWhiteBlocks()
        {
            if (lineRemove < 0) return;
            tetrisBlocks = (from block in tetrisBlocks
                                           where block.position.Y != lineRemove
                                           select block).ToList();
            hopBlocksFromHeight(lineRemove);
        }

        //Pushes all blocks from center height 1 position downwards
        private void hopBlocksFromHeight(int height)
        {
            var blocksAboveLine = (from block in tetrisBlocks
                where block.position.Y < lineRemove
                select block).ToList();
                
            foreach (var block in blocksAboveLine)
            {
                block.position.Y++;
            }

            score += 250;
            lineRemove = -1;
        }

        //Turns blocks at height white
        public void turnBlocksWhite(int h)
        {
            foreach (var block in tetrisBlocks)
            {
                if (block.position.Y == h) block.setColor(Color.White);
            }
        }

        public void redraw()
        {
            drawingBox.Invalidate();
            form.getPreviewPanel().Invalidate();
        }

        //Position checking
        public bool positionIsNotFree(Point givenPoint)
        {
            foreach (TetrisBlock block in tetrisBlocks)
            {
                if (block.position == givenPoint) return true;
            }
            return false;
        }

        public bool positionOutOfBounds(Point givenPoint)
        {
            if (givenPoint.X < 0) return true;
            if (givenPoint.X > gameSize.Width-1) return true;
            if (givenPoint.Y > gameSize.Height) return true;

            Console.WriteLine(givenPoint.Y);
            Console.WriteLine(gameSize.Height);
            return false;
        }

        private void keyBoardHandler(object sender, KeyPressEventArgs e)
        {
            thisPiece().keyboardEvent(e);
        }

        public void paint(object sender, PaintEventArgs e) {
            
            //Draw game
            Graphics g = e.Graphics; //New Graphics Object
            g.FillRectangle(Brushes.Black, new Rectangle(new Point(0, 0), new Size(drawingBox.Width, drawingBox.Height))); //Back ground
            thisPiece().draw(g); //Draw moving piece
            foreach (TetrisBlock block in tetrisBlocks) //Draw all blocks
            {
                block.draw(g);
            }
            
            //Draw Score
            form.setPanelInfo(score, state, steps);
        }

        public void paintPreview(object sender, PaintEventArgs e)
        {
            //Preview piece
            Graphics g = e.Graphics; //New Graphics Object
            g.FillRectangle(Brushes.DimGray, new Rectangle(new Point(0, 0), new Size(drawingBox.Width, drawingBox.Height))); //Back ground
            foreach (TetrisBlock block in nextPiece().getPreviewBlocks()) //Draw all blocks
            {
                block.draw(g);
            }
        }

        internal void addBlock(TetrisBlock block)
        {
            //Check if the blocks Y position is under < 0
            if (block.position.Y < 0)
            {
                state = GameOver;
                return;
            }
            this.score += 10;
            tetrisBlocks.Add(block);
        }

        internal void newPiece()
        {
            if (state == GameOver) return;
            
            Console.WriteLine(tetriminoList);
            tetriminoList.RemoveAt(0);
            tetriminoList.Add(new Tetrimino(this));
        }

        public Tetrimino thisPiece()
        {
            return tetriminoList[0];
        }

        public Tetrimino nextPiece()
        {
            return tetriminoList[1];
        }

        internal void speedUp()
        {
            if (aTimer.Interval > 100) aTimer.Interval = 100;
        }
    }

}