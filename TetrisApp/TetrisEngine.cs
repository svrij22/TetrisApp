using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TetrisApp
{
    public class TetrisEngine
    {
        public TetrisForm tetrisForm;
        private Font fnt = new Font("Arial", 10);
        private System.Timers.Timer aTimer;
        private int steps;
        private Size gameSize;
        private int defaultInterval = 480;

        private TetrisPlayer localPlayer;
        private TetrisPlayer otherPlayer;

        private bool isMultiplayer;
        public TetrisClient tetrisClient;

        public TetrisEngine(TetrisForm form, bool ismultiplayerm, TetrisClient client, Random ownRandom, Random otherRandom)
        {
            tetrisClient = client;
            setup(form, ismultiplayerm, ownRandom, otherRandom);
        }

        public void setup(TetrisForm form, bool ismultiplayer, Random ownRandom, Random otherRandom)
        {
            tetrisForm = form;
            
            //Set multiplayer
            isMultiplayer = ismultiplayer;
            
            //Set gamesize
            gameSize = new Size(10, 15); // sideways, downwards
            
            //Set tetrisForm size
            tetrisForm.setGameSize(gameSize, isMultiplayer);
            
            //Get local box
            localPlayer = new TetrisPlayer(tetrisForm.getLocalPlayerBox(), gameSize, ownRandom);
            
            //Set paint event
            tetrisForm.getLocalPlayerBox().Paint += (o, args) => paintForPlayer(args, localPlayer, true);
            
            //Set keypress event
            tetrisForm.getPreviewPanel().Paint += paintPreview;
            tetrisForm.KeyPress += keyBoardHandler;
            
            //If multiplayer active create other player and set paint event
            if (isMultiplayer)
            {
                otherPlayer = new TetrisPlayer(tetrisForm.getOtherPlayerBox(), gameSize, otherRandom);
                tetrisForm.getOtherPlayerBox().Paint += (o, args) => paintForPlayer(args, otherPlayer, false);
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
            aTimer.Elapsed += gameStep;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }
        
        /*
         * Multiplayer
         */

        public void otherPlayerKeyPress(char key)
        {
            otherPlayer.thisTetromino().keyboardEvent(new KeyPressEventArgs(key));
            redraw();
        }

        private void keyBoardHandler(object sender, KeyPressEventArgs e)
        {
            localPlayer.thisTetromino().keyboardEvent(e);
            tetrisClient?.sendKeyPress(e.KeyChar);
            redraw();
        }
        
        public void otherPlayerSetGrid(String grid)
        {
            otherPlayer.blocksFromString(grid);
        }

        /*
         * Game logic
         */

        //Gamestep
        public void gameStep(object source, System.Timers.ElapsedEventArgs e)
        {
            
            //Check game over
            gameEndEvent();
            
            //Game step
            localPlayer.doGameStep();
            otherPlayer?.doGameStep();
            steps++;
            
            /*Every 15 steps*/
            if (steps % 15 == 0)
            {
                tetrisClient.SendSerializedGrid(localPlayer.serializeBlocks());
            }
            
            redraw();
        }

        /*
         * Game End event -> When one or two players are game over, stop the timers and show a message box with your score.
         */
        public void gameEndEvent()
        {
            if (!isMultiplayer)
            {
                if (localPlayer.state == TetrisState.GameOver)
                {
                    aTimer.Enabled = false;
                    string caption = "Game over";
                    string message = "Your score: " + localPlayer.playerScore;
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    MessageBox.Show(message, caption, buttons);
                }
            }
            else
            {
                if (localPlayer.isGameOver() && otherPlayer.isGameOver())
                {
                    var hasWon = localPlayer.playerScore > otherPlayer.playerScore;
                    aTimer.Enabled = false;
                    string caption = hasWon ? "You win!" : "Game over sucker";
                    string message = "Your score: " + localPlayer.playerScore + " vs " + otherPlayer.playerScore;
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    MessageBox.Show(message, caption, buttons);
                }
            }
        }
        
        /*
         * Invalidates the panels and forces them to execute their paint event
         */

        public void redraw()
        {
            localPlayer.getBox().Invalidate();
            tetrisForm.getPreviewPanel().Invalidate();
            otherPlayer?.getBox().Invalidate();
        }

        /*
         * Drawing
         * Each 'player' has their own logic.
         */
        public void paintForPlayer(PaintEventArgs e, TetrisPlayer player, bool drawScore) {
            
            //Draw game
            Graphics g = e.Graphics; //New Graphics Object
            
            //Get box
            PictureBox drawingBox = player.getBox();
            
            //Fill box /w Back ground
            g.FillRectangle(Brushes.Black, new Rectangle(new Point(0, 0), new Size(drawingBox.Width, drawingBox.Height)));
            
            //Draw moving piece
            player.thisTetromino().drawGhost(g);
            player.thisTetromino().draw(g);
            
            //Draw all blocks
            foreach (TetrisBlock block in player.tetrisBlocks) 
            {
                block.draw(g, false);
            }

            //Draw Score
            if (drawScore)
            {
                tetrisForm.setPanelInfo(player.playerScore, player.state, steps);
            }
        }

        /*
         * Paint the preview panel, paint the next tetrimino
         */
        public void paintPreview(object sender, PaintEventArgs e)
        {
            //Preview piece
            Graphics g = e.Graphics; //New Graphics Object

            //Get panel
            Panel prevPanel = tetrisForm.getPreviewPanel();
            
            //Fill background
            g.FillRectangle(Brushes.DimGray, new Rectangle(new Point(0, 0), new Size(prevPanel.Width, prevPanel.Height)));
            
            //Draw for local player
            foreach (TetrisBlock block in localPlayer.nextTetromino().getPreviewBlocks())
            {
                block.draw(g, false);
            }
        }

        public void stopTimer()
        {
            aTimer.Enabled = false;
        }
    }

}