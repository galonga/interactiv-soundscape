using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using Microsoft.Owin.Hosting;
using System;
using System.Net.Http;
using System.Timers;
using System.Net.Http.Formatting;
using System.Windows.Forms;
using SlimDX.DirectInput;
using GameControllerState = SlimDX.DirectInput.JoystickState;
using System.Threading;

// <copyright file="Form1.cs" company="Timo Arndt">
// Copyright (c) 2015 All Rights Reserved
// </copyright>
// <author>Timo Arndt</author>
// <date>06/30/2015 11:39:58 AM </date>
// <summary>MainClass</summary>


namespace WindowsFormsPadSoundScape
{
    public partial class SoundScapeMain : Form
    {
        private ASIOController audioController;
        string baseUrl = "http://localhost:1337";
        DirectInput directInput;
        GamePadController controller;
        bool[] cubeActive;
        private bool systemActive;
        private System.Timers.Timer timer1;
        Thread backgroundThread;

        public SoundScapeMain()
        {
            InitializeComponent();
            this.FormClosing += Form1_Closing;
            audioController = new ASIOController();
            audioController.initAudioPlayer();
            directInput = new DirectInput();
            controller = new GamePadController(directInput, 0);

            cubeActive = new bool[] { false, false, false, false,false };
            //WebApp.Start<Startup>(baseUrl);
            label1.Text = "Controle Device: ";
            label2.Text = controller.GetControllerName();


            progressBar1.Maximum = (int)audioController.GetTrackLenght(1);
            progressBar1.Step = 1;
            progressBar2.Maximum = (int)audioController.GetTrackLenght(2);
            progressBar2.Step = 1;
            progressBar3.Maximum = (int)audioController.GetTrackLenght(3);
            progressBar3.Step = 1;
            progressBar4.Maximum = (int)audioController.GetTrackLenght(4);
            progressBar4.Step = 1;

            timer1 = new System.Timers.Timer();

            timer1.Interval = 500;
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Tick);
            timer1.Enabled = true;
            systemActive = true;
            backgroundThread = new Thread(
                new ThreadStart(() =>
                {
                    //Upadte Progressbars
                    while (systemActive)
                    {
                        Thread.Sleep(1000);
                        progressBar1.BeginInvoke(
                            new Action(() =>
                                {
                                    progressBar1.Value = (int)audioController.GetCurTrackPos(1);
                                }
                        ));
                        progressBar2.BeginInvoke(
                            new Action(() =>
                            {
                                progressBar2.Value = (int)audioController.GetCurTrackPos(2);
                            }
                        ));
                        progressBar3.BeginInvoke(
                            new Action(() =>
                            {
                                progressBar3.Value = (int)audioController.GetCurTrackPos(3);
                            }
                        ));
                        progressBar4.BeginInvoke(
                            new Action(() =>
                            {
                                progressBar4.Value = (int)audioController.GetCurTrackPos(4);
                            }
                        ));
                    }
                }
                ));
            backgroundThread.Start();

        }




        private void Form1_Load(object sender, EventArgs e)
        {

        }


        /// <summary>
        /// Cleanup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

             /*
            if (MessageBox.Show("Do you want to exit?", "SoundScape",
               MessageBoxButtons.YesNo) == DialogResult.Yes)
            {*/
                systemActive = false;
                backgroundThread.Abort();
                this.timer1.Enabled = false;
                audioController.Stop();
                audioController.Cleanup();
                controller.Release();

               /* 
            }
            else { e.Cancel = true; }*/
            
        }


        

        private void timer1_Tick(object sender, ElapsedEventArgs e)
        {
            if (controller.joystickAvable) { 
                GameControllerState state = controller.GetState();

                if (!state.IsPressed(4) && !state.IsPressed(5) && !state.IsPressed(6) && !state.IsPressed(7))
                { //wenn kein Würfel auf Schale Stop!
                    audioController.Stop();
                }
                else
                {
                    //Würfel 1
                    if (!state.IsPressed(4))
                    {

                        btnStart1.BackColor = Color.Green;
                        if (!cubeActive[1])
                        {
                            cubeActive[1] = true;
                            audioController.StartTrack(1);
                        }
                    }
                    else
                    {
                        btnStart1.BackColor = Color.LightGray;
                        audioController.StopTrack(1);
                        cubeActive[1] = false;
                    }
                    //Würfel 2
                    if (!state.IsPressed(5))
                    {
                        btnStart2.BackColor = Color.Green;
                        if (!cubeActive[2])
                        {
                            cubeActive[2] = true;
                            audioController.StartTrack(2);
                        }
                    }
                    else
                    {
                        btnStart2.BackColor = Color.LightGray;
                        audioController.StopTrack(2);
                        cubeActive[2] = false;
                    }
                    //Würfel 3
                    if (!state.IsPressed(6))
                    {
                        btnStart3.BackColor = Color.Green;
                        if (!cubeActive[3])
                        {
                            cubeActive[3] = true;
                            audioController.StartTrack(3);
                        }
                    }
                    else
                    {
                        btnStart3.BackColor = Color.LightGray;
                        audioController.StopTrack(3);
                        cubeActive[3] = false;
                    }
                    //Würfel 4
                    if (!state.IsPressed(7))
                    {
                        btnStart4.BackColor = Color.Green;
                        if (!cubeActive[4])
                        {
                            cubeActive[4] = true;
                            audioController.StartTrack(4);
                        }
                    }
                    else
                    {
                        btnStart4.BackColor = Color.LightGray;
                        audioController.StopTrack(4);
                        cubeActive[4] = false;
                    }
                }
            }
        }

        private void btnStart1_Click(object sender, System.EventArgs e)
        {
            audioController.StartTrack(1);
        }

        private void btnStart2_Click(object sender, System.EventArgs e)
        {
            audioController.StartTrack(2);
        }

        private void btnStart3_Click(object sender, System.EventArgs e)
        {
            audioController.StartTrack(3);
        }

        private void btnStart4_Click(object sender, System.EventArgs e)
        {
            audioController.StartTrack(4);
        }

        private void btnStop_Click(object sender, System.EventArgs e)
        {
            audioController.Stop();
        }

        private void btnInitAudio_Click(object sender, System.EventArgs e)
        {
            audioController.initAudioPlayer();
        }

        private void btnAbout_Click(object sender, System.EventArgs e)
        {
            MessageBox.Show("(c) www.visit-timo.de");
        }
    }
}
