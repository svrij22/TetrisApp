using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TetrisApp
{
    public class TetrisEngine
    {
        private TetrisForm tetrisForm;
        private Font fnt = new Font("Arial", 10);
        private System.Timers.Timer aTimer;
        private int steps;
        private Size gameSize;
        private int defaultInterval = 600;

        private TetrisPlayer localPlayer;
        private TetrisPlayer otherPlayer;

        private bool isMultiplayer;
        public TetrisClient tetrisClient;
        
        private Random rand;

        public TetrisEngine(TetrisForm form, bool ismultiplayerm, TetrisClient client)
        {
            tetrisClient = client;
            rand = new Random(Guid.NewGuid().GetHashCode());
            client?.syncRandom();
            setup(form, ismultiplayerm);
        }

        public void setup(TetrisForm form, bool ismultiplayer)
        {
            tetrisForm = form;
            
            //Set multiplayer
            isMultiplayer = ismultiplayer;
            
            //Set gamesize
            gameSize = new Size(20, 15); // sideways, downwards
            
            //Set tetrisForm size
            tetrisForm.setGameSize(gameSize, isMultiplayer);
            
            //Get local box
            localPlayer = new TetrisPlayer(tetrisForm.getLocalPlayerBox(), gameSize, rand, this);
            
            //Set paint event
            tetrisForm.getLocalPlayerBox().Paint += (o, args) => paintForPlayer(args, localPlayer, true);
            
            //Set keypress event
            tetrisForm.getPreviewPanel().Paint += paintPreview;
            tetrisForm.KeyPress += keyBoardHandler;
            
            if (isMultiplayer)
            {
                otherPlayer = new TetrisPlayer(tetrisForm.getOtherPlayerBox(), gameSize, rand, this);
                tetrisForm.getOtherPlayerBox().Paint += (o, args) => paintForPlayer(args, otherPlayer, false);
            }
            
            //Start timer
            startTimer();
            
            //TODO ghost piece maken
            //TODO als client wordt gesloten kill het project
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
        
        /*
         * Multiplayer
         */

        public void otherPlayerKeyPress(char key)
        {
            otherPlayer.thisPiece().keyboardEvent(new KeyPressEventArgs(key));
            redraw();
        }

        private void keyBoardHandler(object sender, KeyPressEventArgs e)
        {
            localPlayer.thisPiece().keyboardEvent(e);
            tetrisClient?.sendKeyPress(e.KeyChar);
            redraw();
        }
        
        public void otherPlayerSetGrid(String grid)
        {
            otherPlayer.blocksFromString(grid);
        }

        public void otherPlayerSetRand(int i)
        {
            otherPlayer.setRandom(new Random(i));
        }

        /*
         * Game logic
         */

        //Gamestep
        public void gameStep(object source, System.Timers.ElapsedEventArgs e)
        {
            localPlayer.doGameStep();
            if (isMultiplayer)
            {
                otherPlayer.doGameStep();
            }
            steps++;
            
            /*Every 15 steps*/
            if (steps % 15 == 0)
            {
                tetrisClient.SendSerializedGrid(localPlayer.serializeBlocks());
            }
            
            redraw();
            aTimer.Interval = defaultInterval;
        }

        public void redraw()
        {
            localPlayer.getBox().Invalidate();
            tetrisForm.getPreviewPanel().Invalidate();
            
            if (isMultiplayer)
            {
                otherPlayer.getBox().Invalidate();
            }
        }

        /*
         * Drawing
         */
        public void paintForPlayer(PaintEventArgs e, TetrisPlayer player, bool drawScore) {
            
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
            if (drawScore)
            {
                tetrisForm.setPanelInfo(player.playerScore, player.state, steps);
            }
        }

        public void paintPreview(object sender, PaintEventArgs e)
        {
            //Preview piece
            Graphics g = e.Graphics; //New Graphics Object

            //Get panel
            Panel prevPanel = tetrisForm.getPreviewPanel();
            
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