using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;

namespace SOFM
{  
    public partial class MainForm : Form
    {
        Random rand = new Random();
        SOMVector [] samples;
        Bitmap sampleBmp;
        SOM som;
        Image[] images;


        float t_inc;
        int iterations;
        int rows, cols;

        int ALGORITHM;
        const int COLOR = 0;
        const int IMAGECLUST = 1;

        int SAMPLE_HEIGHT = 0;
        int SAMPLE_WIDTH = 0;
        bool SAMPLES_LOADED = false;

        public MainForm()
        {
            InitializeComponent();
            
        }

        /* Generates a random image pixel by pixel between the following colours
         * Colours(8) = {red, magenta, blue, cyan, green, yellow, black, white}
         * */
        private void GenerateRandomImage()
        {
            int red, blue, green;
            int index = 0;
            sampleBmp = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            SAMPLE_HEIGHT = pictureBox2.Height;
            SAMPLE_WIDTH = pictureBox2.Width;
            samples = new SOMVector[SAMPLE_WIDTH * SAMPLE_HEIGHT];

            for (int i = 0; i < sampleBmp.Height; i++)
            {
                for (int j = 0; j < sampleBmp.Width; j++)
                {
                    red = rand.Next(0, 2);
                    blue = rand.Next(0, 2);
                    green = rand.Next(0, 2);
                    if (red == 1) red = 255;
                    if (blue == 1) blue = 255;
                    if (green == 1) green = 255;
                    Color randColor = Color.FromArgb(red, blue, green);
                    samples[index] = new SOMVector();
                    samples[index].X = randColor.R;
                    samples[index].Y = randColor.G;
                    samples[index].Z = randColor.B;
                    sampleBmp.SetPixel(i, j, randColor);
                    index++;
                }
            }

            pictureBox2.Image = sampleBmp;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GenerateRandomImage();
            startButton.Enabled = true;
        }


        private async void startButton_Click(object sender, EventArgs e)
        {
            ALGORITHM = COLOR;
            await runSom();
        }

        private Task<int> runSom()
        {

            iterations = Int32.Parse(iterationsTextbox.Text);
            t_inc = (float)1.0f / iterations;

            rows = Int32.Parse(rowTextbox.Text);
            cols = Int32.Parse(colTextbox.Text);

            som = new SOM(this);
            switch (ALGORITHM)
            {
                case COLOR:
                    som.init();
                    break;
                case IMAGECLUST:
                    som.initImageClust();
                    break;
                default:
                break;
            }

            progressBar1.Show();
            progressBar1.Value = 0;
            progressLabel.Text = "Loading...";
            progressLabel.Visible = true;
            progressLabel.Update();

            Run(); //start training the map

            progressLabel.Text = "Training complete.";

            updateDisplay(som.getWeightBitmap(), pictureBox1);
            //outputBMU(som.getBmuCount());
            testingButton.Enabled = true;

            return Task.FromResult(0);
        }

        /*This method is used to display the best matching unit results at the end of training
         * By comparing the output of the bmu's, it's possible to get a better unstanding of how the SOM is clustering 
         * the data and the points it's choosing for each cluster centroid.
         * */
        private void outputBMU(int [,] count)
        {
            Bitmap bmuBmp = new Bitmap(rows, cols);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    bmuBmp.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                }
            }
            for (int i = 0; i < 9; i++)
            {
                Position maxPos = findMax(count);
                if (maxPos != null)
                {
                    bmuBmp.SetPixel(maxPos.X, maxPos.Y, Color.FromArgb(255, 255, 255));
                    count[maxPos.X, maxPos.Y] = 0;
                }
                else
                {
                    Console.WriteLine("Could not find max unit.");
                }
            }

