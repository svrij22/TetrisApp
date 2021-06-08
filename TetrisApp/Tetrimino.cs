﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TetrisApp
{
    public class Tetrimino
    {

        public int[,] pieceMatrix;
        public Point position;
        public Color color;
        public TetrisEngine Engine;
        public Boolean isLocked;

        public Tetrimino(TetrisEngine engine)
        {
            //Set game
            this.Engine = engine;

            //Random color and size
            pieceMatrix = getRandPiece();
            setRandColor();

            //Set random point thats slightly visible
            do
            {
                position = new Point(engine.rnd.Next(7) + 1, -2);
            } while (!canMove(0, 0, getPositionArray(pieceMatrix, 0, 0)));
            
            //Rotate randomly
            for (var i = 0; i < engine.rnd.Next(3); i++)
            {
                RotateMatrix(pieceMatrix, 3, true);
            }
            
            engine.redraw();
        }

        //Keyboard events
        public void keyboardEvent(KeyPressEventArgs e)
        {
            if (isLocked) return;

            if (e.KeyChar.Equals('w'))
            {
                attemptRotation(true);
            }
            if (e.KeyChar.Equals('s'))
            {
                attemptRotation(false);
            }
            if (e.KeyChar.Equals('a'))
            {
                if (canMove(-1 ,0, getPositionArray(pieceMatrix,0,0)))
                {
                    this.position.X -= 1;
                }
            }
            if (e.KeyChar.Equals('d'))
            {
                if (canMove(1, 0, getPositionArray(pieceMatrix,0,0)))
                {
                    this.position.X += 1;
                }
            }
            if (e.KeyChar.Equals('e'))
            {
                Engine.speedUp();
            }
            Engine.redraw();
        }

        public void attemptRotation(bool clockwise)
        {
            //Normal rotation
            int[,] kickposArr = {{0, 0}, {-1, 0}, {1, 0}};
            
            for (int k = 0; k < kickposArr.GetLength(0); k++)
            {
                int dx = kickposArr[k, 0];
                int dy = kickposArr[k, 1];
                if (canRotate(clockwise, dx, dy))
                {
                    pieceMatrix = RotateMatrix(pieceMatrix, 3, clockwise);
                    position.X += dx;
                    position.Y += dy;
                    return;
                }
            }
        }

        //Check if rotation is possible
        public bool canRotate(bool clockwise, int dx, int dy)
        {
            //When rotated, only get possible new positions
            int[,] rotatedMatrix = RotateMatrix(pieceMatrix, 3, clockwise);
            List<Point> rotatedPositions = getPositionArray(rotatedMatrix,dx,dy);
            return canMove(dx, dy, rotatedPositions);
        }

        //Get random Tetris Piece
        public int[,] getRandPiece()
        {
            int[,] array2D = { { 0, 1, 0}, { 1, 1, 0}, { 0, 1, 0} };
            switch (Engine.rnd.Next(8))
            {
                case 0: // Small T
                    array2D = new[,] { { 0, 1, 0}, { 1, 1, 0}, { 0, 1, 0} };
                    break;
                case 1: // |_
                    array2D = new[,] { { 0, 0, 0 }, { 0, 1, 1 }, { 0, 1, 0 } };
                    break;
                case 2:// L
                    array2D = new[,] { { 1, 1, 0 }, { 0, 1, 0 }, { 0, 1, 0 } };
                    break;
                case 3:// L Mirrored
                    array2D = new[,] { { 0, 1, 1 }, { 0, 1, 0 }, { 0, 1, 0 } };
                    break;
                case 4: // Long boy
                    array2D = new[,] { { 0, 1, 0 }, { 0, 1, 0 }, { 0, 1, 0 } };
                    break;
                case 5: // Z
                    array2D = new[,] { { 1, 0, 0 }, { 1, 1, 0 }, { 0, 1, 0 } };
                    break;
                case 6: // Z Mirror
                    array2D = new[,] { { 0, 0, 1 }, { 0, 1, 1 }, { 0, 1, 0 } };
                    break;
                case 7: // Block
                    array2D = new[,] { { 0, 1, 1 }, { 0, 1, 1 }, { 0, 0, 0 } };
                    break;
            }
            return array2D;
        }

        public void draw(Graphics g)
        {
            foreach (TetrisBlock block in getBlocksForPiece())
            {
                block.draw(g);
            }
        }

        public List<TetrisBlock> getBlocksForPiece()
        {
            List<TetrisBlock> blocks = new List<TetrisBlock> { };
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    if (pieceMatrix[i, j] == 1) blocks.Add(new TetrisBlock(color, new Point(i + position.X, j + position.Y)));
                }
            }
            return blocks;
        }

        public List<Point> getPositionArray(int[,] pieceArray, int xOffset, int yOffset)
        {
            List<Point> positions = new List<Point> { };
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    if (pieceArray[i, j] == 1) positions.Add(new Point(i + position.X + xOffset, j + position.Y + yOffset ));
                }
            }
            return positions;
        }
        
        public List<TetrisBlock> getPreviewBlocks()
        {
            List<TetrisBlock> blocks = new List<TetrisBlock> { };
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    if (pieceMatrix[i, j] == 1) blocks.Add(new TetrisBlock(color, new Point(i+1, j+1)));
                }
            }
            return blocks;
        }

        public void setRandColor()
        {
            Color randomColor = Color.FromArgb(Engine.rnd.Next(128) + 128, Engine.rnd.Next(128) + 128, Engine.rnd.Next(128) + 128);
            color = randomColor;
        }

        static int[,] RotateMatrix(int[,] matrix, int n, Boolean clockwise)
        {
            int[,] ret = new int[n, n];
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                {
                    if (!clockwise)
                        ret[i, j] = matrix[n - j - 1, i];
                    else
                        ret[i, j] = matrix[j, n - i - 1];
                }
            }
            return ret;
        }
        public Boolean canMove(int dx, int dy, List<Point> positionArray)
        {
            foreach (Point point in positionArray)
                if (Engine.positionIsNotFree(new Point(point.X + dx, point.Y + dy))) return false;
            foreach (Point point in positionArray)
                if (Engine.positionOutOfBounds(new Point(point.X + dx, point.Y + 1 + dy))) return false;
            return true;
        }

        public void move()
        {
            if (canMove(0, 1, getPositionArray(pieceMatrix,0,0)))
            {
                this.position.Y += 1;
            }
            else
            {
                isLocked = true;
                pieceLock();
            }
        }

        public void pieceLock()
        {
            foreach (TetrisBlock block in getBlocksForPiece())
            {
                Engine.addBlock(block);
            }
            Engine.newPiece();
        }
    }
}