using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SOFM
{
    class Position
    {
        private int x;
        private int y;

        public Position(int theX, int theY)
        {
            x = theX;
            y = theY;
        }

        public int X
        {
            set { x = value; }
            get { return x; }
        }

        public int Y
        {
            set { y = value; }
            get { return y; }
        }
    }
}
