using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Microsoft.AspNetCore.SignalR.Client;

namespace TetrisApp
{
    public class TetrisClient
    {
        public static ClientForm clientForm;
        public static Label ServerLabel;
        public System.Timers.Timer aTimer;
        public HubConnection Connection;
        public RadioButton mpButton;
        public TetrisClient(ClientForm form)
        {
            Console.WriteLine("Bound and started");
            
            //Bind form and label
            clientForm = form;
            ServerLabel = clientForm.getServerLabel();
            mpButton = clientForm.getRadioButton();

            mpButton.CheckedChanged += (sender, args) =>
            {
                RadioButton radioButton = (RadioButton) sender;
                if (!radioButton.Checked)
                {
                    //Start timer
                    pingTimer();
                    startClient();
                }
                else
                {
                    stopConnection();
                }
            };
            
            ServerLabel.Text = "State: Disconnected";
            
        }

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

            Connection.On<Int32>("ServerState", s =>
            {
                ServerLabel.Text = @"State: " + s + @" Player(s) Connected. Waiting...";
            });
            
            //Attempt start
            await Connection.StartAsync();
        }

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

        public void onTetrisRun()
        {
            var form = new TetrisForm();
            new TetrisEngine(form);
            Application.Run(form);
        }
        
        public void pingTimer()
        {
            aTimer = new System.Timers.Timer {Interval = 400};
            aTimer.Elapsed += pingServer;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void pingServer(object sender, ElapsedEventArgs e)
        {
            Connection.InvokeAsync("ServerState");
            
            if (Connection.State == HubConnectionState.Disconnected)
            {
                ServerLabel.Text = "State: Stopped";
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