using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgammonGame.Classes
{
    // Represents the backgammon game board and manages all pieces
    public class Board
    {
        // List of all pieces in the game
        public List<Piece> pieces { get; set; }
        
        // Checks if all white pieces are in the home board (positions 18-23)
        public bool AllWhiteHome
        {
            get
            {
                // Get all white pieces that are not out yet
                var whitePieces = pieces.Where(p => p.Color == "White" && p.Position < 24).ToList();
                // Check if all are in home positions (18-23)
                return whitePieces.All(p => p.Position >= 18 && p.Position <= 23);
            }
        }

        // Checks if all black pieces are in the home board (positions 0-5)
        public bool AllBlackHome
        {
            get
            {
                // Get all black pieces that are not out yet
                var blackPieces = pieces.Where(p => p.Color == "Black" && p.Position > -1).ToList();
                // Check if all are in home positions (0-5)
                return blackPieces.All(p => p.Position >= 0 && p.Position <= 5);
            }
        }

        // Constructor initializes the board with starting pieces
        public Board()
        {
            pieces = new List<Piece>();
            InitBoard();
        }
        
        // Sets up the initial board configuration according to backgammon rules
        public void InitBoard()
        {
            // White pieces starting positions
            pieces.Add(new Piece("White", 0));
            pieces.Add(new Piece("White", 0));
            
            // Black pieces at position 5
            pieces.Add(new Piece("Black", 5));
            pieces.Add(new Piece("Black", 5));
            pieces.Add(new Piece("Black", 5));
            pieces.Add(new Piece("Black", 5));
            pieces.Add(new Piece("Black", 5));
            
            // Black pieces at position 7
            pieces.Add(new Piece("Black", 7));
            pieces.Add(new Piece("Black", 7));
            pieces.Add(new Piece("Black", 7));
            
            // White pieces at position 11
            pieces.Add(new Piece("White", 11));
            pieces.Add(new Piece("White", 11));
            pieces.Add(new Piece("White", 11));
            pieces.Add(new Piece("White", 11));
            pieces.Add(new Piece("White", 11));
            
            // Black pieces at position 12
            pieces.Add(new Piece("Black", 12));
            pieces.Add(new Piece("Black", 12));
            pieces.Add(new Piece("Black", 12));
            pieces.Add(new Piece("Black", 12));
            pieces.Add(new Piece("Black", 12));
            
            // White pieces at position 16
            pieces.Add(new Piece("White", 16));
            pieces.Add(new Piece("White", 16));
            pieces.Add(new Piece("White", 16));
            
            // White pieces at position 18
            pieces.Add(new Piece("White", 18));
            pieces.Add(new Piece("White", 18));
            pieces.Add(new Piece("White", 18));
            pieces.Add(new Piece("White", 18));
            pieces.Add(new Piece("White", 18));
            
            // Black pieces at position 23
            pieces.Add(new Piece("Black", 23));
            pieces.Add(new Piece("Black", 23));
        }

        // Moves a piece to a new position on the board
        public void MovePiece(Piece piece, int np)
        {
            piece.MoveTo(np);
        }
    }
}