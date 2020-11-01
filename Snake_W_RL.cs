using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake_Q_RL
{
    class QRLModel
    {
        private double Alpha;
        private double Gamma;
        private double Exploration;
        private double[,,,,,][] Q;


        public QRLModel(double alpha, double gamma, double exploration, int nup, int ndown, int nleft, int nright, int nbdir, int nrdir)
        {
            Alpha = alpha;
            Gamma = gamma;
            Exploration = exploration;

            Q = new double[nup, ndown, nright, nleft, nbdir, nrdir][];
            for (int i1 = 0; i1 < Q.GetLength(0); i1++)
            {
                for (int i2 = 0; i2 < Q.GetLength(1); i2++)
                {
                    for (int i3 = 0; i3 < Q.GetLength(2); i3++)
                    {
                        for (int i4 = 0; i4 < Q.GetLength(3); i4++)
                        {
                            for (int i5 = 0; i5 < Q.GetLength(4); i5++)
                            {
                                for (int i6 = 0; i6 < Q.GetLength(5); i6++)
                                {
                                    
                                        Q[i1, i2, i3, i4, i5, i6] = new double[] { 0, 0, 0, 0 };
                                    
                                }    
                            }
                        }
                    }
                }
            }


        }


        public int GetBestaction(int up, int down, int left, int right, int dir, int rdir)
        {
            Random random = new Random();
            int maxIndex;
            if (Exploration < random.NextDouble())
            {
                maxIndex = random.Next(0, 3);
            }
            else
            {
                double[] temp = Q[up, down, left, right, dir, rdir];
                double maxValue = Q[up, down, left, right, dir, rdir].Max();
                maxIndex = temp.ToList().IndexOf(maxValue);
            }

            return maxIndex;
        }

        private void SetQvalue(int up, int down, int left, int right, int dir, int rdir, int action, double value) {
            Q[up, down, left, right, dir, rdir][action] = value;
        }

        public void UpdateQTable(int pup, int pdown, int pleft, int pright, int pdir,int prdir, int paction,
                                 int up, int down, int left, int right, int dir, int rdir, int reward) {

          double previousQ = Q[pup, pdown, pleft, pright, pdir, prdir][paction];
          double value = previousQ + Alpha * (reward + (Gamma * (Q[up, down, left, right, dir, rdir].Max())) - previousQ);
          SetQvalue(pup, pdown, pleft, pright, pdir, prdir, paction, value);
        }



    }
}
