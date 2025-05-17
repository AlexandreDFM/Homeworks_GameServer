// See https://aka.ms/new-console-template for more information

using System;

class TicTacToe
{
    static void DrawBoard(char[] board)
    {
        Console.WriteLine("------------");
        Console.WriteLine(" {0} | {1} | {2} ", board[1], board[2], board[3]);
        Console.WriteLine("------------");
        Console.WriteLine(" {0} | {1} | {2} ", board[4], board[5], board[6]);
        Console.WriteLine("------------");
        Console.WriteLine(" {0} | {1} | {2} ", board[7], board[8], board[9]);
        Console.WriteLine("------------");
    }

    static bool CheckForWin(char[] board, char player)
    {
        return (board[1] == player && board[2] == player && board[3] == player) ||
               (board[4] == player && board[5] == player && board[6] == player) ||
               (board[7] == player && board[8] == player && board[9] == player) ||
               (board[1] == player && board[4] == player && board[7] == player) ||
               (board[2] == player && board[5] == player && board[8] == player) ||
               (board[3] == player && board[6] == player && board[9] == player) ||
               (board[3] == player && board[5] == player && board[7] == player) ||
               (board[1] == player && board[5] == player && board[9] == player);
    }

    static bool IsFree(char[] board, int loc)
    {
        return board[loc] == ' ';
    }

    static bool CheckForTie(char[] board)
    {
        for (int i = 1; i < 10; i++)
        {
            if (IsFree(board, i))
                return false;
        }
        return true;
    }

    static int GetUserMove(char[] board)
    {
       Console.Write("Enter a number between 1 and 9: ");
       return int.Parse(Console.ReadLine());
    }


        static int GetComputerMove(char[] board)
    {
        for (int i = 1; i < 10; i++)
        {
            if (IsFree(board, i))
            {
                board[i] = 'O';
                if (CheckForWin(board, 'O'))
                {
                    board[i] = ' ';
                    return i;
                }
                board[i] = ' ';
            }
        }

        for (int i = 1; i < 10; i++)
        {
            if (IsFree(board, i))
            {
                board[i] = 'X';
                if (CheckForWin(board, 'X'))
                {
                    board[i] = ' ';
                    return i;
                }
                board[i] = ' ';
            }
        }

        for (int i = 1; i < 10; i += 2)
        {
            if (i != 5 && IsFree(board, i))
            {
                return i;
            }
        }

        if (IsFree(board, 5))
            return 5;

        for (int i = 2; i < 10; i += 2)
        {
            if (IsFree(board, i))
            {
                return i;
            }
        }

        return 1; // fallback, should never reach here
    }

    static void Main()
    {
        Console.WriteLine("Welcome to Tic Tac Toe!");
        char[] board = { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' };
        bool playing = true;
        char turn = 'O'; // O: Computer, X: User

        DrawBoard(board);
        while (playing)
        {
            int loc;
            if (turn == 'X') // User's turn
            {
                loc = GetUserMove(board);
                board[loc] = 'X';
            }
            else // Computer's turn
            {
                loc = GetComputerMove(board);
                board[loc] = 'O';
                Console.Write("A.I. thinking");
                for (int i = 0; i < 3; i++)
                {
                    Thread.Sleep(400);
                    Console.Write(". ");
                }
                Console.WriteLine();
            }
            DrawBoard(board);

            if (CheckForWin(board, turn))
            {
                Console.WriteLine("{0} wins!", turn);
                playing = false;
            }
            else if (CheckForTie(board))
            {
                Console.WriteLine("It's a tie!");
                playing = false;
            }
            else
            {
                turn = (turn == 'O') ? 'X' : 'O';
            }
        }
    }
}