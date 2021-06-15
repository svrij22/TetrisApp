using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;
using static TetrisApp.TetrisPlayer;
using Color = System.Drawing.Color;

namespace TetrisApp
{
    public class Tetromino
    {

        public int[,] pieceMatrix;
        public Point position;
        public Color color;
        public TetrisPlayer Player;
        public Boolean isLocked;

        public Tetromino(TetrisPlayer Player)
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
            } while (!canMove(0, 0));
            
            //Rotate randomly
            for (var i = 0; i < Player.rnd.Next(3); i++)
            {
                rotateMatrix(pieceMatrix, 3, true);
            }
        }

        public Tetromino(TetrisPlayer player, int[,] matrix, Point point, int i)
        {
            //Set default values
            this.Player = player;
            pieceMatrix = matrix;
            color = Color.FromArgb(i);
            position = point;
        }

        //Move step event
        public bool move()
        {
            if (!canMove(0, 1)) return false;
            position.Y += 1;
            return true;
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
                if (canMove(-1 ,0))
                {
                    position.X -= 1;
                }
            }
            if (e.KeyChar.Equals('d'))
            {
                if (canMove(1, 0))
                {
                    position.X += 1;
                }
            }
            if (e.KeyChar.Equals('e'))
            {
                int moveDown = getSpaceBelow();
                position.Y += moveDown;
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

        /*
         * Drawing
         */

        //Sets a random color
        public void setRandColor()
        {
            Color randomColor = Color.FromArgb(Player.rnd.Next(128) + 128, Player.rnd.Next(128) + 128, Player.rnd.Next(128) + 128);
            color = randomColor;
        }
        
        //Draws blocks
        public void draw(Graphics g)
        {
            foreach (TetrisBlock block in getBlocksForPiece())
            {
                block.draw(g, false);
            }
        }
        
        //Draws ghost
        public void drawGhost(Graphics g)
        {
            foreach (TetrisBlock block in getBlocksForPiece())
            {
                block.position.Y += getSpaceBelow();
                block.draw(g, true);
            }
        }

        /*
         * Max 50 blocks, checks space under block to draw ghost
         */
        public int getSpaceBelow()
        {
            var maxDown = 0;
            for(var i = 1; i < 50; i++)
            {
                if (canMove(0, i))
                {
                    maxDown = i;
                }
            }
            return maxDown;
        }

        /*
         * Matrix convertion
         */
        public List<TetrisBlock> getBlocksForPiece()
        {
            return getPointArray(pieceMatrix, 0, 0).Select(p => new TetrisBlock(color, p)).ToList();
        }

        public List<TetrisBlock> getPreviewBlocks()
        {
            return getPointArray(pieceMatrix, 1, 1).Select(p =>
            {
                p.Offset(-position.X, -position.Y);
                return new TetrisBlock(color, p);
            }).ToList();
        }
        
        public List<Point> getPointArray(int[,] pieceArray, int xOffset, int yOffset)
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
         * Position and Collision Checking
         */
        
        //Checks if matrix is able to move dx/dy positions
        public bool canMove(int dx, int dy)
        {
            return canMove(dx, dy, getPointArray(pieceMatrix, 0, 0));
        }
        public Boolean canMove(int dx, int dy, List<Point> positionArray)
        {
            foreach (Point point in positionArray)
                if (positionIsNotFree(new Point(point.X + dx, point.Y + dy))) return false;
            foreach (Point point in positionArray)
                if (positionOutOfBounds(new Point(point.X + dx, point.Y + 1 + dy))) return false;
            return true;
        }
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

        //Check if rotation is possible
        public bool canRotate(bool clockwise, int dx, int dy)
        {
            //When rotated, only get possible new positions
            int[,] rotatedMatrix = rotateMatrix(pieceMatrix, 3, clockwise);
            List<Point> rotatedPositions = getPointArray(rotatedMatrix,dx,dy);
            return canMove(dx, dy, rotatedPositions);
        }
        
        /*Matrix mutation*/
        /*https://stackoverflow.com/questions/42519/how-do-you-rotate-a-two-dimensional-array*/
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

    }
}