using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace chessEloSim
{
    internal class Player
    {
        private int playerStrength;
        private int playerElo;
        private Random rand;

        public Player(int strength, int elo = 1500)
        {
            this.playerStrength = strength;
            this.playerElo = elo;
            this.rand = new Random(this.playerStrength);
        }

        public int getPlayerStrength() { return playerStrength; }
        public int getPlayerElo() { return playerElo; }

        public void updateElo(float score, float expectedScore)
        {
            const int K_FACTOR = 32;
            this.playerElo += (int)(K_FACTOR * (score - expectedScore));
        }

        public float expectedScore(Player otherPlayer)
        {
            return 1.0f / (1.0f + (float)Math.Pow(10, (otherPlayer.getPlayerElo() - this.playerElo) / 400));
        }

        enum P { WIN = 0, DRAW = 1, LOSE = 2 }
        public void playMatch(Player otherPlayer)
        {
            //=============== Get the Odds of Win / Draw / Lose ==============//
            int[] odds = calculateOdds(otherPlayer);

            //=============== Calculate actual and expected scores ==============//
            int win  =        odds[ (int)P.WIN  ];
            int draw = win  + odds[ (int)P.DRAW ];
            int lose = draw + odds[ (int)P.LOSE ];


            int roll = this.rand.Next(1, 100);
            float actual = 0;
                                                                           //actual scores
            /* this player wins   */ if      (roll <= win)                 { actual = 1.0f; }
            /* the game is a draw */ else if (roll >= draw && roll < lose) { actual = 0.5f; }
            /* this player loses  */ else   /*roll >= lose*/               { actual = 0.0f; }
            float expected = expectedScore(otherPlayer);

            //=============== Resolve the Match by updating rankings ==============//
            updateElo(actual, expected);

            // flip the values for the other player
            actual   = 1.0f - actual;
            expected = 1.0f - expected;

            otherPlayer.updateElo(actual, expected);
        }

        private int[] calculateOdds(Player otherPlayer) 
        {
            //calculate the odds of winning / losing / drawing in this function. Returns the three values in an array
            int[] odds = new int[3];
            odds[ (int) P.WIN  ] = 35;
            odds[ (int) P.DRAW ] = 30;
            odds[ (int) P.LOSE ] = 35;

            return odds;
        }
    }   
}
