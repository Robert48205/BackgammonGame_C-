using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgammonGame.Classes
{
    // Represents a player in the backgammon game
    internal class Player
    {
        // Player's display name
        public string Name { get; set; }
        
        // Player's piece color (White or Black)
        public string Color { get; set; }
        
        // Constructor to create a new player with specified name and color
        public Player(string name, string color)
        {
            Name = name;
            Color = color;
        }
        
        
    }
}
