using ChessChallenge.API;
using System;

//no minmax
//both colours
//checkmates
//checks
//little endgame
//promotions

//IMPROVEMENTS
//MinMax lol
public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        //Default move to stop CS shitting itself
        Move bestMove = Move.NullMove;

        int bestWeight = int.MinValue;
        //Run Minmax on each possible move to check which is best
        foreach (Move currentMove in board.GetLegalMoves())
        {
            board.MakeMove(currentMove);
            int currentWeight = -MinMax(board, 2);
            board.UndoMove(currentMove);
            if (bestWeight < currentWeight) 
            {
                bestWeight = currentWeight;
                bestMove = currentMove;
            }
        }
        return bestMove;
    }
    public int MinMax(Board board, int depth, int alpha, int beta) 
    {
        if (depth == 0)
        {
            return -EvaluateBoard(board);
        }

        int bestWeight = int.MinValue;

        foreach (Move currentMove in board.GetLegalMoves())
        {
            board.MakeMove(currentMove);
            int currentWeight = -MinMax(board, depth - 1);
            board.UndoMove(currentMove);
            bestWeight = Math.Max(bestWeight, currentWeight);
            //alpha = Math.Max(currentWeight, alpha);
            //if(currentWeight >= beta) return beta;
        }
        return bestWeight;
    }

    //TODO
    //Evaluate move takes a move as input and aims to 
    //Return a value based on how good that move is
    //Higher is better
    public int EvaluateMove(Move currentMove)
    {
        //How good is this move
        int moveWeight = 0;

        //Increase weight if a piece is captured
        if(currentMove.IsCapture) moveWeight += ((int) (currentMove.CapturePieceType - currentMove.MovePieceType) + 5) * 75;

        return moveWeight;
    }
    //Evaluate board takes a board as input and aims to 
    //Return a value based on how good that board is
    //Assumes the ally move has already been taken
    //Higher is better
    public int EvaluateBoard(Board board)
    {
        //How good is this board    
        int boardWeight = 0;

        //
        ulong[] pawnPos = {0x661800, 0xc36600006600, 0x241818000000, 0xff180000000000};
        ulong[] knightPos = {0xc3810000000081c3, 0x3c4281818181423c, 0x244200661800, 0x183c3c180000};

        //Add value to the board based on what pieces are 
        //in play for both sides
        //More ally pieces == Higher value
        //More enemy pieces == Lower value

        //RAMP THIS UP
        //Currently poor trades being made
        int[] pieceValue = {75,225,225,375,675,000};
        int counter = 0;
        foreach (PieceList pieceList in board.GetAllPieceLists())
        {
            //Counts the number of a particular kind of piece
            //Times the value of that piece
            //Makes the value negative if we are talking about an enemy piece
            //If it is white to move then we are black and 
            //So white pieces are negative and black is positive
            boardWeight += pieceList.Count * pieceValue[counter] * (pieceList.IsWhitePieceList == board.IsWhiteToMove ? -1 : 1);
            counter++;
            counter %= 6;
        }

        //TODO
        //ulong flip = 0x1111111111111111;
        //if(colour) flip = 0x0000000000000000;
        //ulong allyBoard = board.WhitePiecesBitboard ^ flip;
        //ulong enemyBoard = board.BlackPiecesBitboard ^ flip;

        //Reduce weight if piece can get taken
        //True flag means only moves involving a capture
        foreach (Move enemyCapture in board.GetLegalMoves(true))
        {
            //if((int)enemyCapture.CapturePieceType > 1) boardWeight -=  50 * (int)enemyCapture.CapturePieceType;
            boardWeight -= pieceValue[(int)enemyCapture.CapturePieceType];
        }

        //Weight moves which put the opponent in check
        if(board.IsInCheck()) boardWeight += 30;

        //Always make move if it is a checkmate
        if(board.IsInCheckmate()) return 0x11111111;

        //TODO
        //Adds number of our pieces which are in the center of the board
        //cols b-g
        //rows 2-5
        //currentWeight += BitboardHelper.GetNumberOfSetBits(allyBoard & 0x7e7e7e7e0000);
        
        //Calculates weights of positioning of pieces on the board
        int bbScore = 0;
        //Pawn
        ulong tempBitboard = board.GetPieceBitboard((PieceType)1, true);
        bbScore += BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0xff180000000000) * 40;
        bbScore += BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0x241818000000 ) * 20;
        bbScore += BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0xc36600006600 ) * 10;
        bbScore += BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0x661800 ) * -10;
        //Knight
        tempBitboard = board.GetPieceBitboard((PieceType)2, true);
        bbScore += BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0xc3810000000081c3 ) * -40;
        bbScore += BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0x3c4281818181423c ) * -20;
        bbScore += BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0x244200661800 ) * 10;
        bbScore += BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0x183c3c180000 ) * 20;
        //Bishop
        tempBitboard = board.GetPieceBitboard((PieceType)3, true);
        bbScore += BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0xff818181818181ff ) * -10;
        bbScore += BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0x3c7e3c7e4200  ) * 10;
        //Queen
        tempBitboard = board.GetPieceBitboard((PieceType)5, true);
        bbScore += BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0xf7818180008181e7  ) * -10;
        bbScore += BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0x3c3c3c3e0400   ) * 5;
        //King
        tempBitboard = board.GetPieceBitboard((PieceType)6, true);
        bbScore += BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0x183c0000004266    ) * 5;
        bbScore += BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0xe7c3c38181990000       ) * 5;

        //Pawn
        tempBitboard = board.GetPieceBitboard((PieceType)1, false);
        bbScore -= BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0x18ff00 ) * 40;
        bbScore -= BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0x1818240000  ) * 20;
        bbScore -= BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0x66000066c30000  ) * 10;
        bbScore -= BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0x18660000000000  ) * -10;
        //Knight
        tempBitboard = board.GetPieceBitboard((PieceType)2, false);
        bbScore -= BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0xc3810000000081c3  ) * -40;
        bbScore -= BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0x3c4281818181423c  ) * -20;
        bbScore -= BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0x18660042240000  ) * 10;
        bbScore -= BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0x183c3c180000  ) * 20;
        //Bishop
        tempBitboard = board.GetPieceBitboard((PieceType)3, false);
        bbScore -= BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0xff818181818181ff  ) * -10;
        bbScore -= BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0x427e3c7e3c0000   ) * 10;
        //Queen
        tempBitboard = board.GetPieceBitboard((PieceType)5, false);
        bbScore -= BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0xe7818100018181ef  ) * -10;
        bbScore -= BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0x207c3c3c3c0000    ) * 5;
        //King
        tempBitboard = board.GetPieceBitboard((PieceType)6, false);
        bbScore -= BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0x66420000003c1800     ) * 5;
        bbScore -= BitboardHelper.GetNumberOfSetBits(tempBitboard ^ 0x998181c3c3e7) * 5;

        boardWeight += board.IsWhiteToMove ? bbScore : -bbScore;

        //ENDGAMES

        if(board.GetPieceBitboard((PieceType)4, !board.IsWhiteToMove) == 0)
        {
            boardWeight += board.GetKingSquare(!board.IsWhiteToMove).Rank * 10 - 35;
            boardWeight += board.GetKingSquare(!board.IsWhiteToMove).File * 10 - 35;
        }

        //TODO
        //Increases weight if pawn is further up the board in the endgame
        //int pieceDepth = 7 - currentMove.TargetSquare.Rank;
        //if(colour) pieceDepth = currentMove.TargetSquare.Rank;
        //if(BitboardHelper.GetNumberOfSetBits(enemyBoard) > 7) if((int) currentMove.MovePieceType == 1) currentWeight += 50 * pieceDepth;

        //TODO
        //Increases weight if a pawn can be promoted to a queen
        //Check if any pawns are in the 7th column
        //DO WE NEED THIS???
        //if((int)currentMove.PromotionPieceType == 5) currentWeight += 500;

        return boardWeight;
    }
}