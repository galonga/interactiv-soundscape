using NAudio.Wave;
using System;
using System.Timers;

namespace WindowsFormsPadSoundScape
{
    class ASIOController
    {
        Mp3FileReader[] input;
        AsioOut asioPlayer;
        MultiplexingWaveProvider waveProvider;
        Timer timer1;
        static int[] activetracks = new int[] { 0, 0 };
        bool[] trackFinished;
        bool[] trackActive;

        public long maxLen;
        public long curPos;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsFormsPadSoundScape.ASIOController"/> class.
        /// </summary>
        public ASIOController()
        {
            input = new Mp3FileReader[] { new Mp3FileReader(@"C:\SoundScape\background.mp3"),
                new Mp3FileReader(@"C:\SoundScape\sound1.mp3"),
                new Mp3FileReader(@"C:\SoundScape\sound2.mp3"),
                new Mp3FileReader(@"C:\SoundScape\sound3.mp3"),
                new Mp3FileReader(@"C:\SoundScape\sound4.mp3"),
                new Mp3FileReader(@"C:\SoundScape\sound1_leise.mp3"),
                new Mp3FileReader(@"C:\SoundScape\sound2_leise.mp3"),
                new Mp3FileReader(@"C:\SoundScape\sound3_leise.mp3"),
                new Mp3FileReader(@"C:\SoundScape\sound4_leise.mp3") };
            trackFinished = new bool[] { true, true, true, true, true };
            trackActive = new bool[] { false, false, false, false, false };
            maxLen = input[0].Length; //Background Länge

        }


        /// <summary>
        /// Inits the ASIO audio player.
        /// </summary>
        public void initAudioPlayer()
        {
            timer1 = new Timer();
            this.timer1.Interval = 250;
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Tick);

            asioPlayer = new AsioOut();
            //waveProvider = new MultiplexingWaveProvider(new IWaveProvider[] { input1, input2, input3, input4, nosound }, 8);
            waveProvider = new MultiplexingWaveProvider(new IWaveProvider[] {input[0], //background / Defaultsound
                input[1], input[2], input[3], input[4], //loud Streams
                input[5], input[6], input[7], input[8]},  //silent Streams  
                8); //ASIO channels

            //Defaultsound
            ResetWaveProviderConnection();
            asioPlayer.Init(waveProvider);
        }

        /// <summary>
        /// Starts the ASIO Player
        /// </summary>
        private void Play()
        {
            // create device if necessary
            if (this.asioPlayer == null)
            {
                this.asioPlayer = new AsioOut();
                //this.asioPlayer.ChannelOffset = GetUserSpecifiedChannelOffset();
                this.asioPlayer.Init(waveProvider);
            }
            this.input[0].Position = 0; //Background
            this.input[1].Position = 0; //loudsounds..
            this.input[2].Position = 0;
            this.input[3].Position = 0;
            this.input[4].Position = 0;
            this.input[5].Position = 0; //silentsounds
            this.input[6].Position = 0;
            this.input[7].Position = 0;
            this.input[8].Position = 0;


            this.asioPlayer.Play();
            this.timer1.Enabled = true;
        }

        /// <summary>
        /// Resets the wave provider connection. --> no Sound is playing
        /// </summary>
        private void ResetWaveProviderConnection()
        {
            waveProvider.ConnectInputToOutput(0, 0);
            waveProvider.ConnectInputToOutput(0, 1);
            waveProvider.ConnectInputToOutput(0, 2);
            waveProvider.ConnectInputToOutput(0, 3);

            waveProvider.ConnectInputToOutput(1, 4);
            waveProvider.ConnectInputToOutput(2, 5);
            waveProvider.ConnectInputToOutput(3, 6);
            waveProvider.ConnectInputToOutput(4, 7);
        }


