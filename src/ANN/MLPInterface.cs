﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANN
{

    /// <summary>
    /// Class to interface with the MLP for test/example purposes
    /// </summary>
    public class MLPInterface
    {
        /// <summary>
        /// THe MLP object
        /// </summary>
        private MLP nn;

        /// <summary>
        /// All of the values from training data (unfiltered)
        /// </summary>
        private float[,] fullTrainingSet;

        /// <summary>
        /// All of the values from testing data (unfiltered)
        /// </summary>
        private float[,] fullTestingSet;

        /// <summary>
        /// 2d array of training data
        /// </summary>
        private float[,] trainingSet;

        /// <summary>
        /// 2d array of testing data
        /// </summary>
        private float[,] testingSet;

        /// <summary>
        /// Constructor to test training and testing of the neural network with some example data hardcoded
        /// </summary>
        public MLPInterface()
        {
            int[] hiddenStructure = {18};
            // 12 Inputs | 7 Outputs | 8 hidden ...
            nn = new MLP(18, 7, hiddenStructure, 0.02F, 0F);
            trainMLP();
        }

        /// <summary>
        /// Trains & tests the MLP
        /// </summary>
        private void trainMLP()
        {
            // Get the training and testing data
            getTrainingData();
            getTestingData();

            // Get training length
            int numOfTraining = 34500;
            // Get testing length
            int numOfTesting = 1400;

            // Filter the data needed
            filterFullData(34500, 1400);

            this.nn.TrainNetwork(numOfTraining, trainingSet);
            this.nn.TestNetwork(numOfTesting, testingSet);
        }

        /// <summary>
        /// Gets the training data from the files
        /// </summary>
        private void getTrainingData()
        {
            string path = @"..\..\..\training-files\training-data.txt";
            // Opens stream, gets data AND closes stream
            String training_file = System.IO.File.ReadAllText(path);

            int i = 0, j = 0;
            this.fullTrainingSet = new float[34500, 61];
            // Reads in the data
            foreach (var row in training_file.Split('\n'))
            {
                j = 0;
                foreach (var col in row.Trim().Split(','))
                {
                    this.fullTrainingSet[i, j] = float.Parse(col.Trim());
                    j++;
                }
                i++;
            }
        }

        /// <summary>
        /// Gets the testing data from the file
        /// </summary>
        private void getTestingData()
        {
            string path = @"..\..\..\training-files\testing-data.txt";
            String testing_file = System.IO.File.ReadAllText(path);

            int i = 0, j = 0;
            this.fullTestingSet = new float[1400, 61];
            // Reads in the testing data
            foreach (var row in testing_file.Split('\n'))
            {
                j = 0;
                foreach (var col in row.Trim().Split(','))
                {
                    this.fullTestingSet[i, j] = float.Parse(col.Trim());
                    j++;
                }
                i++;
            }
        }

        /// <summary>
        /// Filters out the points needed and calculates the measurements
        /// </summary>
        private void filterFullData(int numOfTraining, int numOfTesting)
        {
            this.trainingSet = new float[34500, 25];
            for (int i = 0; i < numOfTraining; i++)
            {
                // Head to Left Hand
                this.trainingSet[i, 0] = toFloat(Math.Sqrt(Math.Pow((this.fullTrainingSet[i, 0] + this.fullTrainingSet[i, 36]), 2) + Math.Pow((this.fullTrainingSet[i, 1] + this.fullTrainingSet[i, 37]), 2)));
                // Head to Left Wrist
                this.trainingSet[i, 1] = toFloat(Math.Sqrt(Math.Pow((this.fullTrainingSet[i, 0] + this.fullTrainingSet[i, 30]), 2) + Math.Pow((this.fullTrainingSet[i, 1] + this.fullTrainingSet[i, 31]), 2)));
                // Head to Left Elbow
                this.trainingSet[i, 2] = toFloat(Math.Sqrt(Math.Pow((this.fullTrainingSet[i, 0] + this.fullTrainingSet[i, 24]), 2) + Math.Pow((this.fullTrainingSet[i, 1] + this.fullTrainingSet[i, 25]), 2)));
                // Head to Right Elbow
                this.trainingSet[i, 3] = toFloat(Math.Sqrt(Math.Pow((this.fullTrainingSet[i, 0] + this.fullTrainingSet[i, 27]), 2) + Math.Pow((this.fullTrainingSet[i, 1] + this.fullTrainingSet[i, 28]), 2)));
                // Head to Right Wrist
                this.trainingSet[i, 4] = toFloat(Math.Sqrt(Math.Pow((this.fullTrainingSet[i, 0] + this.fullTrainingSet[i, 33]), 2) + Math.Pow((this.fullTrainingSet[i, 1] + this.fullTrainingSet[i, 34]), 2)));
                // Head to Right Hand
                this.trainingSet[i, 5] = toFloat(Math.Sqrt(Math.Pow((this.fullTrainingSet[i, 0] + this.fullTrainingSet[i, 39]), 2) + Math.Pow((this.fullTrainingSet[i, 1] + this.fullTrainingSet[i, 40]), 2)));
                // Left Hand to Left Shoulder
                this.trainingSet[i, 6] = toFloat(Math.Sqrt(Math.Pow((this.fullTrainingSet[i, 36] + this.fullTrainingSet[i, 6]), 2) + Math.Pow((this.fullTrainingSet[i, 37] + this.fullTrainingSet[i, 7]), 2)));
                // Left Wrist to Left Shoulder
                this.trainingSet[i, 7] = toFloat(Math.Sqrt(Math.Pow((this.fullTrainingSet[i, 30] + this.fullTrainingSet[i, 6]), 2) + Math.Pow((this.fullTrainingSet[i, 31] + this.fullTrainingSet[i, 7]), 2)));
                // Right Hand to Right Shoulder
                this.trainingSet[i, 8] = toFloat(Math.Sqrt(Math.Pow((this.fullTrainingSet[i, 39] + this.fullTrainingSet[i, 9]), 2) + Math.Pow((this.fullTrainingSet[i, 40] + this.fullTrainingSet[i, 10]), 2)));
                // Right Wrist to Right Shoulder
                this.trainingSet[i, 9] = toFloat(Math.Sqrt(Math.Pow((this.fullTrainingSet[i, 33] + this.fullTrainingSet[i, 9]), 2) + Math.Pow((this.fullTrainingSet[i, 34] + this.fullTrainingSet[i, 10]), 2)));
                // Left Hand to spine
                this.trainingSet[i, 10] = toFloat(Math.Sqrt(Math.Pow((this.fullTrainingSet[i, 15] + this.fullTrainingSet[i, 39]), 2) + Math.Pow((this.fullTrainingSet[i, 16] + this.fullTrainingSet[i, 40]), 2)));
                // Right Hand to spine
                this.trainingSet[i, 11] = toFloat(Math.Sqrt(Math.Pow((this.fullTrainingSet[i, 15] + this.fullTrainingSet[i, 42]), 2) + Math.Pow((this.fullTrainingSet[i, 16] + this.fullTrainingSet[i, 43]), 2)));

                // Left Hand to Right Hand
                this.trainingSet[i, 12] = toFloat(Math.Sqrt(Math.Pow((this.fullTrainingSet[i, 36] + this.fullTrainingSet[i, 39]), 2) + Math.Pow((this.fullTrainingSet[i, 37] + this.fullTrainingSet[i, 40]), 2)));
                // Left Elbow to Right Elbow
                this.trainingSet[i, 13] = toFloat(Math.Sqrt(Math.Pow((this.fullTrainingSet[i, 24] + this.fullTrainingSet[i, 27]), 2) + Math.Pow((this.fullTrainingSet[i, 25] + this.fullTrainingSet[i, 28]), 2)));
                // Left Shoulder to Right Hand
                this.trainingSet[i, 14] = toFloat(Math.Sqrt(Math.Pow((this.fullTrainingSet[i, 6] + this.fullTrainingSet[i, 39]), 2) + Math.Pow((this.fullTrainingSet[i, 7] + this.fullTrainingSet[i, 40]), 2)));
                // Right Shoulder to Left Hand
                this.trainingSet[i, 15] = toFloat(Math.Sqrt(Math.Pow((this.fullTrainingSet[i, 9] + this.fullTrainingSet[i, 36]), 2) + Math.Pow((this.fullTrainingSet[i, 10] + this.fullTrainingSet[i, 37]), 2)));
                // Left Hand to Left Knee
                this.trainingSet[i, 16] = toFloat(Math.Sqrt(Math.Pow((this.fullTrainingSet[i, 36] + this.fullTrainingSet[i, 42]), 2) + Math.Pow((this.fullTrainingSet[i, 37] + this.fullTrainingSet[i, 43]), 2)));
                // Right Hand to Right Knee
                this.trainingSet[i, 17] = toFloat(Math.Sqrt(Math.Pow((this.fullTrainingSet[i, 39] + this.fullTrainingSet[i, 45]), 2) + Math.Pow((this.fullTrainingSet[i, 40] + this.fullTrainingSet[i, 46]), 2)));

                // 54-60 are expected outputs
                this.trainingSet[i, 18] = this.fullTrainingSet[i, 54];
                this.trainingSet[i, 19] = this.fullTrainingSet[i, 55];
                this.trainingSet[i, 20] = this.fullTrainingSet[i, 56];
                this.trainingSet[i, 21] = this.fullTrainingSet[i, 57];
                this.trainingSet[i, 22] = this.fullTrainingSet[i, 58];
                this.trainingSet[i, 23] = this.fullTrainingSet[i, 59];
                this.trainingSet[i, 24] = this.fullTrainingSet[i, 60];
            }

            this.testingSet = new float[1400, 25];

            // Same order as above

            for (int i = 0; i < numOfTesting; i++)
            {
                this.testingSet[i, 0] = toFloat(Math.Sqrt(Math.Pow((this.fullTestingSet[i, 0] + this.fullTestingSet[i, 36]), 2) + Math.Pow((this.fullTestingSet[i, 1] + this.fullTestingSet[i, 37]), 2)));
                this.testingSet[i, 1] = toFloat(Math.Sqrt(Math.Pow((this.fullTestingSet[i, 0] + this.fullTestingSet[i, 30]), 2) + Math.Pow((this.fullTestingSet[i, 1] + this.fullTestingSet[i, 31]), 2)));
                this.testingSet[i, 2] = toFloat(Math.Sqrt(Math.Pow((this.fullTestingSet[i, 0] + this.fullTestingSet[i, 24]), 2) + Math.Pow((this.fullTestingSet[i, 1] + this.fullTestingSet[i, 25]), 2)));
                this.testingSet[i, 3] = toFloat(Math.Sqrt(Math.Pow((this.fullTestingSet[i, 0] + this.fullTestingSet[i, 27]), 2) + Math.Pow((this.fullTestingSet[i, 1] + this.fullTestingSet[i, 28]), 2)));
                this.testingSet[i, 4] = toFloat(Math.Sqrt(Math.Pow((this.fullTestingSet[i, 0] + this.fullTestingSet[i, 33]), 2) + Math.Pow((this.fullTestingSet[i, 1] + this.fullTestingSet[i, 34]), 2)));
                this.testingSet[i, 5] = toFloat(Math.Sqrt(Math.Pow((this.fullTestingSet[i, 0] + this.fullTestingSet[i, 39]), 2) + Math.Pow((this.fullTestingSet[i, 1] + this.fullTestingSet[i, 40]), 2)));
                this.testingSet[i, 6] = toFloat(Math.Sqrt(Math.Pow((this.fullTestingSet[i, 36] + this.fullTestingSet[i, 6]), 2) + Math.Pow((this.fullTestingSet[i, 37] + this.fullTestingSet[i, 7]), 2)));
                this.testingSet[i, 7] = toFloat(Math.Sqrt(Math.Pow((this.fullTestingSet[i, 30] + this.fullTestingSet[i, 6]), 2) + Math.Pow((this.fullTestingSet[i, 31] + this.fullTestingSet[i, 7]), 2)));
                this.testingSet[i, 8] = toFloat(Math.Sqrt(Math.Pow((this.fullTestingSet[i, 39] + this.fullTestingSet[i, 9]), 2) + Math.Pow((this.fullTestingSet[i, 40] + this.fullTestingSet[i, 10]), 2)));
                this.testingSet[i, 9] = toFloat(Math.Sqrt(Math.Pow((this.fullTestingSet[i, 33] + this.fullTestingSet[i, 9]), 2) + Math.Pow((this.fullTestingSet[i, 34] + this.fullTestingSet[i, 10]), 2)));
                // L hand to spine
                this.testingSet[i, 10] = toFloat(Math.Sqrt(Math.Pow((this.fullTestingSet[i, 15] + this.fullTestingSet[i, 39]), 2) + Math.Pow((this.fullTestingSet[i, 16] + this.fullTestingSet[i, 40]), 2)));
                // R hand to spine
                this.testingSet[i, 11] = toFloat(Math.Sqrt(Math.Pow((this.fullTestingSet[i, 15] + this.fullTestingSet[i, 42]), 2) + Math.Pow((this.fullTestingSet[i, 16] + this.fullTestingSet[i, 43]), 2)));

                // Left Hand to Right Hand
                this.testingSet[i, 12] = toFloat(Math.Sqrt(Math.Pow((this.fullTestingSet[i, 36] + this.fullTestingSet[i, 39]), 2) + Math.Pow((this.fullTestingSet[i, 37] + this.fullTestingSet[i, 40]), 2)));
                // Left Elbow to Right Elbow
                this.testingSet[i, 13] = toFloat(Math.Sqrt(Math.Pow((this.fullTestingSet[i, 24] + this.fullTestingSet[i, 27]), 2) + Math.Pow((this.fullTestingSet[i, 25] + this.fullTestingSet[i, 28]), 2)));
                // Left Shoulder to Right Hand
                this.testingSet[i, 14] = toFloat(Math.Sqrt(Math.Pow((this.fullTestingSet[i, 6] + this.fullTestingSet[i, 39]), 2) + Math.Pow((this.fullTestingSet[i, 7] + this.fullTestingSet[i, 40]), 2)));
                // Right Shoulder to Left Hand
                this.testingSet[i, 15] = toFloat(Math.Sqrt(Math.Pow((this.fullTestingSet[i, 9] + this.fullTestingSet[i, 36]), 2) + Math.Pow((this.fullTestingSet[i, 10] + this.fullTestingSet[i, 37]), 2)));
                // Left Hand to Left Knee
                this.testingSet[i, 16] = toFloat(Math.Sqrt(Math.Pow((this.fullTestingSet[i, 36] + this.fullTestingSet[i, 42]), 2) + Math.Pow((this.fullTestingSet[i, 37] + this.fullTestingSet[i, 43]), 2)));
                // Right Hand to Right Knee
                this.testingSet[i, 17] = toFloat(Math.Sqrt(Math.Pow((this.fullTestingSet[i, 39] + this.fullTestingSet[i, 45]), 2) + Math.Pow((this.fullTestingSet[i, 40] + this.fullTestingSet[i, 46]), 2)));

                // 54-60 are expected outputs
                this.testingSet[i, 18] = this.fullTestingSet[i, 54];
                this.testingSet[i, 19] = this.fullTestingSet[i, 55];
                this.testingSet[i, 20] = this.fullTestingSet[i, 56];
                this.testingSet[i, 21] = this.fullTestingSet[i, 57];
                this.testingSet[i, 22] = this.fullTestingSet[i, 58];
                this.testingSet[i, 23] = this.fullTestingSet[i, 59];
                this.testingSet[i, 24] = this.fullTestingSet[i, 60];
            }
        }


        /// <summary>
        /// Calls the MLP
        /// </summary>
        /// <param name="inputs">
        ///     The skeleton points
        /// </param>
        /// <returns>
        ///     A string representing the gesture classification
        /// </returns>
        public String recall(float[] inputs)
        {
            float[] distances = measureInputs(inputs);

            return classifyGesture(distances);
        }

        /// <summary>
        /// Finds the Euclidean distance between two skeleton points using the x & y axis (not z)
        /// </summary>
        /// <param name="joints">
        ///     An array of floats containing the used joint data
        /// </param>
        /// <returns>
        ///     An array of distances
        ///         Order is shown in the comments
        /// </returns>
        public float[] measureInputs(float[] joints)
        {
            float[] measuredInputs = new float[18];

            // Head to left hand
            measuredInputs[0] = toFloat(Math.Sqrt(Math.Pow((joints[0] + joints[14]), 2) + Math.Pow((joints[1] + joints[15]), 2)));
            // Head to left wrist
            measuredInputs[1] = toFloat(Math.Sqrt(Math.Pow((joints[0] + joints[10]), 2) + Math.Pow((joints[1] + joints[11]), 2)));
            // Head to left elbow
            measuredInputs[2] = toFloat(Math.Sqrt(Math.Pow((joints[0] + joints[6]), 2) + Math.Pow((joints[1] + joints[7]), 2)));
            // Head to right elbow
            measuredInputs[3] = toFloat(Math.Sqrt(Math.Pow((joints[0] + joints[8]), 2) + Math.Pow((joints[1] + joints[9]), 2)));
            // Head to right wrist
            measuredInputs[4] = toFloat(Math.Sqrt(Math.Pow((joints[0] + joints[12]), 2) + Math.Pow((joints[1] + joints[13]), 2)));
            // Head to right hand
            measuredInputs[5] = toFloat(Math.Sqrt(Math.Pow((joints[0] + joints[16]), 2) + Math.Pow((joints[1] + joints[17]), 2)));
            // Hand left to shoulder left
            measuredInputs[6] = toFloat(Math.Sqrt(Math.Pow((joints[14] + joints[2]), 2) + Math.Pow((joints[15] + joints[3]), 2)));
            // wrist left to shoulder left
            measuredInputs[7] = toFloat(Math.Sqrt(Math.Pow((joints[10] + joints[2]), 2) + Math.Pow((joints[11] + joints[3]), 2)));
            // Hand right to shoulder right
            measuredInputs[8] = toFloat(Math.Sqrt(Math.Pow((joints[16] + joints[4]), 2) + Math.Pow((joints[17] + joints[5]), 2)));
            // Wrist right to shoulder right
            measuredInputs[9] = toFloat(Math.Sqrt(Math.Pow((joints[12] + joints[4]), 2) + Math.Pow((joints[13] + joints[5]), 2)));
            // Left hand to spine
            measuredInputs[10] = toFloat(Math.Sqrt(Math.Pow((joints[18] + joints[14]), 2) + Math.Pow((joints[19] + joints[15]), 2)));
            // Right hand to spine
            measuredInputs[11] = toFloat(Math.Sqrt(Math.Pow((joints[18] + joints[16]), 2) + Math.Pow((joints[19] + joints[17]), 2)));

            // Left Hand to Right Hand
            measuredInputs[12] = toFloat(Math.Sqrt(Math.Pow((joints[14] + joints[16]), 2) + Math.Pow((joints[15] + joints[17]), 2)));
            // Left Elbow to Right Elbow
            measuredInputs[13] = toFloat(Math.Sqrt(Math.Pow((joints[6] + joints[8]), 2) + Math.Pow((joints[7] + joints[9]), 2)));
            // Left Shoulder to Right Hand
            measuredInputs[14] = toFloat(Math.Sqrt(Math.Pow((joints[2] + joints[16]), 2) + Math.Pow((joints[3] + joints[17]), 2)));
            // Right Shoulder to Left Hand
            measuredInputs[15] = toFloat(Math.Sqrt(Math.Pow((joints[4] + joints[14]), 2) + Math.Pow((joints[5] + joints[15]), 2)));
            // Left Hand to Left Knee
            measuredInputs[16] = toFloat(Math.Sqrt(Math.Pow((joints[14] + joints[20]), 2) + Math.Pow((joints[15] + joints[21]), 2)));
            // Right Hand to Right Knee
            measuredInputs[17] = toFloat(Math.Sqrt(Math.Pow((joints[16] + joints[22]), 2) + Math.Pow((joints[17] + joints[23]), 2)));

            return measuredInputs;
        }

        /// <summary>
        ///     Handles the conversion of a double to float
        ///     whilst avoiding hitting +/- infinity.
        /// </summary>
        /// <param name="input">
        ///     The value to convert to float
        /// </param>
        /// <returns>
        ///     A float value
        /// </returns>
        private float toFloat(double input)
        {
            float result = (float)input;
            if (float.IsPositiveInfinity(result))
            {
                result = float.MaxValue;
            }
            else if (float.IsNegativeInfinity(result))
            {
                result = float.MinValue;
            }

            return result;

        }

        /// <summary>
        ///     Classifies
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public string classifyGesture(float[] inputs)
        {
            String gesture = "";
            float[] outputs = this.nn.RecallNetwork(inputs);
            float maxValue = outputs.Max();
            // If the strongest node is above the threshold of 0.95
            if (maxValue > 0.95F)
            {
                int maxIndex = outputs.ToList().IndexOf(maxValue);
                foreach (float output in outputs)
                {
                    Console.WriteLine(Math.Round(output, 3));
                }
                switch (maxIndex)
                {
                    case 0:
                        // Play
                        return "PLAY";
                    case 1:
                        // Pause
                        return "PAUSE";
                    case 2:
                        // Skip Forward
                        return "SKIP";
                    case 3:
                        // Go Backward
                        return "BACK";
                    case 4:
                        // Volume Up
                        return "VUP";
                    case 5:
                        // Volume Down
                        return "VDOWN";
                    case 6:
                        // Mute
                        return "MUTE";
                }
            }
            return gesture;
        }
    }
}
