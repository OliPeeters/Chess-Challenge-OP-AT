using ChessChallenge.API;
using System;

//no minmax
//both colours
//checkmates
//checks
//little endgame
//promotions

//IMPROVEMENTS
//both colours
public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] legalMoves = board.GetLegalMoves();

        Move bestMove = legalMoves[0];

        bool colour = board.IsWhiteToMove;
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
            if(board.IsInCheckmate()) return currentMove;

            //Adds number of our pieces which are in the center of the board
            //cols b-g
            //rows 2-5
            currentWeight += BitboardHelper.GetNumberOfSetBits(allyBoard & 0x7e7e7e7e0000);

            //Increases weight if pawn is further up the board in the endgame
            int pieceDepth = 7 - currentMove.TargetSquare.Rank;
            if(colour) pieceDepth = currentMove.TargetSquare.Rank;
            if(BitboardHelper.GetNumberOfSetBits(enemyBoard) > 7) if((int) currentMove.MovePieceType == 1) currentWeight += 50 * pieceDepth;

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
    public int EvaluateMove(Move currentMove)
    {
        int moveWeight = 0;

        //Increase weight if a piece is captured
        if(currentMove.IsCapture) moveWeight += ((int) (currentMove.CapturePieceType - currentMove.MovePieceType) + 5) * 75;

    }
    public int EvaluateBoard(Board board)
    {

        int boardWeight = 0;

        int[] pieceValue = {1,3,3,5,9,0};
        int counter = 0;
        foreach (PieceList pieceList in board.GetAllPieceLists())
        {
            boardWeight += pieceList.Count * pieceValue[counter] * (pieceList.IsWhitePieceList == board.IsWhiteToMove ? 1 : -1);
            counter++;
            counter %= 6;
        }

        //ulong flip = 0x1111111111111111;
        //if(colour) flip = 0x0000000000000000;
        //ulong allyBoard = board.WhitePiecesBitboard ^ flip;
        //ulong enemyBoard = board.BlackPiecesBitboard ^ flip;

        //Reduce weight if piece can get taken
        Move[] enemyLegalMoves = board.GetLegalMoves(true);
        foreach (Move enemyCapture in enemyLegalMoves)
        {
            if((int)enemyCapture.CapturePieceType > 1) boardWeight -=  50 * (int)enemyCapture.CapturePieceType;
        }

        //Weight moves which put the opponent in check
        if(board.IsInCheck()) boardWeight += 100;

        //Always make move if it is a checkmate
        if(board.IsInCheckmate()) return 0x11111111;

        //Adds number of our pieces which are in the center of the board
        //cols b-g
        //rows 2-5
        currentWeight += BitboardHelper.GetNumberOfSetBits(allyBoard & 0x7e7e7e7e0000);

        //Increases weight if pawn is further up the board in the endgame
        int pieceDepth = 7 - currentMove.TargetSquare.Rank;
        if(colour) pieceDepth = currentMove.TargetSquare.Rank;
        if(BitboardHelper.GetNumberOfSetBits(enemyBoard) > 7) if((int) currentMove.MovePieceType == 1) currentWeight += 50 * pieceDepth;

        //Increases weight if a pawn can be promoted to a queen
        if((int)currentMove.PromotionPieceType == 5) currentWeight += 500;

        return currentWeight;
    }
}