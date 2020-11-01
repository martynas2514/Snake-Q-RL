using System;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Console;

namespace Snake
{
    class Program
    {
        static void Main()
        {
            Snake_Q_RL.QRLModel Model = new Snake_Q_RL.QRLModel(0.8, 0.5, 0.8, 4, 4, 4, 4, 4, 4);
            int i = 0;
            while ( i < 5000)
            {
             WriteLine(i);
             Game(Model, false);
                i++;
            }

            while (true)
            {
                Game(Model, true);
            }
        }


        static void DrawPixel(Pixel pixel)
        {
            SetCursorPosition(pixel.XPos, pixel.YPos);
            ForegroundColor = pixel.ScreenColor;
            Write("■");
            SetCursorPosition(0, 0);
        }

        static void DrawBorder()
        {
            for (int i = 0; i < WindowWidth; i++)
            {
                SetCursorPosition(i, 0);
                Write("■");

                SetCursorPosition(i, WindowHeight - 1);
                Write("■");
            }

            for (int i = 0; i < WindowHeight; i++)
            {
                SetCursorPosition(0, i);
                Write("■");

                SetCursorPosition(WindowWidth - 1, i);
                Write("■");
            }
        }

        struct Pixel
        {
            public Pixel(int xPos, int yPos, ConsoleColor color)
            {
                XPos = xPos;
                YPos = yPos;
                ScreenColor = color;
            }
            public int XPos { get; set; }
            public int YPos { get; set; }
            public ConsoleColor ScreenColor { get; set; }
        }

        enum Direction
        {
            Up,
            Down,
            Right,
            Left
        }


        static void Game(Snake_Q_RL.QRLModel qRL, bool draw) {
            WindowHeight = 16;
            WindowWidth = 32;

            var rand = new Random();

            var score = 0;

            var head = new Pixel(WindowWidth / 2, WindowHeight / 2, ConsoleColor.Red);
            var berry = new Pixel(rand.Next(1, WindowWidth - 2), rand.Next(1, WindowHeight - 2), ConsoleColor.Cyan);

            var body = new List<Pixel>();

            var currentMovement = Direction.Right;

            var gameover = false;

            int up;
            int down;
            int left;
            int right;
            int dir;
            int rdir;
            
            while (true)
            {

                up = Scale(head.YPos);
                down = Scale(WindowHeight - head.YPos);
                left = Scale(head.XPos);
                right = Scale(WindowWidth - head.XPos);
                dir = DirToBerry(head.XPos, head.YPos, berry.XPos, berry.YPos);
                rdir = DirToNum(currentMovement);
                int reward = -1; 

                int chosenDir = qRL.GetBestaction(up, down, left, right, dir, rdir);
                currentMovement = ReadMovement(currentMovement, chosenDir);

                body.Add(new Pixel(head.XPos, head.YPos, ConsoleColor.Green));

                switch (currentMovement)
                {
                    case Direction.Up:
                        head.YPos--;
                        break;
                    case Direction.Down:
                        head.YPos++;
                        break;
                    case Direction.Left:
                        head.XPos--;
                        break;
                    case Direction.Right:
                        head.XPos++;
                        break;
                }

                

                gameover |= (head.XPos == WindowWidth - 1 || head.XPos == 0 || head.YPos == WindowHeight - 1 || head.YPos == 0);

                //DrawBorder();

                if (berry.XPos == head.XPos && berry.YPos == head.YPos)
                {
                    //score++;
                    reward = 50;
                    berry = new Pixel(rand.Next(1, WindowWidth - 2), rand.Next(1, WindowHeight - 2), ConsoleColor.Cyan);
                }

                if (draw)
                {
                    Clear();
                }
                for (int i = 0; i < body.Count; i++)
                {
                    if (draw)
                    {
                        DrawPixel(body[i]);
                    }
                    
                    gameover |= (body[i].XPos == head.XPos && body[i].YPos == head.YPos);
                }

                int nup = Scale(head.YPos);
                int ndown = Scale(WindowHeight - head.YPos);
                int nleft = Scale(head.XPos);
                int nright = Scale(WindowWidth - head.XPos);
                int ndir = DirToBerry(head.XPos, head.YPos, berry.XPos, berry.YPos);
                int nrdir = DirToNum(currentMovement);
                if (gameover)
                {
                    reward = -50;
                    qRL.UpdateQTable(up, down, left, right, dir,rdir, chosenDir, nup, ndown, nleft, nright, ndir, nrdir, reward);
                    break;
                }
                qRL.UpdateQTable(up, down, left, right, dir, rdir, chosenDir, nup, ndown, nleft, nright, ndir, nrdir, reward);

                if (draw)
                {
                    DrawPixel(head);
                    DrawPixel(berry);
                }
                

                if (body.Count > score)
                {
                    body.RemoveAt(0);
                }

                if (draw)
                {
                    System.Threading.Thread.Sleep(10);
                }
                
            }

            if (draw)
            {
                SetCursorPosition(WindowWidth / 5, WindowHeight / 2);
                WriteLine($"Game over, Score: {score}");
                SetCursorPosition(WindowWidth / 5, WindowHeight / 2 + 1);
            }

        }

        static int Scale(int x)
        {
            int val;
            if (x >= 3)
            {
                val = 3;
            }
            else if(x< 0)
            {
                val = 0;
            }
            else
            {
                val = x;
            }
            return val;
        }

        static int DirToNum( Direction x) {
            int value = 0;

            if (x == Direction.Down)
            {
                value = 1;
            }
            else if (x == Direction.Up)
            {
                value = 0;
            }
            else if (x == Direction.Right)
            {
                value = 3;
            }
            else if (x == Direction.Left)
            {
                value = 2;
            }

            return value;
        }

        static Direction ReadMovement(Direction movement, int key)
        {

            

            if (key == 0 && movement != Direction.Down)
            {
                movement = Direction.Up;
            }
            else if (key == 1 && movement != Direction.Up)
            {
                movement = Direction.Down;
            }
            else if (key == 2 && movement != Direction.Right)
            {
                movement = Direction.Left;
            }
            else if (key == 3 && movement != Direction.Left)
            {
                movement = Direction.Right;
            }


            return movement;
        }

        static int DirToBerry(int x, int y, int bx, int by) {
            // 0 up, 1 down, 2 right, 3 left
            int value = 0;
            int dx = x - bx;
            int dy = y - by;
            if (Math.Abs(dx) > Math.Abs(dy))
            {
                if (dx > 0)
                {
                    value = 0;
                }
                else
                {
                    value = 1;
                }
            }
            else
            {
                if (dy > 0)
                {
                    value = 2;
                }
                else
                {
                    value = 3;
                }
            }
            return value;



        }
    }
}
