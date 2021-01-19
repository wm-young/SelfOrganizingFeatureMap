using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SOFM
{
    public partial class ParametersForm : Form
    {
        int iterations;
        int rows;
        int columns;
        double learningRate;
        double decayConstant;
        double neighbourhoodRadius;

        public ParametersForm()
        {
            InitializeComponent();
            UpdateParameterValues();
        }

        public int Iterations { get { return iterations; } }
        public int Rows{get { return rows; } }
        public int Columns{ get { return columns; } }
        public double LearningRate { get { return learningRate; } }
        public double DecayConstant { get { return decayConstant; } }
        public double NeighbourhoodRadius { get { return neighbourhoodRadius; } }

        private void okayButton_Click(object sender, EventArgs e)
        {
            UpdateParameterValues();
            this.Hide();
        }

        private void UpdateParameterValues()
        {
            iterations = Int32.Parse(iterationsTextbox.Text);
            rows = Int32.Parse(rowTextbox.Text);
            columns = Int32.Parse(colTextbox.Text);
            learningRate = Double.Parse(learningRateTextbox.Text);
            decayConstant = Double.Parse(decayConstantTextbox.Text);
            neighbourhoodRadius = Double.Parse(neighRadTextbox.Text);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            //neighRadTextbox.ClearUndo;
            iterationsTextbox.Text = iterations.ToString();
            rowTextbox.Text = rows.ToString();
            colTextbox.Text = columns.ToString();
            decayConstantTextbox.Text = decayConstant.ToString();
            neighRadTextbox.Text = neighbourhoodRadius.ToString();
            learningRateTextbox.Text = learningRate.ToString();
        }
    }
}
