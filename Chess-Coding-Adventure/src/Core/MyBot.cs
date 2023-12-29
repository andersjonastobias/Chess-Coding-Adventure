using Chess.Core;

//using ChessChallenge.Chess;
//using ChessChallenge.Chess;
using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks.Sources;
using Chess.Core;
//using static MyBot;

namespace Chess.Core
{
    public class MyBot// : IChessBot
    {

        double[] centralvalue = (from i in Enumerable.Range(0, 8) from j in Enumerable.Range(0, 8) select 7 - Math.Abs(i - 3.5) - Math.Abs(j - 3.5)).ToArray();
        int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

        int depth = 1;
        int minDepth = 1;
        int maxDepth = 2;
        int minValue = int.MinValue + 1;
        int maxValue = int.MaxValue - 1;
        double weighNumberOfMoves;
        MoveGenerator moveGenerator;

        public MyBot(int depth, double weighNumberOfMoves = 0.3)
        {
            this.weighNumberOfMoves = weighNumberOfMoves;
            this.depth = depth;
            Console.WriteLine(this.depth);
            this.moveGenerator = new MoveGenerator();
        }

        public MyBot(ref int i)
        {
            this.weighNumberOfMoves = 0.7;

            i += 1;
            Console.WriteLine(this.depth);
        }

        public string info()
        {
            return $"{this.weighNumberOfMoves}    {this.GetHashCode()}";
        }

        public record ScoredMove(Move? move, double score);

        private ScoredMove AnalyzeMoves(Board board,  double alpha, double beta, int maxDepthForMove, int currentDepth, double currentScore, ScoredMove initialMove, Func<double, double, bool> comparer)
        {
            Move[] allMoves = this.moveGenerator.GenerateMoves(board).ToArray();// board.GetLegalMoves();

            ScoredMove bestMove = initialMove;
            foreach (var move in allMoves)
            {
                if (MoveIsCheckmate(board, move))
                {
                    return new(move, -initialMove.score);
                }
                //if (move.IsCapture && maxDepthForMove < this.maxDepth)
                //    maxDepth += 1;
                ScoredMove newMove;
                currentScore += ScoreMove(board, move) * Math.Pow(-1, currentDepth);//Ugly to use depth to determine whihc player is playing
                board.MakeMove(move);
                if (currentDepth == maxDepthForMove)
                {
                    //currentScore = Score(board);
                    currentScore = currentScore + this.weighNumberOfMoves * moveGenerator.GenerateMoves(board).Length;
                    newMove = new(move, currentScore);
                }
                else
                {
                    newMove = Analyze(board, alpha, beta, currentScore, maxDepthForMove, currentDepth + 1);
                }

                if (currentDepth % 2 == 0)
                {
                    if (newMove.score > bestMove.score)// newscore > scoredMove.score)
                    {
                        bestMove = new(move, newMove.score);

                        if (newMove.score > beta)
                        {
                            board.UnmakeMove(move);
                            currentScore -= ScoreMove(board, move);
                            return bestMove;
                        }

                        alpha = Math.Max(alpha, newMove.score);

                    }
                }
                else
                {
                    if (newMove.score < bestMove.score)// newscore <scoredMove.score)
                    {
                        bestMove = new(move, newMove.score);
                        //scoredMove2 = new(move, newscore);
                        if (newMove.score < alpha)
                        {
                            board.UnmakeMove(move);
                            currentScore -= ScoreMove(board, move);
                            return bestMove;
                        }

                        beta = Math.Min(beta, newMove.score);
                    }

                }
                board.UnmakeMove(move);
                currentScore -= ScoreMove(board, move);
            }
            return bestMove;

        }

        public ScoredMove Analyze(Board board, double alpha, double beta, double currentScore, int maxDepthForMove, int currentDepth)
        {

            if (currentDepth % 2 == 0)
            {//minimizer
                return AnalyzeMoves(board, alpha, beta, maxDepthForMove, currentDepth, currentScore, new(null, minValue), (bestvalue, newvalue) => (bestvalue > newvalue));
            }
            else
            {//maximizer
                return AnalyzeMoves(board, alpha, beta, maxDepthForMove, currentDepth, currentScore, new(null, maxValue), (bestvalue, newvalue) => (bestvalue < newvalue));
            }
            // var val = UpdateScore(move, board);

        }

        private int ScoreMove(Board board, Move move)
        {
            int startSquare = move.StartSquare;
            int targetSquare = move.TargetSquare;
            int moveFlag = move.MoveFlag;
            bool isPromotion = move.IsPromotion;
            bool isEnPassant = moveFlag is Move.EnPassantCaptureFlag;

            int movedPiece = board.Square[startSquare];
            int movedPieceType = Piece.PieceType(movedPiece);
            //int capturedPiece = isEnPassant ? Piece.MakePiece(Piece.Pawn, OpponentColour) : board.Square[targetSquare];
            int capturedPiece = board.Square[targetSquare];
            int capturedPieceType = Piece.PieceType(capturedPiece);

            // Find highest value capture
            //Piece capturedPiece = board.GetPiece(move.TargetSquare);
            int capturedPieceValue = pieceValues[(int)capturedPieceType];

            return capturedPieceValue;

        }

        public Move Think(Board board)
        {
            var currentScore = 0;
            return Analyze(board, minValue, maxValue, currentScore, this.maxDepth, 0).move.Value;

        
        }

        // Test if this move gives checkmate
        bool MoveIsCheckmate(Board board, Move move)
        {
            board.MakeMove(move);
            Move[] moves = moveGenerator.GenerateMoves(board).ToArray();
            bool isMate =   moves.Length==0;
            board.UnmakeMove(move);
            return isMate;
        }
    }
}