            updateDisplay(bmuBmp, bmuPicturebox);
        }

        private Position findMax(int[,] set)
        {
            int max = 0;
            Position pos = null;
            for (int i = 0; i < som.getBmuCount().GetLength(1); i++)
            {
                for (int j = 0; j < som.getBmuCount().GetLength(0); j++)
                {
                    if (som.getBmuCount()[i,j] >= max)
                    {
                        max = som.getBmuCount()[i, j];
                        pos = new Position(i, j);
                    }
                }
                Console.Write(Environment.NewLine);
            }
            return pos;
        }

        /* Selected a random sample from the sample data */
        private SOMVector GetRandomSample(SOMVector[] data)
        {

            int row_index = rand.Next(SAMPLE_HEIGHT);
            int col_index = rand.Next(SAMPLE_WIDTH+1);

            return data[row_index * col_index];
        }

        /* This is the main mehtod in the learning process of the self organizing map. It runs the  
         * following algorithm:
         * While(t < tMax)
         *      Grab a random sample
         *      Find the Best Matching Unit (BMU)
         *      Scale the neighbourhood of the BMU and update learning rate with respect to time t
         *      Repeat until convergence
         * Do        
         * 
         * */
        private void Run()
        {
            int count = 0;
            updateDisplay(som.getWeightBitmap(), pictureBox1);
            SOMVector randSample;
            Position bmu;
            
            progressBar1.Maximum = iterations;

            for (float t=0.0f; t < 1.0; t+=t_inc)
            {
                if (count%50 == 0 && ALGORITHM == IMAGECLUST)
                {
                    panel1.Controls.Clear();
                    displayImageClusters();
                }
                randSample = GetRandomSample(samples);
                bmu = som.getBMU(randSample);
                som.scaleNeighbours(bmu, randSample, t);

                progressBar1.Increment(1); 
                progressBar1.Update();
                pictureBox1.Refresh();

                count++;
            }
        }//run


        private void displayImageClusters()
        {
            PictureBox imagePb;
            SOMVector aSample;
            Position posBMU;
            Bitmap bmuBmp = new Bitmap(rows, cols);
            double widthPercentage = (panel1.Width - 40) / cols;
            double heightPercentage = (panel1.Height - 40) / rows;

            int xfactor = 0;
            int yfactor = 0;

            for (int i = 0; i < images.Count(); i++)
            {
                imagePb = new PictureBox();
                imagePb.Height = 50;
                imagePb.Width = 50;
                aSample = samples[i];
                posBMU = som.getBMU(aSample);
                imagePb.Image = images[i];
                xfactor = (int)(posBMU.X * widthPercentage);
                yfactor = (int)(posBMU.Y * heightPercentage);

                imagePb.Location = new Point(xfactor, yfactor);
                imagePb.SizeMode = PictureBoxSizeMode.StretchImage;
                panel1.Controls.Add(imagePb);

                /*BMU Display bitmap */
                for (int k = 0; k < rows; k++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        bmuBmp.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                    }
                }
                bmuBmp.SetPixel(posBMU.X, posBMU.Y, Color.FromArgb(50, 255, 50));

            }
            //updateDisplay(bmuBmp, bmuPicturebox);
            this.Refresh();
        }

        //Returns the bitmap of the sample data
        public Bitmap getSampleBitmap()
        {
            return sampleBmp;
        }

        //updates the outputdisplay with the given bitmap file and given picturebox
        public void updateDisplay(Bitmap bmp, PictureBox window)
        {
            window.Refresh();
            window.Image = bmp;

            if (bmp.Width < window.Width || bmp.Width < window.Height)
            {
                window.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            else if (bmp.Width > window.Width || bmp.Width > window.Height)
            {
                window.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            else
            {
                window.SizeMode = PictureBoxSizeMode.Normal;
            }

        }

        private void LoadImages()
        {
            Bitmap bitmap;

            string [] image_path = Directory.GetFiles(@Environment.CurrentDirectory + "\\images\\", "*"+"kong*.jpg");
            images = new Image[image_path.Count()];
            displayBox.AppendText("Loading "+ image_path.Count() + " images from: " + @Environment.CurrentDirectory + "\\images\\" + Environment.NewLine);
            for (int i = 0; i < image_path.Count(); i++)
            {
                displayBox.AppendText(image_path[i].Substring(image_path[i].LastIndexOf('\\')) + Environment.NewLine);
                Image image = Image.FromFile(image_path[i]);
                bitmap = new Bitmap(image);
                images[i] = bitmap;
            }
            displayBox.AppendText("Done." + Environment.NewLine);

            //Set the display images for visual aid to know which images are being selected for sample creation
            imageCollectionInputPicturebox1.Image = images[0];
            imageCollectionInputPicturebox1.SizeMode = PictureBoxSizeMode.StretchImage;
            imageCollectionInputPicturebox1.Refresh();

            imageCollectionInputPicturebox2.Image = images[3];
            imageCollectionInputPicturebox2.SizeMode = PictureBoxSizeMode.StretchImage;
            imageCollectionInputPicturebox2.Refresh();

            imageCollectionInputPicturebox3.Image = images[6];
            imageCollectionInputPicturebox3.SizeMode = PictureBoxSizeMode.StretchImage;
            imageCollectionInputPicturebox3.Refresh();

            imageCollectionInputPicturebox4.Image = images[9];
            imageCollectionInputPicturebox4.SizeMode = PictureBoxSizeMode.StretchImage;
            imageCollectionInputPicturebox4.Refresh();

            imageCollectionInputPicturebox5.Image = images[12];
            imageCollectionInputPicturebox5.SizeMode = PictureBoxSizeMode.StretchImage;
            imageCollectionInputPicturebox5.Refresh();

            imageCollectionInputPicturebox6.Image = images[15];
            imageCollectionInputPicturebox6.SizeMode = PictureBoxSizeMode.StretchImage;
            imageCollectionInputPicturebox6.Refresh();

            imageCollectionInputPicturebox7.Image = images[17];
            imageCollectionInputPicturebox7.SizeMode = PictureBoxSizeMode.StretchImage;
            imageCollectionInputPicturebox7.Refresh();

            InitializeSamples(1, images.Length); //-1 for the trailing newline char
            //After images, load the sample vectors from a file
            samples = LoadSamplesFromFile(@Environment.CurrentDirectory + "\\saves\\vectors\\kongs.vec");

            //If samples have not been loaded, create samples from images and save to file
            if(!SAMPLES_LOADED)
            {
                double[] v2;

                displayBox.AppendText("Constructing sample vectors from images, this may take awhile..." + Environment.NewLine);
                for (int i = 0; i < images.Length; i++)
                {
                    v2 = partitionImage(new Bitmap(images[i]));
                    samples[i] = new SOMVector(v2.Count());
                    samples[i].Vector = v2;
                }
                displayBox.AppendText("Done." + Environment.NewLine);
                SaveSamplesToFile(samples, @Environment.CurrentDirectory + "\\saves\\vectors\\kongs.vec");
            }

            startButton2.Enabled = true;
        }


        /* This method partitions a given image into 9 equal parts, namely:
         *  [upper left,    upper middle,     upper right]
         *  [left middle,   middle,           right middle]
         *  [bottom left,   bottom middle,    bottom right]
         *  Once that is done the corresponding vector for each image is added to the samples collection
         *  The vector sample vector is also constructed and added to the list of samples to be used for testing.
         *  The vector is of the following form:
         *  v1 =[mu(RGB),sig(
         * */
        private double [] partitionImage(Bitmap image)
        {
            //Bitmap testImage = new Bitmap( image.Width,image.Height);
            int count = 0; //used to count the number of pixels in a given partition
            int pixelCount= 0; //number of pixels in the entire image

            int partition = 8; //keep track of which partition of the image is the current

            /*Values to construct vector 1 that represent the whole image */
            int totalRed = 0;
            int totalGreen = 0;
            int totalBlue = 0;
            int totalRGB = 0;

            /* Values to construct vector 2 that represents each partition */
            int partitionRed = 0;
            int partitionGreen = 0;
            int partitionBlue = 0;
            int partitionRGB = 0;
            int avgPartitionRGB = 0;

            double stdevRGB = 0;

            int height_inc = (int)Math.Floor((double)(image.Height / 3));
            int width_inc = (int)Math.Floor((double)(image.Width / 3));

            double [] vector = new double[26]; //vector that represents a given image based on 
            
            //loop through 3x3 partitions
            for (int height = height_inc; height <= image.Height; height += height_inc)
            {
                for (int width = width_inc; width <= image.Width; width += width_inc)
                {
                    //Pixel by pixel for each partition
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                           // Console.WriteLine("Pixel : i: " + i + " j: " + j + ".R" + image.GetPixel(j, i).R + ".G" + image.GetPixel(j, i).G + ".B" + image.GetPixel(j, i).B);
                            Color color = image.GetPixel(j, i);
                            totalRed += color.R;
                            totalBlue += color.G;
                            totalGreen += color.B;

                            partitionRed += color.R;
                            partitionGreen += color.G;
                            partitionBlue += color.B;

                            //testImage.SetPixel(j,i, Color.FromArgb(color.R, color.G, color.B));

                            count++;
                            pixelCount++;
                        }
                    }

                    totalRGB = checked((totalRed + totalBlue + totalGreen) / 3);
                    partitionRGB = (partitionRed + partitionGreen + partitionBlue) / 3;

                    //Constuct sample vector
                    avgPartitionRGB = partitionRGB / count;
                    //loop through and calculate st.deviation
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            stdevRGB += Math.Pow((image.GetPixel(j, i).R - avgPartitionRGB), 2);
                        }
                    }

                    stdevRGB = Math.Sqrt(stdevRGB/count);
                    vector[partition] = (double)avgPartitionRGB;
                    vector[partition + 1] = (double)stdevRGB;
                    partition = partition + 2;

                    count = 0;
                    partitionRed = 0;
                    partitionGreen = 0;
                    partitionBlue = 0;
                }
            }

            int avgTotalRed = totalRed / pixelCount;
            int avgTotalGreen = totalGreen / pixelCount;
            int avgTotalBlue = totalBlue / pixelCount;
            long avgTotalRGB = totalRGB/pixelCount;

            double stDevTotalRed = 0.0;
            double stDevTotalGreen = 0.0;
            double stDevTotalBlue = 0.0;
            double stDevTotalRGB = 0.0;

            //Calculate standard deviations
            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    Color color = image.GetPixel(j, i);
                    stDevTotalRed += Math.Pow((image.GetPixel(j, i).R - avgTotalRed), 2);
                    stDevTotalGreen += Math.Pow((image.GetPixel(j, i).G - avgTotalGreen), 2);
                    stDevTotalBlue += Math.Pow((image.GetPixel(j, i).B - avgTotalRGB), 2);
                    stDevTotalRGB = stDevTotalRed + stDevTotalGreen + stDevTotalBlue / 3;
                    count++;
                }
            }

            stDevTotalRed = Math.Sqrt(stDevTotalRed / count);
            stDevTotalGreen = Math.Sqrt(stDevTotalGreen / count);
            stDevTotalBlue = Math.Sqrt(stDevTotalBlue / count);
            stDevTotalRGB = Math.Sqrt(stDevTotalRGB / count);
            vector[0] = (double)avgTotalRGB;
            vector[1] = (double)stDevTotalRGB;
            vector[2] = (double)avgTotalRed;
            vector[3] = (double)stDevTotalRed;
            vector[4] = (double)avgTotalGreen;
            vector[5] = (double)stDevTotalGreen;
            vector[6] = (double)avgTotalBlue;
            vector[7] = (double)stDevTotalBlue;
            return vector;
        }//partitionImage

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public int getRows()
        {
            return rows;
        }

        public int getCols()
        {
            return cols;
        }

        public PictureBox getPictureBox()
        {
            return pictureBox1;
        }

        public double getLearningRate()
        {
            return Double.Parse(learningRateTextbox.Text);
        }

        private void loadImageCollectionButton_Click(object sender, EventArgs e)
        {
            genRandImgButton.Enabled = false;
            LoadImages();

        }

        public double getDecayConstant()
        {
            return Double.Parse(decayConstantTextbox.Text);
        }

        public double getNeighbourhoodRadius()
        {
            return Double.Parse(neighRadTextbox.Text);
        }

        public int getSampleVectorSize()
        {
            return samples[0].getSize();
        }

        public int getSampleSize()
        {
            return samples.Count();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("" +
                "1) To start color organization, click 'Generate random image' then click 'Start'" + Environment.NewLine + Environment.NewLine +
                "2) To start Image Clustering, click 'Load Collection of Images', then click 'Start'" + Environment.NewLine + Environment.NewLine +
                "3) After Image Clustering, you can test the network for accuracy by feeding the testing samples through by clicking on 'Start Testing'" +
                "");
        }

        private void testingButton_Click(object sender, EventArgs e)
        {

        }

        private void groupBox6_Enter(object sender, EventArgs e)
        {

        }

        // Loads sample vectors from the given file path. Parses each line of strings
        // and converts the values into double values when instantiating the SOMVector
        // Returns the SOMVector [] when read from the file successfully, null otherwise
        private SOMVector[] LoadSamplesFromFile(string filePath)
        {
            SOMVector[] samples;
            try
            {
                StreamReader reader = new StreamReader(filePath, System.Text.Encoding.Default);
                string[] values = reader.ReadToEnd().Split('\n');
                reader.Close();
                samples = new SOMVector[values.Length - 1]; //-1 for the trailing newline char
                //InitializeSamples(1, values.Length - 1); //-1 for the trailing newline char

                for (int i = 0; i < values.Length - 1; i++)
                {
                    System.Console.WriteLine(values[i]);
                    string[] line = values[i].Split(',');
                    double[] vector = new double[line.Length];
                    for (int j = 0; j < line.Length; j++)
                    {
                        vector[j] = Double.Parse(line[j]);
                    }
                    samples[i] = new SOMVector(vector);
                }

                SAMPLES_LOADED = true;

                return samples;
            }
            catch (FileNotFoundException e)
            {
                displayBox.AppendText("File not found: " + filePath + Environment.NewLine);
            }
            catch (IOException e)
            {
                MessageBox.Show("Unexpected error occured while attempting to read file:" +
                    Environment.NewLine +
                    filePath + Environment.NewLine +
                    e.StackTrace);
            }
            
            return null;
        }

        private bool SaveSamplesToFile(SOMVector[] vectors, string filePath)
        {
            string [] stringVectors = new string[vectors.Length];

            for(int i = 0; i < vectors.Length; i++)
            { 
                stringVectors[i] = vectors[i].ToString();
            }
            displayBox.AppendText("Saving samples to file..." + Environment.NewLine);
            System.IO.File.WriteAllLines(filePath, stringVectors);
            displayBox.AppendText("Saved.");
            return true;
        }

        private void InitializeSamples(int width, int height)
        {
            SAMPLE_HEIGHT = height;
            SAMPLE_WIDTH = width;
            samples = new SOMVector[SAMPLE_WIDTH * SAMPLE_HEIGHT];
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LoadSamplesFromFile(@Environment.CurrentDirectory + "\\saves\\vectors\\kongs.vec");
            displayBox.AppendText("Samples successfully loaded from:" + @Environment.CurrentDirectory + "\\saves\\vectors\\kongs.vec" + Environment.NewLine);
            //Console.WriteLine(samples);
        }

        private void startButton2_Click(object sender, EventArgs e)
        {
            ALGORITHM = IMAGECLUST;
            runSom();
        }

    }
}
