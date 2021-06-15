using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static TetrisApp.TetrisState;

namespace TetrisApp
{
    public class TetrisPlayer
    {
        public List<TetrisBlock> tetrisBlocks = new List<TetrisBlock>();
        public PictureBox playerBox;
        public int playerScore;
        public Size boxSize;
        public Random rnd;
        private List<Tetromino> tetrominoList = new List<Tetromino>();
        public TetrisState state = Started;

        public TetrisPlayer(PictureBox playerBox, Size boxSize, Random rnd)
        {
            this.playerBox = playerBox;
            this.boxSize = boxSize;
            this.rnd = rnd;

            //Adds the first and next piece to the list
            tetrominoList.Add(new Tetromino(this));
            tetrominoList.Add(new Tetromino(this));
        }

        /*
         * Multiplayer logic and serialization
         */

        public string serializeBlocks()
        {
            StringWriter stringWriter = new StringWriter();
            foreach (var tetrisBlock in tetrisBlocks)
            {
                stringWriter.Write(tetrisBlock.serialize());
            }

            return stringWriter.ToString();
        }

        public void blocksFromString(String blocksStr)
        {
            tetrisBlocks = new List<TetrisBlock>();
            foreach (var blockStr in blocksStr.Split('|'))
            {
                try
                {
                    tetrisBlocks.Add(TetrisBlock.fromString(blockStr));
                }
                catch (Exception ignored)
                {
                }
            }
        }

        /*
         * GAME LOGIC
         */

        public void doGameStep()
        {
            /*If not game over*/
            if (state != GameOver)
            {
                /*If cant move*/
                if (!thisTetromino().move())
                {
                    /*Lock blocks and new tetrimino*/
                    foreach (TetrisBlock block in thisTetromino().getBlocksForPiece())
                    {
                        addBlockToField(block);
                    }
                    newTetromino();
                }
            }
            checkLines();
        }

        /**/
        /* Blocks Logic*/
        /**/

        internal void addBlockToField(TetrisBlock block)
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

        internal void newTetromino()
        {
            if (state == GameOver) return;

            tetrominoList.RemoveAt(0);
            Tetromino newTet = new Tetromino(this);
            tetrominoList.Add(newTet);
        }

        public Tetromino thisTetromino()
        {
            return tetrominoList[0];
        }

        public Tetromino nextTetromino()
        {
            return tetrominoList[1];
        }

        /**/
        /* LINE MUTATIONS*/
        /**/

        public void checkLines()
        {
            //Map all horizontal lines with the amount of blocks present
            Dictionary<int, int> lineMap = new Dictionary<int, int>();
            tetrisBlocks.ForEach(b =>
            {
                lineMap[b.position.Y] = lineMap.ContainsKey(b.position.Y) ? lineMap[b.position.Y] + 1 : 1;
            });

            //Check for each line how much blocks are present, and if equal to game width, remove them
            lineMap.Keys.ToList().ForEach(yPos =>
            {
                if (lineMap[yPos] == boxSize.Width)
                {
                    tetrisBlocks = tetrisBlocks.Where(b => b.position.Y != yPos).ToList();
                    hopBlocksFromHeight(yPos);
                }
            });
        }

        //Pushes all blocks from center height 1 position downwards
        private void hopBlocksFromHeight(int height)
        {
            tetrisBlocks.ToList().ForEach(b =>
            {
                if (b.position.Y < height) b.position.Y++;
            });
            playerScore += 250;
        }

        /*
         * Drawing ETC
         */

        public PictureBox getBox()
        {
            return playerBox;
        }

        public bool isGameOver()
        {
            return state == GameOver;
        }
    }
}