using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Win32.SafeHandles;

namespace TetrisApp
{
    public class TetrisClient
    {
        public static ClientForm clientForm;
        public static Label ServerLabel;
        public System.Timers.Timer aTimer;
        public HubConnection Connection;
        public RadioButton mpButton;
        public Button readyButton;
        public TetrisEngine currentEngine;

        public Random ownRandom;
        public Random otherRandom;
        
        public TetrisClient(ClientForm form)
        {
            //Bind form and label
            clientForm = form;
            ServerLabel = clientForm.getServerLabel();
            mpButton = clientForm.getRadioButton();
            readyButton = clientForm.getReadyButton();

            //mpButton
            mpButton.CheckedChanged += (sender, args) => switchMultiplayer((RadioButton)sender);
            
            //ready button
            readyButton.Click += (sender, args) => readyUp();
            
            //Set server label
            ServerLabel.Text = "State: Disconnected";
            
        }

        //Switches between multiplayer and local play
        public void switchMultiplayer(RadioButton sender)
        {
            
            if (!sender.Checked)
            {
                //Start timer
                pingTimer();
                startClient();
            }
            else
            {
                stopConnection();
            }
        }

        public void disconnectClient()
        {
            string caption = "Disconnected. We're sorry :(";
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            stopConnection();
            currentEngine.tetrisForm.Hide();
            currentEngine.stopTimer();
            currentEngine = null;
            clientForm.Show();
            MessageBox.Show("", caption, buttons);
        }

        //Player is getting ready to play
        public void readyUp()
        {
            //Local Play
            if (mpButton.Checked)
            {
                onTetrisRun(new Random(), new Random());
            }
            else
            {
                if (Connection == null) return;
                //Ready Up
                if (Connection.State == HubConnectionState.Connected)
                {
                    Connection.InvokeAsync("ReadyUp");
                }
            }
        }

        //Start the HubConnection Client
        public async void startClient()
        {
            //Create connection
            Connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/tetrisHub")
                .Build();
            
            //On close -> restart
            Connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0,5) * 1000);
                await Connection.StartAsync();
            };

            //Get state package
            Connection.On<Int32, Int32>("ServerState", (playerAmt, plrsReady) =>
            {
                ServerLabel.Text = @"State: " + playerAmt + @" Player(s) Connected. " + plrsReady + @" Ready";
            });
            
            //On Run Signal
            Connection.On("Run", () =>
            {
                int i = 1;// Guid.NewGuid().GetHashCode();
                ownRandom = new Random(i);
                Debug.WriteLine(ownRandom.GetHashCode());
                Connection.InvokeAsync("TradeRandom", i);
            });
            
            //On Trade Signal
            Connection.On("TradeRandom", (int random) =>
            {
                otherRandom = new Random(random);
                Debug.WriteLine(otherRandom.GetHashCode());
                onTetrisRun( ownRandom, otherRandom);
            });
            
            //On position signal
            Connection.On<char>("KeyPress", (key) =>
            {
                currentEngine.otherPlayerKeyPress(key);
            });
            
            //On grid signal
            Connection.On<string>("SendSerializedGrid", grid =>
            {
                currentEngine.otherPlayerSetGrid(grid);
            });
            
            //Attempt start
            await Connection.StartAsync();
        }
        
        /*
         * Game Events
         */

        //KeyPress events
        public void sendKeyPress(char eKeyChar)
        {
            Connection.InvokeAsync("KeyPress", eKeyChar);
        }
        
        //Send Serialized Game
        public void SendSerializedGrid(string grid)
        {
            Connection.InvokeAsync("SendSerializedGrid", grid);
        }

        /*
         * 
         */
        
        //Stop the connection
        public async void stopConnection()
        {
            try
            {
                ServerLabel.Text = "State: 'Stopped";
                await Connection.StopAsync();
                await Connection.DisposeAsync();
                aTimer.Enabled = false;
            }
            catch (Exception ignored)
            {
                // ignored
            }
        }

        //Hide form and start tetris
        public void onTetrisRun(Random ownRandom, Random otherRandom)
        {
            clientForm.Hide();
            var form = new TetrisForm();
            TetrisClient attachedClient = (!mpButton.Checked) ? this : null;
            currentEngine = new TetrisEngine(form, !mpButton.Checked, attachedClient, ownRandom, otherRandom);
        }
        
        //Ping timer elapsed
        public void pingTimer()
        {
            aTimer = new System.Timers.Timer {Interval = 400};
            aTimer.Elapsed += pingServer;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        //Ping the server
        private void pingServer(object sender, ElapsedEventArgs e)
        {
            Connection.InvokeAsync("ServerState");

            if (Connection == null)
            {
                ServerLabel.Text = "State: Failed";
                disconnectClient();
                return;
            }
            
            if (Connection.State == HubConnectionState.Disconnected)
            {
                ServerLabel.Text = "State: Stopped";
                disconnectClient();
            }
            
            if (Connection.State == HubConnectionState.Connected)
            {
                ServerLabel.Text = "State: Connected";
            }
            else
            {
                ServerLabel.Text = "State: Attempting connection";
            }
        }

    }
}