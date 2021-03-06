﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SOFM
{
    /*This class represents the vector for the self organizing map. Each neuron in the lattice
     * has its own vector. The vector is a reflection of the rgb colour values that will be used in the image
     * processing. SOM vector can be used to represent the weights or sample data. */

    class SOMVector
    {
        private double[] vector;

        public SOMVector(double x, double y, double z)
        {
            vector = new double[3];
            vector[0] = x;
            vector[1] = y;
            vector[2] = z;
        }

        public SOMVector()
        { 
            vector = new double [3];
        }

        public SOMVector(int size)
        {
            vector = new double[size];
        }

        public SOMVector(double[] _vector)
        {
            vector = _vector;
        }

        /* Quick access getter and setter methods
         * Mostly used when dealing with RGB values
         * */
        public double X
        {
            set { this.vector[0] = value; }
            get { return vector[0]; }
        }

        public double Y
        {
            set { this.vector[1] = value; }
            get { return vector[1]; }
        }

        public double Z
        {
            set { this.vector[2] = value; }
            get { return vector[2]; }
        }
        //Quick access getter and setter methods

        public double [] Vector
        {
            set { this.vector = value; }
            get { return vector; }
        }

        public double getValueAtIndex(int index)
        {
            return this.vector[index];
        }

        public void setValueAtIndex(int index, double value)
        {
            this.vector[index] = value;
        }

        public int getSize()
        {
            return this.vector.Length;
        }

       
        public override string ToString()
        {
            string stringVector= "";

            for (int i = 0; i < vector.Length; i++)
            {
                stringVector += vector[i].ToString();
                if (i != (vector.Length - 1))
                    stringVector += ",";
            }
            return stringVector;
        }

        public string[] ToStringArray()
        {
            string [] stringVector = new string [vector.Length];

            for (int i = 0; i < vector.Length; i++)
            {
                stringVector[i] = vector[i].ToString();
            }

            return stringVector;
        }

        public string PrettyText()
        {
            string text = "[";
            for (int i = 0; i < vector.Length; i++)
            {
                text += vector[i] + ",";
            }
            text += "]";
            return text;
        }

    }
}
