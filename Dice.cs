using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgammonGame.Classes
{
    // Manages dice rolling functionality for the game
    internal class Dice
    {
        // Random number generator for dice rolls
        private static Random random = new Random();
        
        // Rolls a single die and returns a value between 1 and 6
        public int Roll()
        {
            return random.Next(1, 7);
        }
    }
}