        /// <summary>
        /// Starts the track by id.
        /// </summary>
        /// <param name="id">Identifier.</param>
        internal void StartTrack(int id)
        {
            if (id >= 0 && id <= 4) {
                if (asioPlayer.PlaybackState == PlaybackState.Stopped) {
                    StartTrackPlayerStoped(id);
                    trackActive[id] = true;
                } else if (asioPlayer.PlaybackState == PlaybackState.Playing) {
                    CheckActiveTracks(id); //<=2 active tracks
                    RestartTrackPlayerActive(id);
                    trackActive[id] = true;
                }
            }
        }


        /// <summary>
        /// Stops the track by id.
        /// </summary>
        /// <param name="id">Identifier.</param>
        public void StopTrack(int id)
        {
            if (asioPlayer.PlaybackState == PlaybackState.Playing) {
                switch (id) {
                    case 1:
                        waveProvider.ConnectInputToOutput(0, 0);
                        break;
                    case 2:
                        waveProvider.ConnectInputToOutput(0, 1);
                        break;
                    case 3:
                        waveProvider.ConnectInputToOutput(0, 2);
                        break;
                    case 4:
                        waveProvider.ConnectInputToOutput(0, 3);
                        break;
                }
                trackActive[id] = false;
            }
        }

        /// <summary>
        /// Checks the active tracks and stops the oldest one.
        /// </summary>
        /// <param name="id">Identifier.</param>
        private void CheckActiveTracks(int id)
        {
            if (id != activetracks[1]) {// && id >= 1 && id <= 8)
                //stop oldest track
                StopTrack(activetracks[0]);
                //new oldest track
                activetracks[0] = activetracks[1];
                //level down
                LevelDownTrack(activetracks[0]);
                activetracks[1] = id;
            }
        }

        /// <summary>
        /// Tackaudiolevel down by id.
        /// </summary>
        /// <param name="id">Identifier.</param>
        private void LevelDownTrack(int id)
        {
            if (input[id].Length > input[id].Position) {
                input[id + 4].Position = input[id].Position;
                switch (id) {
                    case 1:
                        waveProvider.ConnectInputToOutput(1, 4);
                        waveProvider.ConnectInputToOutput(id + 4, 0);
                        break;
                    case 2:
                        waveProvider.ConnectInputToOutput(2, 5);
                        waveProvider.ConnectInputToOutput(id + 4, 1);
                        break;
                    case 3:
                        waveProvider.ConnectInputToOutput(3, 6);
                        waveProvider.ConnectInputToOutput(id + 4, 2);
                        break;
                    case 4:
                        waveProvider.ConnectInputToOutput(4, 7);
                        waveProvider.ConnectInputToOutput(id + 4, 3);
                        break;
                }
            }
        }

        /// <summary>
        /// Starts the track player when stoped by id.
        /// </summary>
        /// <param name="id">Identifier.</param>
        private void StartTrackPlayerStoped(int id)
        {
            if (asioPlayer.PlaybackState == PlaybackState.Stopped) {
                switch (id) {
                    case 1:
                        input[1].Position = 0;
                        waveProvider.ConnectInputToOutput(1, 0);
                        waveProvider.ConnectInputToOutput(0, 1);
                        waveProvider.ConnectInputToOutput(0, 2);
                        waveProvider.ConnectInputToOutput(0, 3);
                        break;
                    case 2:
                        input[2].Position = 0;
                        waveProvider.ConnectInputToOutput(0, 0);
                        waveProvider.ConnectInputToOutput(2, 1);
                        waveProvider.ConnectInputToOutput(0, 2);
                        waveProvider.ConnectInputToOutput(0, 3);
                        break;
                    case 3:
                        input[3].Position = 0;
                        waveProvider.ConnectInputToOutput(0, 0);
                        waveProvider.ConnectInputToOutput(0, 1);
                        waveProvider.ConnectInputToOutput(3, 2);
                        waveProvider.ConnectInputToOutput(0, 3);
                        break;
                    case 4:
                        input[4].Position = 0;
                        waveProvider.ConnectInputToOutput(0, 0);
                        waveProvider.ConnectInputToOutput(0, 1);
                        waveProvider.ConnectInputToOutput(0, 2);
                        waveProvider.ConnectInputToOutput(4, 3);
                        break;
                }
                try {
                    Play();
                }
                catch (Exception e) {
                    var foo = e.Message;
                }
            }
        }


