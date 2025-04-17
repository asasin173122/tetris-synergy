using System;
using System.Collections.Generic;
using System.Threading;

namespace Synergy_tetris_asanali
{
    internal class Program
    {
        private static string[] tetrominos = new string[7];
        private static int fieldWidth = 12;
        private static int fieldHeight = 18;
        private static int[] playingField;
        static int screenWidth = 80;
        static int screenHeight = 30;
        private static char[] screen = new char[screenWidth * screenHeight];

        static int currentPiece = 0;
        static int currentRotation = 0;
        static int currentX = (fieldWidth - 3) / 2;
        static int currentY = 0;

        static bool rotateHold = false;

        static int speed = 20;
        static int speedCounter = 0;
        static bool forceDown = false;

        static List<int> lines = new List<int>();

        static int pieceCount = 0;
        private static int score = 0;

        static void Main(string[] args)
        {
            Console.CursorVisible = false;

            InitializeTetrominos();
            InitializePlayingField();
            GenerateNewTetromino();
            bool isGameOver = false;

            while (!isGameOver)
            {
                speedCounter++;
                forceDown = (speedCounter == speed);

                DrawField();
                DrawCurrentPiece();
                UserInput();

                if (forceDown)
                {
                    if (DoesPieceFit(currentPiece, currentRotation, currentX, currentY + 1))
                    {
                        currentY++;
                    }
                    else
                    {
                        for (int px = 0; px < 4; px++)
                        {
                            for (int py = 0; py < 4; py++)
                            {
                                if (tetrominos[currentPiece][Rotate(px, py, currentRotation)] == 'X')
                                {
                                    playingField[(currentY + py) * fieldWidth + (currentX + px)] = currentPiece + 1;
                                }
                            }
                        }

                        pieceCount++;
                        if (pieceCount % 10 == 0 && speed >= 10) speed--;

                        for (int py = 0; py < 4; py++)
                        {
                            if (currentY + py < fieldHeight - 1)
                            {
                                bool line = true;
                                for (int px = 1; px < fieldWidth - 1; px++)
                                    line &= playingField[(currentY + py) * fieldWidth + px] != 0;

                                if (line)
                                {
                                    for (int px = 1; px < fieldWidth - 1; px++)
                                        playingField[(currentY + py) * fieldWidth + px] = 8;

                                    lines.Add(currentY + py);
                                }
                            }
                        }

                        score += 25;
                        if (lines.Count > 0) score += (1 << lines.Count) * 100;

                        if (lines.Count > 0)
                        {
                            DrawField();
                            DrawScreen();
                            Thread.Sleep(500);

                            foreach (var element in lines)
                            {
                                for (int px = 1; px < fieldWidth - 1; px++)
                                {
                                    for (int py = element; py > 0; py--)
                                        playingField[py * fieldWidth + px] = playingField[(py - 1) * fieldWidth + px];

                                    playingField[px] = 0;
                                }
                            }

                            lines.Clear();
                        }

                        GenerateNewTetromino();
                        isGameOver = !DoesPieceFit(currentPiece, currentRotation, currentX, currentY);
                    }
                    speedCounter = 0;
                }

                WriteScoreboard();
                DrawScreen();
            }
        }

        private static void InitializeTetrominos()
        {
            tetrominos[0] = "..X...X...X...X."; // I
            tetrominos[1] = "..X..XX...X....."; // T
            tetrominos[2] = ".....XX..XX....."; // O
            tetrominos[3] = "..X..XX..X......"; // S
            tetrominos[4] = ".X...XX...X....."; // Z
            tetrominos[5] = ".X...X...XX....."; // J
            tetrominos[6] = "..X...X..XX....."; // L
        }

        private static void InitializePlayingField()
        {
            playingField = new int[fieldWidth * fieldHeight];
            for (int x = 0; x < fieldWidth; x++)
                for (int y = 0; y < fieldHeight; y++)
                    playingField[y * fieldWidth + x] = (x == 0 || x == fieldWidth - 1 || y == fieldHeight - 1) ? 9 : 0;
        }

        private static void GenerateNewTetromino()
        {
            currentX = (fieldWidth - 3) / 2;
            currentY = 0;
            currentRotation = 0;
            currentPiece = new Random().Next(0, 7);
        }

        private static void DrawCurrentPiece()
        {
            for (int px = 0; px < 4; px++)
                for (int py = 0; py < 4; py++)
                    if (tetrominos[currentPiece][Rotate(px, py, currentRotation)] == 'X')
                        screen[(currentY + py + 2) * screenWidth + (currentX + px + 2)] = (char)(currentPiece + 65);
        }

        private static void DrawField()
        {
            for (int x = 0; x < fieldWidth; x++)
                for (int y = 0; y < fieldHeight; y++)
                    screen[(y + 2) * screenWidth + (x + 2)] = " ABCDEFG=#"[playingField[y * fieldWidth + x]];
        }

        private static void DrawScreen()
        {
            Console.SetCursorPosition(0, 0);
            for (int y = 0; y < screenHeight; y++)
            {
                for (int x = 0; x < screenWidth; x++)
                    Console.Write(screen[y * screenWidth + x]);
                Console.WriteLine();
            }
        }

        private static void WriteScoreboard()
        {
            string scoreboard = $"Score: {score}";
            for (int i = 0; i < scoreboard.Length; i++)
                screen[screenWidth * 1 + i] = scoreboard[i];
        }

        private static void UserInput()
        {
            if (Console.KeyAvailable)
            {
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.A:
                    case ConsoleKey.LeftArrow:
                        if (DoesPieceFit(currentPiece, currentRotation, currentX - 1, currentY)) currentX--;
                        break;
                    case ConsoleKey.D:
                    case ConsoleKey.RightArrow:
                        if (DoesPieceFit(currentPiece, currentRotation, currentX + 1, currentY)) currentX++;
                        break;
                    case ConsoleKey.S:
                    case ConsoleKey.DownArrow:
                        if (DoesPieceFit(currentPiece, currentRotation, currentX, currentY + 1)) currentY++;
                        break;
                    case ConsoleKey.W:
                    case ConsoleKey.UpArrow:
                        if (!rotateHold && DoesPieceFit(currentPiece, (currentRotation + 1) % 4, currentX, currentY))
                        {
                            currentRotation = (currentRotation + 1) % 4;
                            rotateHold = true;
                        }
                        break;
                }
            }
            else rotateHold = false;
        }

        private static int Rotate(int px, int py, int r)
        {
            switch (r % 4)
            {
                case 0: return py * 4 + px;
                case 1: return 12 + py - (px * 4);
                case 2: return 15 - (py * 4) - px;
                case 3: return 3 - py + (px * 4);
            }
            return 0;
        }

        private static bool DoesPieceFit(int tetromino, int rotation, int posX, int posY)
        {
            for (int px = 0; px < 4; px++)
            {
                for (int py = 0; py < 4; py++)
                {
                    int pi = Rotate(px, py, rotation);
                    int fi = (posY + py) * fieldWidth + (posX + px);

                    if (posX + px >= 0 && posX + px < fieldWidth && posY + py >= 0 && posY + py < fieldHeight)
                    {
                        if (tetrominos[tetromino][pi] == 'X' && playingField[fi] != 0)
                            return false;
                    }
                    else
                    {
                        if (tetrominos[tetromino][pi] == 'X')
                            return false;
                    }
                }
            }
            return true;
        }
    }
}
