using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgammonGame.Classes
{
    // Represents a single game piece on the backgammon board
    public class Piece
    {
        // Color of the piece (White or Black)
        public string Color { get; set; }
        
        // Current position of the piece on the board (0-23 for regular positions, -1 for Black OUT, 24 for White OUT, -2 for Black BAR, 25 for White BAR)
        public int Position { get; set; }
        
        // Constructor to create a new piece with specified color and position
        public Piece(string color, int position)
        {
            Color = color;
            Position = position;
        }
        
        // Moves the piece to a new position
        public void MoveTo(int np)
        {
            Position = np;
        }
    }
}