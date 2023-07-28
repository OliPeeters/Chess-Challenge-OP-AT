using ChessChallenge.API;
using System;

//no minmax
//only white
//checkmates
//checks
//little endgame
//promotions

public class version1 : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] legalMoves = board.GetLegalMoves();
        Move bestMove = legalMoves[0];

        ulong oldBoard = board.AllPiecesBitboard;
        ulong allyBoard = board.WhitePiecesBitboard;
        ulong enemyBoard = board.BlackPiecesBitboard;
        // ATTEMPTED FLIP
        //if (!board.IsWhiteToMove)
        //{
            // Flips board so that bot is always playing from bottom
            //oldBoard = 0x1111111111111111 ^ oldBoard;
            //allyBoard = 0x1111111111111111 ^ allyBoard;
            //enemyBoard = 0x1111111111111111 ^ enemyBoard;
        //}
        
        int bestWeight = 0;
        foreach (Move currentMove in legalMoves)
        {
            board.MakeMove(currentMove);

            ulong tempBoard = allyBoard;
            BitboardHelper.ToggleSquare(ref tempBoard, currentMove.StartSquare);
            BitboardHelper.ToggleSquare(ref tempBoard, currentMove.TargetSquare);

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
            if(board.IsInCheckmate()) return currentMove;

            //Adds number of our pieces which are in the center of the board
            //cols b-g
            //rows 2-5
            currentWeight += BitboardHelper.GetNumberOfSetBits(tempBoard & 0x7e7e7e0000);

            //Increases weight if pawn is further up the board in the endgame
            if(BitboardHelper.GetNumberOfSetBits(enemyBoard) > 7)
            {
                if((int) currentMove.MovePieceType == 1) currentWeight += 50 * currentMove.TargetSquare.Rank;
            }

            //Increases weight if a pawn can be promoted to a queen
            if((int)currentMove.PromotionPieceType == 5) currentWeight += 500;

            //Check if current move is better than best move
            if (currentWeight > bestWeight)
            {
                bestWeight = currentWeight;
                bestMove = currentMove;
            }
            board.UndoMove(currentMove);
        }
        //=====================================
        //MINMAX
        //=====================================
        

        //=====================================
        //ENDGAME
        //=====================================

        return bestMove;
    }
}