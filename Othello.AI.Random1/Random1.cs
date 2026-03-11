using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Othello.Contract;

namespace Othello.AI.Random;

public class RandomAI1 : IOthelloAI
{
    private const int SearchDepth = 4;

    public string Name => "NegaMax AI";

    public async Task<Move?> GetMoveAsync(BoardState board, DiscColor yourColor, CancellationToken ct)
    {
        await Task.Delay(150, ct);

        List<Move> validMoves = GetValidMoves(board, yourColor);
        if (validMoves.Count == 0)
        {
            return null;
        }

        Move? bestMove = null;
        int bestScore = int.MinValue;
        int alpha = int.MinValue + 1;
        int beta = int.MaxValue;

        foreach (Move move in validMoves)
        {
            var nextBoard = ApplyMove(board, move, yourColor);
            var score = -NegaMax(nextBoard, GetOpponent(yourColor), SearchDepth - 1, -beta, -alpha, yourColor, ct);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }

            if (bestScore > alpha)
            {
                alpha = bestScore;
            }
        }

        return bestMove;
    }

    private int NegaMax(
        BoardState board,
        DiscColor currentColor,
        int depth,
        int alpha,
        int beta,
        DiscColor maximizingColor,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var validMoves = GetValidMoves(board, currentColor);
        var opponent = GetOpponent(currentColor);
        var opponentMoves = GetValidMoves(board, opponent);

        if (depth == 0 || (validMoves.Count == 0 && opponentMoves.Count == 0))
        {
            return EvaluateBoard(board, maximizingColor);
        }

        if (validMoves.Count == 0)
        {
            return -NegaMax(board, opponent, depth - 1, -beta, -alpha, maximizingColor, ct);
        }

        var bestScore = int.MinValue;

        foreach (var move in validMoves)
        {
            var nextBoard = ApplyMove(board, move, currentColor);
            var score = -NegaMax(nextBoard, opponent, depth - 1, -beta, -alpha, maximizingColor, ct);

            if (score > bestScore)
            {
                bestScore = score;
            }

            if (bestScore > alpha)
            {
                alpha = bestScore;
            }

            if (alpha >= beta)
            {
                break;
            }
        }

        return bestScore;
    }

    private int EvaluateBoard(BoardState board, DiscColor color)
    {
        var score = 0;
        var opponent = GetOpponent(color);

        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                if (board.Grid[r, c] == color)
                {
                    score++;
                }
                else if (board.Grid[r, c] == opponent)
                {
                    score--;
                }
            }
        }

        return score;
    }

    private List<Move> GetValidMoves(BoardState board, DiscColor color)
    {
        var moves = new List<Move>();

        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                var move = new Move(r, c);
                if (IsValidMove(board, move, color))
                {
                    moves.Add(move);
                }
            }
        }

        return moves;
    }

    private bool IsValidMove(BoardState board, Move move, DiscColor color)
    {
        if (board.Grid[move.Row, move.Column] != DiscColor.None)
        {
            return false;
        }

        int[] dr = { -1, -1, -1, 0, 0, 1, 1, 1 };
        int[] dc = { -1, 0, 1, -1, 1, -1, 0, 1 };
        var opponent = GetOpponent(color);

        for (int i = 0; i < 8; i++)
        {
            int r = move.Row + dr[i];
            int c = move.Column + dc[i];
            var foundOpponent = false;

            while (IsInside(r, c) && board.Grid[r, c] == opponent)
            {
                r += dr[i];
                c += dc[i];
                foundOpponent = true;
            }

            if (foundOpponent && IsInside(r, c) && board.Grid[r, c] == color)
            {
                return true;
            }
        }

        return false;
    }

    private BoardState ApplyMove(BoardState board, Move move, DiscColor color)
    {
        var nextBoard = board.Clone();
        nextBoard.Grid[move.Row, move.Column] = color;

        int[] dr = { -1, -1, -1, 0, 0, 1, 1, 1 };
        int[] dc = { -1, 0, 1, -1, 1, -1, 0, 1 };
        var opponent = GetOpponent(color);

        for (int i = 0; i < 8; i++)
        {
            var discsToFlip = new List<(int Row, int Column)>();
            int r = move.Row + dr[i];
            int c = move.Column + dc[i];

            while (IsInside(r, c) && nextBoard.Grid[r, c] == opponent)
            {
                discsToFlip.Add((r, c));
                r += dr[i];
                c += dc[i];
            }

            if (IsInside(r, c) && nextBoard.Grid[r, c] == color)
            {
                foreach (var disc in discsToFlip)
                {
                    nextBoard.Grid[disc.Row, disc.Column] = color;
                }
            }
        }

        return nextBoard;
    }

    private static DiscColor GetOpponent(DiscColor color)
    {
        return color == DiscColor.Black ? DiscColor.White : DiscColor.Black;
    }

    private static bool IsInside(int row, int column)
    {
        return row >= 0 && row < 8 && column >= 0 && column < 8;
    }
}
