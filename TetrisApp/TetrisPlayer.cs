using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using static TetrisApp.TetrisState;

namespace TetrisApp
{
    public class TetrisPlayer
    {
        public List<TetrisBlock> tetrisBlocks = new List<TetrisBlock>();
        public PictureBox playerBox;
        public int playerScore;
        public Size boxSize;
        public readonly Random rnd;
        private List<Tetrimino> tetriminoList = new List<Tetrimino>();
        public TetrisState state = Started;
        public int lineRemove = -1;

        public TetrisPlayer(PictureBox playerBox, Size boxSize, Random rnd)
        {
            this.playerBox = playerBox;
            this.boxSize = boxSize;
            this.rnd = rnd;
            
            tetriminoList.Add(new Tetrimino(this));
            tetriminoList.Add(new Tetrimino(this));
        }
        
        /*
         * Multiplayer logic and serialization
         */


        public void getBlocksSerialized()
        {
            foreach (var tetrisBlock in tetrisBlocks)
            {
                Debug.WriteLine(tetrisBlock.serialize());
                try
                {
                    Debug.WriteLine(tetrisBlock.fromString(tetrisBlock.serialize()).serialize());
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }
        
        /*
         * GAME LOGIC
         */
        
        public void doGameStep()
        {
            getBlocksSerialized();
            removeWhiteBlocks();
            if (state != GameOver)
            {
                if (!thisPiece().isLocked) thisPiece().move();
            }
            checkLines();
        }
        
        
        /**/
        /* Blocks Logic*/
        /**/
        
        internal void addBlock(TetrisBlock block)
        {
            //Check if the blocks Y position is under < 0
            if (block.position.Y < 0)
            {
                state = GameOver;
                return;
            }
            playerScore += 10;
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
        
        /**/
        /* LINE MUTATIONS*/
        /**/
        
        //Line mutations
        public void checkLines()
        {
            for(var n = 0; n < boxSize.Height; n++)
            {
                var blocksPerLine = 0;
                foreach (var block in tetrisBlocks)
                {
                    if (block.position.Y == n) blocksPerLine += 1;
                    if (blocksPerLine == boxSize.Width)
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

            playerScore += 250;
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
        
        /*
         * Drawing ETC
         */

        public PictureBox getBox()
        {
            return playerBox;
        }
    }
}