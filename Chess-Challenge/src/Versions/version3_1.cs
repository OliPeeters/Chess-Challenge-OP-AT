using ChessChallenge.API;
using System;

//TO-ADD



public class version3_1 : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        // ATTEMPTED FLIP
        //if (!board.IsWhiteToMove)
        //{
            // Flips board so that bot is always playing from bottom
            //oldBoard = 0x1111111111111111 ^ oldBoard;
            //allyBoard = 0x1111111111111111 ^ allyBoard;
            //enemyBoard = 0x1111111111111111 ^ enemyBoard;
        //}
        
        //=====================================
        //MINMAX
        //=====================================
        Move bestMove = Move.NullMove;
        int bestScore = -10000;
        foreach (Move currentMove in board.GetLegalMoves())
        {
            board.MakeMove(currentMove);
            //Second argument is maxDepth
            int localMax = minimax(board, 2, currentMove);
            board.UndoMove(currentMove);

            if(localMax > bestScore)
            {
                bestMove = currentMove;
                bestScore = localMax;
            }
        }

        //=====================================
        //ENDGAME
        //=====================================

        return bestMove;
    }

    //Depth cannot be <= 0
    private int minimax(Board board, int depth, Move move)
    {
        if(depth == 0) return Scorer(board, move);
        int bestScore = -10000;
        foreach (Move currentMove in board.GetLegalMoves())
        {
            board.MakeMove(currentMove);
            int localMax = minimax(board,depth-1, currentMove);
            board.UndoMove(currentMove);

            if(localMax > bestScore)
            {
                Move bestMove = currentMove;
                bestScore = localMax;
            }
        }
        return bestScore;
    }

    int Scorer(Board board, Move currentMove)
    {

        bool colour = board.IsWhiteToMove;

        ulong flip = 0x1111111111111111;
        if(colour) flip = 0x0000000000000000;
        ulong allyBoard = board.WhitePiecesBitboard ^ flip;
        ulong enemyBoard = board.BlackPiecesBitboard ^ flip;

        int currentWeight = 0;

        //Increase weight if a piece is captured
        if(currentMove.IsCapture) currentWeight += ((int) (currentMove.CapturePieceType - currentMove.MovePieceType) + 5) * 75;

        //Reduce weight if piece can get taken
        Move[] enemyLegalMoves = board.GetLegalMoves(true);
        foreach (Move enemyCapture in enemyLegalMoves)
        {
            if((int)enemyCapture.CapturePieceType > 1) currentWeight -=  50 * (int)enemyCapture.CapturePieceType;
        }

        //Weight moves which put the opponent in check
        if(board.IsInCheck()) currentWeight += 100;

        //Always make move if it is a checkmate
        if(board.IsInCheckmate()) return 1000000;

        //OPTIONAL
        //Adds number of our pieces which are in the center of the board
        //cols b-g
        //rows 2-5
        //currentWeight += BitboardHelper.GetNumberOfSetBits(allyBoard & 0x7e7e7e7e0000);

        //Increases weight if pawn is further up the board in the endgame
        int pieceDepth = 7 - currentMove.TargetSquare.Rank;
        if(colour) pieceDepth = currentMove.TargetSquare.Rank;
        if(BitboardHelper.GetNumberOfSetBits(enemyBoard) > 7) if((int) currentMove.MovePieceType == 1) currentWeight += 50 * pieceDepth;

        //Increases weight if a pawn can be promoted to a queen
        if((int)currentMove.PromotionPieceType == 5) currentWeight += 500;

        return currentWeight;
    }
}