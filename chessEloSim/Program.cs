using System.CodeDom.Compiler;
using System.Security.Cryptography;
using System.Text;

namespace chessEloSim
{
    class Program
    {
        private static Random rand = new Random(7);
        static void Main(string[] args)
        {
            //round robin pairing so that each player plays each other player exactly once
            //store the results of each round (each players elo score after updating) in a file for plotting on GNUplot
            /*
             * 
             *    Trying to see how many trials it takes for the elo score to adjust to the real score 
             *    Assumes that the formula for calculating wins / losses / draws is correct
             *    
             *    ||
             *    ||      _
             *    ||     / \     _
             * e  ||-   /   \   / \      _
             * l  || \_/     \_/   \    / \
             * o  ||----------------------------- <-- real value stays the same over time
             *    ||                 \_/       
             *    ===============================
             *                nth game
             */

            //To-Do
            //add support for CLI args
            //create gnuplot script for displaying data
            //figure out how to calc W/D/L prob

            const int N_ROUNDS = 40;
            Player[] players = generatePlayers(100);

            using (StreamWriter writer = new StreamWriter("data.txt"))
            {
                writer.WriteLine(getResults(players));
                for (int i = 0; i < N_ROUNDS; ++i)
                    writer.WriteLine(playRound(players));
            }



        }
        private static String playRound(Player[] players)
        {
            for (int i = 0; i < players.Length; ++i)
            {
                for (int ii = i + 1; ii < players.Length; ++ii)
                {
                    if( ii != players.Length ) players[i].playMatch(players[ii]);
                }
            }

            return getResults(players);
        }

        private static String getResults(Player[] players)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < players.Length; ++i)
            {
                sb.Append(players[i].getPlayerElo());
                if (i < players.Length) sb.Append('\t');
            }

            return sb.ToString();
        }

        private static Player[] generatePlayers(int nPlayers)
        {
            Player[] players = new Player[nPlayers];
            for(int i = 0; i < nPlayers; ++i)
                players[i] = new Player(generateStrength(i+1, nPlayers));
            return players;
        }

        private static int generateStrength(int ticket, int nPlayers)
        {
            ////////////////////////////////////////
            // -2sd+        | 2.1%+ |    0 - 400  ||
            // -2sd to -1sd | 13.6% |  400 - 800  ||
            // -1sd to 0sd  | 34.1% |  800 - 1200 ||
            // 0sd to 1sd   | 34.1% | 1200 - 1600 ||
            // 1sd to 2sd   | 13.6% | 1600 - 2000 ||
            // 2sd+         | 2.1%+ | 2000 - 2400 ||
            ////////////////////////////////////////

            float minusTwoSD  = 0.021f;
            float minusOneSD  = minusTwoSD + 0.136f;
            float minusZeroSD = minusOneSD + 0.341f;
            float plusZeroSD  = minusZeroSD + 0.341f;
            float plusOneSD   = plusZeroSD + 0.136f;
            float plusTwoSD   = plusOneSD + 0.021f;

            const int mean = 1200;
            const int dev  = 400;

            float ratio = (float) ticket / (float) nPlayers;

            //we need to determine how many SDs this ticket has

            int sds;
            if (ratio <= minusTwoSD) sds = -3;
            else
            if (ratio > minusTwoSD && ratio <= minusOneSD) sds = -2;
            else
            if (ratio > minusOneSD && ratio <= minusZeroSD) sds = -1;
            else
            if (ratio > minusZeroSD && ratio <= plusZeroSD) sds = 0;
            else
            if (ratio > plusZeroSD && ratio <= plusOneSD) sds = 0;
            else
            if (ratio > plusOneSD && ratio <= plusTwoSD) sds = 1;
            else sds = 2;

            //bounded by # of sds and # of sds + 1
            return rand.Next(mean + (dev * sds), mean + (dev * sds) + dev);
            //take the ticket - find SD zone associated with that ticket - return a random rating in that range
        }
    }
}