        /// <summary>
        /// Restarts the track by id when player is active.
        /// </summary>
        /// <param name="id">Identifier.</param>
        private void RestartTrackPlayerActive(int id)
        {
            if (asioPlayer.PlaybackState == PlaybackState.Playing) {
                switch (id) {
                    case 1:
                        input[1].Position = 0;
                        waveProvider.ConnectInputToOutput(1, 0);
                        break;
                    case 2:
                        input[2].Position = 0;
                        waveProvider.ConnectInputToOutput(2, 1);
                        break;
                    case 3:
                        input[3].Position = 0;
                        waveProvider.ConnectInputToOutput(3, 2);
                        break;
                    case 4:
                        input[4].Position = 0;
                        waveProvider.ConnectInputToOutput(4, 3);
                        break;
                }
            }
        }


        /// <summary>
        /// Gets the track lenght.
        /// </summary>
        /// <returns>The track lenght.</returns>
        /// <param name="id">Identifier.</param>
        internal long GetTrackLenght(int id)
        {
            if (input != null) {
                return this.input[id].Length;
            } else {
                return 0;
            }
        }

        /// <summary>
        /// Gets the current track position.
        /// </summary>
        /// <returns>The current track position.</returns>
        /// <param name="id">Identifier.</param>
        internal long GetCurTrackPos(int id)
        {
            if (input != null && trackActive[id]) {
                if (this.input[id].Position < GetTrackLenght(id))
                    return this.input[id].Position;
                else
                    return GetTrackLenght(id);
            } else {
                return 0;
            }
        }


        /// <summary>
        /// Stop this ASIO Player playback.
        /// </summary>
        internal void Stop()
        {
            this.timer1.Enabled = false;
            trackActive[1] = false;
            trackActive[2] = false;
            trackActive[3] = false;
            trackActive[4] = false;
            ResetWaveProviderConnection();
            asioPlayer.Stop();
        }

        /// <summary>
        /// Cleanup this instance.
        /// </summary>
        public void Cleanup()
        {
            if (this.asioPlayer != null) {
                this.asioPlayer.Dispose();
                this.asioPlayer = null;
            }

            if (this.input[0] != null) {
                this.input[0].Dispose();
                this.input[0] = null;
            }

            if (this.input[1] != null) {
                this.input[1].Dispose();
                this.input[1] = null;
            }

            if (this.input[2] != null) {
                this.input[2].Dispose();
                this.input[2] = null;
            }

            if (this.input[3] != null) {
                this.input[3].Dispose();
                this.input[3] = null;
            }

            if (this.input[4] != null) {
                this.input[4].Dispose();
                this.input[4] = null;
            }

            if (this.input[5] != null) {
                this.input[5].Dispose();
                this.input[5] = null;
            }

            if (this.input[6] != null) {
                this.input[6].Dispose();
                this.input[6] = null;
            }

            if (this.input[7] != null) {
                this.input[7].Dispose();
                this.input[7] = null;
            }

            if (this.input[8] != null) {
                this.input[8].Dispose();
                this.input[8] = null;
            }
        }


        /// <summary>
		/// Check the tracklenght. Reset if its necessary (loop).
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (asioPlayer != null && asioPlayer.PlaybackState == PlaybackState.Playing) {
                curPos = input[0].Length;

                if (input[1].Position >= input[1].Length && input[2].Position >= input[2].Length && input[3].Position >= input[3].Length && input[4].Position >= input[4].Length) {
                    Stop();
                    StartTrackPlayerStoped(0);
                } else if (input[0].Position >= input[0].Length) {
                    input[0].Position = 0;
                }
            }
        }
    }
}
