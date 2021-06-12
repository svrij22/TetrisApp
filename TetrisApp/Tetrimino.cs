using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static TetrisApp.TetrisPlayer;

namespace TetrisApp
{
    public class Tetrimino
    {

        public int[,] pieceMatrix;
        public Point position;
        public Color color;
        public TetrisPlayer Player;
        public Boolean isLocked;

        public Tetrimino(TetrisPlayer Player)
        {
            //Set game
            this.Player = Player;

            //Random color and size
            pieceMatrix = getRandPiece();
            setRandColor();

            //Set random point thats slightly visible
            do
            {
                position = new Point(Player.rnd.Next(7) + 1, -2);
            } while (!canMove(0, 0, getPositionArray(pieceMatrix, 0, 0)));
            
            //Rotate randomly
            for (var i = 0; i < Player.rnd.Next(3); i++)
            {
                rotateMatrix(pieceMatrix, 3, true);
            }
        }

        public Tetrimino(TetrisPlayer player, int[,] matrix, Point point, int i)
        {
            //Set default values
            this.Player = player;
            pieceMatrix = matrix;
            color = Color.FromArgb(i);
            position = point;
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
        }

        //Attempt rotation for kicks
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
                    pieceMatrix = rotateMatrix(pieceMatrix, 3, clockwise);
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
            int[,] rotatedMatrix = rotateMatrix(pieceMatrix, 3, clockwise);
            List<Point> rotatedPositions = getPositionArray(rotatedMatrix,dx,dy);
            return canMove(dx, dy, rotatedPositions);
        }

        //Get random Tetris Piece
        public int[,] getRandPiece()
        {
            int[,] array2D = { { 0, 1, 0}, { 1, 1, 0}, { 0, 1, 0} };
            switch (Player.rnd.Next(8))
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
                block.draw(g, false);
            }
        }
        
        public void drawGhost(Graphics g, int height)
        {
            var maxDown = 0;
            for(var i = 1; i < height; i++)
            {
                if (canMove(0, i, getPositionArray(pieceMatrix, 0, 0)))
                {
                    maxDown = i;
                }
            }
            
            foreach (TetrisBlock block in getBlocksForPiece())
            {
                var opacity = 0.5;
                block.position.Y += maxDown;
                block.draw(g, true);
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
        

        /*
         * Position Checking
         */
        public bool positionIsNotFree(Point givenPoint)
        {
            foreach (TetrisBlock block in Player.tetrisBlocks)
            {
                if (block.position == givenPoint) return true;
            }
            return false;
        }

        public bool positionOutOfBounds(Point givenPoint)
        {
            if (givenPoint.X < 0) return true;
            if (givenPoint.X > Player.boxSize.Width-1) return true;
            if (givenPoint.Y > Player.boxSize.Height) return true;
            return false;
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

        //Sets a random color
        public void setRandColor()
        {
            Color randomColor = Color.FromArgb(Player.rnd.Next(128) + 128, Player.rnd.Next(128) + 128, Player.rnd.Next(128) + 128);
            color = randomColor;
        }

        
        //rotates the matrix clockwise or counterclockwise
        static int[,] rotateMatrix(int[,] matrix, int n, Boolean clockwise)
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
        
        //Checks if matrix is able to move dx/dy positions
        public Boolean canMove(int dx, int dy, List<Point> positionArray)
        {
            foreach (Point point in positionArray)
                if (positionIsNotFree(new Point(point.X + dx, point.Y + dy))) return false;
            foreach (Point point in positionArray)
                if (positionOutOfBounds(new Point(point.X + dx, point.Y + 1 + dy))) return false;
            return true;
        }

        //Move step event
        public void move()
        {
            if (canMove(0, 1, getPositionArray(pieceMatrix,0,0)))
            {
                position.Y += 1;
            }
            else
            {
                isLocked = true;
                pieceLock();
            }
        }

        //Lock the piece and starts a new one
        public void pieceLock()
        {
            foreach (TetrisBlock block in getBlocksForPiece())
            {
                Player.addBlock(block);
            }
            Player.newPiece();
        }

    }
}