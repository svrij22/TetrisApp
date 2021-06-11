using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TetrisApp
{
    public class TetrisEngine
    {
        private readonly TetrisForm form;
        private Font fnt = new Font("Arial", 10);
        private System.Timers.Timer aTimer;
        private int steps;
        private Size gameSize;
        private int defaultInterval = 600;
        private int score;

        private TetrisPlayer localPlayer;
        private TetrisPlayer otherPlayer;

        private bool isMultiplayer;
        
        private static readonly Random rnd = new Random();

        public TetrisEngine(TetrisForm form, bool ismultiplayer)
        {
            this.form = form;
            
            //Set multiplayer
            isMultiplayer = ismultiplayer;
            
            //Set gamesize
            gameSize = new Size(15, 5); // sideways, downwards
            
            //Set form size
            form.setGameSize(gameSize, isMultiplayer);
            
            //Get local box
            localPlayer = new TetrisPlayer(form.getLocalPlayerBox(), gameSize, rnd);
            
            //Set paint event
            form.getLocalPlayerBox().Paint += (o, args) => paintForPlayer(args, localPlayer);
            
            //Set keypress event
            form.getPreviewPanel().Paint += paintPreview;
            form.KeyPress += keyBoardHandler;
            
            if (isMultiplayer)
            {
                otherPlayer = new TetrisPlayer(form.getOtherPlayerBox(), gameSize, rnd);
                form.getOtherPlayerBox().Paint += (o, args) => paintForPlayer(args, otherPlayer);
            }
            
            //Start timer
            startTimer();
            
            //TODO ghost piece maken
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
            localPlayer.doGameStep();
            if (isMultiplayer)
            {
                otherPlayer.doGameStep();
            }
            steps++;
            redraw();
            aTimer.Interval = defaultInterval;
        }

        public void redraw()
        {
            localPlayer.getBox().Invalidate();
            form.getPreviewPanel().Invalidate();
            
            if (isMultiplayer)
            {
                otherPlayer.getBox().Invalidate();
            }
        }

        private void keyBoardHandler(object sender, KeyPressEventArgs e)
        {
            localPlayer.thisPiece().keyboardEvent(e);
            redraw();
        }

        public void paintForPlayer(PaintEventArgs e, TetrisPlayer player) {
            
            //Draw game
            Graphics g = e.Graphics; //New Graphics Object
            
            //Get box
            PictureBox drawingBox = player.getBox();
            
            //Fill box /w Back ground
            g.FillRectangle(Brushes.Black, new Rectangle(new Point(0, 0), new Size(drawingBox.Width, drawingBox.Height)));
            
            //Draw moving piece
            player.thisPiece().draw(g);
            
            //Draw all blocks
            foreach (TetrisBlock block in player.tetrisBlocks) 
            {
                block.draw(g);
            }
            
            //Draw Score
            form.setPanelInfo(score, player.state, steps);
        }

        public void paintPreview(object sender, PaintEventArgs e)
        {
            //Preview piece
            Graphics g = e.Graphics; //New Graphics Object

            //Get panel
            Panel prevPanel = form.getPreviewPanel();
            
            //Fill background
            g.FillRectangle(Brushes.DimGray, new Rectangle(new Point(0, 0), new Size(prevPanel.Width, prevPanel.Height)));
            
            //Draw for local player
            foreach (TetrisBlock block in localPlayer.nextPiece().getPreviewBlocks())
            {
                block.draw(g);
            }
        }
    }

}