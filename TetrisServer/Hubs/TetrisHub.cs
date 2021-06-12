using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.SignalR;

namespace TetrisServer.Hubs
{
    public static class UserHandler
    {
        public static HashSet<string> ConnectedIds = new();
        public static HashSet<string> ReadyIds = new();
    }

    public static class TimerHandler
    {
        private static Timer aTimer;
        private static int steps;
        public static void start()
        {
            // Create a timer and set a two second interval.
            aTimer = new Timer();
            aTimer.Interval = 450;
            aTimer.Elapsed += (_, _) =>
            {
                steps++;
                Debug.WriteLine(steps);
            };
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }
    }
    
    public class TetrisHub : Hub
    {
        
        public override Task OnConnectedAsync()
        {
            UserHandler.ConnectedIds.Add(Context.ConnectionId);
            return base.OnConnectedAsync();
        }
        

        public override Task OnDisconnectedAsync(Exception exception)
        {
            UserHandler.ConnectedIds.Remove(Context.ConnectionId);
            UserHandler.ReadyIds.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task ReadyUp()
        {
            UserHandler.ReadyIds.Add(Context.ConnectionId);
            await ServerState();
            if (UserHandler.ReadyIds.Count == 2)
            {
                await Clients.All.SendAsync("Run");
            }
        }
        
        public async Task TradeRandom(int random)
        {
            await Clients.Others.SendAsync("TradeRandom", random);
            TimerHandler.start();
        }
        public async Task ServerState()
        {
            await Clients.Caller.SendAsync("ServerState", UserHandler.ConnectedIds.Count, UserHandler.ReadyIds.Count);
        }
        
        public async Task KeyPress(char key)
        {
            await Clients.Others.SendAsync("KeyPress", key);
        }
        
        public async Task SendSerializedGrid(string grid)
        {
            await Clients.Others.SendAsync("SendSerializedGrid", grid);
        }
    }
}
