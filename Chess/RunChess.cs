﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chess
{
    class RunChess
    {
        private static ManagedObject RNN_Chess;
        private static int[] ChessPieces_Count = { 2, 2, 2, 1, 1, 8 };

        public static void RunRNN()
        {
            int offset = 0, size = 0, epochs = 0;

            Console.Write("\tOffset: ");
            string input = Console.ReadLine();
            offset = int.Parse(input);
            Console.Write("\tTraining Samples: ");
            input = Console.ReadLine();
            size = int.Parse(input);
            Console.Write("\tEpochs: ");
            input = Console.ReadLine();
            epochs = int.Parse(input);


            if(RNN_Chess == null)
            {
                //DataBase.ConvertdefaultInput();
                DataBase.GetWorkspaceSize(offset, size);
                WeightManager.WeightReader();
                RNN_Chess = new ManagedObject(Program.Dimensions);
                RNN_Chess.InitializeVariables(Variables.InputWeights, Variables.HiddenWeights, Variables.Biases);
                RNN_Chess.InitializeConstants(-0.005f);
            }

            for(int o = 0; o < epochs; o++)
            {
                Console.WriteLine("Epoch: " + o);

                for(int i = 0; i < size; i++)
                {
                    DataBase.ConvertChessNotation(offset + i);
                    RNN_Chess.UpdateDimensions(Program.Dimensions);

                    for(int j = 0; j < Program.Dimensions[0]; j++)
                    {
                        RNN_Chess.RunRNN(Variables.InputState[j], Program.Dimensions[1]);
                    }

                    for(int j = 0; j < Program.Dimensions[0]; j++)
                    {
                        RNN_Chess.BackPropagation(Variables.winningColor);
                    }
                }
                RNN_Chess.UpdateWeightMatrices(Variables.InputWeights, Variables.HiddenWeights, Variables.Biases);
            }

            WeightManager.WeightWriter();
            RNN_Chess.FreeWorkSpace();
        }

        public static void GetInputState(string folderpath, int input)
        {
            Bitmap bmp;
            Graphics gr;

            int[] Available_ChessPieces = new int[12];
            string[,] cat = new string[8, 8];
            byte[] buffer = new byte[64];
            int dx = 0;
            int dy = 0;

            if(input == 0){
                bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                gr = Graphics.FromImage(bmp);
                gr.CopyFromScreen(0, 0, 0, 0, bmp.Size);
            }
            else{
                bmp = new Bitmap(Program.directory + @"TempBitmap.PNG");
                gr = Graphics.FromImage(bmp);
            }

            for(int i = 0; i < 8; i++)
            {
                if(i > 2)
                {
                    dx = 1;
                    if(i > 6)
                    {
                        dx = 2;
                    }
                }
                else
                {
                    dx = 0;
                }

                for(int j = 0; j < 8; j++)
                {
                    if(j > 2)
                    {
                        dy = 1;
                        if(j > 6)
                        {
                            dy = 2;
                        }
                    }
                    else
                    {
                        dy = 0;
                    }

                    cat[i, j] = ChessPieces.CompareChessPiece(598 + 91 * i - dx, 179 + 91 * j - dy, bmp, folderpath);

                    if(cat[i, j].Contains(","))
                    {
                        int color = int.Parse(cat[i, j].Split(',')[0]);
                        int category = int.Parse(cat[i, j].Split(',')[1]);
                        int pieces = 0;

                        for(int k = 0; k < category; k++)
                        {
                            pieces += ChessPieces_Count[k];
                        }
                        pieces = (pieces + Available_ChessPieces[color * 6 + category]) * 2;

                        buffer[color * 32 + pieces] = byte.Parse(j.ToString());
                        buffer[color * 32 + pieces + 1] = byte.Parse(i.ToString());

                        Available_ChessPieces[color * 6 + category]++;
                    }
                }
            }

            File.WriteAllBytes(Program.directory + @"Chess Weights\InputState.txt", buffer);
        }
    }
}