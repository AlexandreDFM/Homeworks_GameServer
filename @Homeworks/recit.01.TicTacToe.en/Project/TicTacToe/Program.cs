/* 
 * File Name: tictactoe.py
 * Author : Alexandre Kévin DE FREITAS MARTINS
 * Creation Date: 05/09/2024
 * Description: This is the tictactoe.py
 * Copyright (c) 2024 Alexandre Kévin DE FREITAS MARTINS
 * Version: 1.0.0
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the 'Software'), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
*/

using System;

class TicTacToe {
    static void DrawBoard(char[] board) {
        Console.WriteLine("------------");
        Console.WriteLine(" {0} | {1} | {2} ", board[0], board[1], board[2]);
        Console.WriteLine("------------");
        Console.WriteLine(" {0} | {1} | {2} ", board[3], board[4], board[5]);
        Console.WriteLine("------------");
        Console.WriteLine(" {0} | {1} | {2} ", board[6], board[7], board[8]);
        Console.WriteLine("------------");
    }

    static bool CheckForWin(char[] board, char player) {
        return (board[0] == player && board[1] == player && board[2] == player) ||
               (board[3] == player && board[4] == player && board[5] == player) ||
               (board[6] == player && board[7] == player && board[8] == player) ||
               (board[0] == player && board[3] == player && board[6] == player) ||
               (board[1] == player && board[4] == player && board[7] == player) ||
               (board[2] == player && board[5] == player && board[8] == player) ||
               (board[2] == player && board[4] == player && board[6] == player) ||
               (board[0] == player && board[4] == player && board[8] == player);
    }

    static bool IsFree(char[] board, int loc) {
        return board[loc] == ' ';
    }

    static bool CheckForTie(char[] board) {
        for (int i = 0; i < 9; i++) {
            if (IsFree(board, i)) return false;
        }
        return true;
    }

    static int GetUserMove(char[] board) {
        int loc;
        Console.Write("Enter a number between 1 and 9: ");
        for (; !int.TryParse(Console.ReadLine(), out loc) || loc < 1 || loc > 9 || IsFree(board, loc);) {
            Console.Write("Invalid move. Enter a number between 1 and 9:");
        }
        return loc;
    }

    static void Main()
    {
        Console.WriteLine("Welcome to Tic Tac Toe!");
        char[] board = { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' };
        bool playing = true;
        char turn = 'X';

        DrawBoard(board);
        while (playing) {
            int loc;
            if (turn == 'X') {
                loc = GetUserMove(board);
                board[loc - 1] = 'X';
            } else {
                loc = GetUserMove(board);
                board[loc - 1] = 'O';
            }
            DrawBoard(board);
            if (CheckForWin(board, turn)) {
                Console.WriteLine("{0} wins!", turn);
                playing = false;
            } else if (CheckForTie(board)) {
                Console.WriteLine("It's a tie!");
                playing = false;
            } else {
                turn = (turn == 'O') ? 'X' : 'O';
            }
        }
        Console.Write("Thank you for playing !");
        _ = Console.ReadLine();
        return;
    }
}
