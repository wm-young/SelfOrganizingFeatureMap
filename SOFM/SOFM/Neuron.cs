using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SOFM
{
    /* A neuron represents a node in the lattice of the SOM. Each neuron has a corresponding weight vector and position in the lattice
     * */
    class Neuron
    {
        Position position;
        SOMVector weightVector;

        public Neuron(int x, int y)
        {
            position = new Position(x,y);
            weightVector = new SOMVector();
        }

        public Neuron(int x, int y,int vectorLength)
        {
            position = new Position(x, y);
            weightVector = new SOMVector(vectorLength);
        }

        public Position Position
        {
            set { position = value; }
            get { return position; }
        }

        public SOMVector WeightVector
        {
            set { weightVector = value;}
            get {return weightVector;}
        }
    }
}
