using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SOFM
{
    class SOM
    {
        private Neuron [,] neurons; //lattice of neurons
        private int [,] bmuCount; //2d arrray representing the number of times each neuron was selected as the best matching unit
        Bitmap weightBmp;//weight bmp for weight values to use for image
        Form1 form; 
        Random rand;
        double radius;
        double learningRate;
        double decayConstant;


        //Dimension of lattice
        int rows; 
        int cols;

        public SOM(Form1 gui)
        {
            form = gui;
            rand = new Random();
            rows = form.getRows();
            cols = form.getCols();
            radius = form.getNeighbourhoodRadius();
            learningRate = form.getLearningRate();
            decayConstant = form.getDecayConstant();
        }

        public void init()
        {
            initWeights();
        }

        public void initImageClust()
        {
            initWeightsForImageClust();
        }

        //Randomly initializes the weights in the lattice
        private void initWeightsForImageClust()
        {
            neurons = new Neuron[rows, cols];
            bmuCount = new int[rows, cols];
            weightBmp = new Bitmap(rows, cols);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    neurons[i, j] = new Neuron(i, j,form.getSampleVectorSize());
                    for (int k = 0; k < neurons[i, j].WeightVector.getSize(); k++)
                    {
                        neurons[i, j].WeightVector.setValueAtIndex(k, rand.Next(0, 255));
                    }
                    weightBmp.SetPixel(i, j, Color.FromArgb((int)neurons[i, j].WeightVector.getValueAtIndex(2), (int)neurons[i, j].WeightVector.getValueAtIndex(4), (int)neurons[i, j].WeightVector.getValueAtIndex(6)));
                    bmuCount[i, j] = 0;
                }
            }
        }

        //Randomly initializes the weights in the lattice
        private void initWeights()
        {
            neurons = new Neuron[rows, cols];
            bmuCount = new int[rows, cols];
            weightBmp = new Bitmap(rows, cols);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    neurons[i, j] = new Neuron(i,j);
                    neurons[i, j].WeightVector.X = rand.Next(0, 255);
                    neurons[i, j].WeightVector.Y = rand.Next(0, 255);
                    neurons[i, j].WeightVector.Z = rand.Next(0, 255);
                    weightBmp.SetPixel(i, j, Color.FromArgb((int)neurons[i, j].WeightVector.X, (int)neurons[i, j].WeightVector.Y, (int)neurons[i, j].WeightVector.X));
                    bmuCount[i, j] = 0;
                }
            }
        }

        //Calculates the euclidean distance between the two vectors, assuming the vectors are the same size
        public double dist(SOMVector vector1, SOMVector vector2)
        {
            double sum = 0.0;
            for (int i = 0; i < vector1.getSize(); i++)
            {
                sum += Math.Pow((vector1.getValueAtIndex(i) - vector2.getValueAtIndex(i)), 2);
            }
            return Math.Sqrt(sum);
        }

        /* This method scales the neighbours of the given best matching unit.
         * */
        public void scaleNeighbours(Position bmuPos, SOMVector sample,float time)
        {
            int neighbourhoodRadius = (int)Math.Round((radius * (Math.Exp(-4.5*(time)))));
            SOMVector outer = new SOMVector(neighbourhoodRadius, neighbourhoodRadius, 0.0f);
            SOMVector zeroVector = new SOMVector(0.0f, 0.0f, 0.0f);

            for (int i = -neighbourhoodRadius; i < neighbourhoodRadius; i++)
            {
                for (int j = -neighbourhoodRadius; j < neighbourhoodRadius; j++)
                {
                    if ((bmuPos.X + i) >= 0 && ((bmuPos.X + i) < rows) && ((bmuPos.Y + j) >= 0) && ((bmuPos.Y + j) < cols))
                    {
                        outer.X = i;
                        outer.Y = j;
                        double distance = dist(zeroVector, outer);

                        //amount to scale each node depending on the distance from the bmu
                        double newLearningRate = learningRate*(Math.Exp(-(time/decayConstant)));
                        
                        //represents new vector after update is complete
                        SOMVector temp = new SOMVector(sample.getSize());

                        for (int index = 0; index < temp.getSize(); index++)
                        {
                            temp.setValueAtIndex(index, neurons[bmuPos.X + i, bmuPos.Y + j].WeightVector.getValueAtIndex(index) * (1.0f - newLearningRate) + sample.getValueAtIndex(index) * newLearningRate);
                        }

                        neurons[bmuPos.X + i, bmuPos.Y + j].WeightVector = temp;
                    }
                }
            }
            updateBitmap();
        }

        //Updates the weight bitmap with the corresponding wieght vector values in the lattice to be used for image processing purposes.
        private void updateBitmap()
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    weightBmp.SetPixel(i, j, Color.FromArgb((int)neurons[i, j].WeightVector.X, (int)neurons[i, j].WeightVector.Y, (int)neurons[i, j].WeightVector.Z));
                }
            }
        }

        /* Returns the position of the best matching unit(BMU) from the lattice.
         * The BMU is found by comparing all the weight vectors of the neurons in the lattice 
         * to the given sample vector. The one that has the smallest distance is the most accurate
         * neuron and is selected as the BMU. */
        public Position getBMU(SOMVector sample)
        {
            Position pos = new Position(0, 0);
            double bestDist = 10000.0;
            double tempDist = 0.0f;
            for (int i = 0; i < neurons.GetLength(1); i++)
            {
                for (int j = 0; j < neurons.GetLength(0); j++)
                {
                    tempDist = dist(sample, neurons[i, j].WeightVector);

                    if (tempDist < bestDist)
                    {
                        bestDist = tempDist;
                        pos.X = i; //check that the row and col lines up with x and y
                        pos.Y = j;
                    }
                }
            }
            bmuCount[pos.X, pos.Y]++;
            return pos;
        }

        //Returns the neuron at the given x and y location in the lattic
        public Neuron getUnitAt(int x, int y)
        {
            return neurons[x, y];
        }

        public Neuron[,] getWeights()
        {
            return neurons;
        }

        public int[,] getBmuCount()
        {
            return bmuCount;
        }


        //Returns the Bitmap represented by the weight vectors
        public Bitmap getWeightBitmap()
        {
            return weightBmp;
        }

    }
}
