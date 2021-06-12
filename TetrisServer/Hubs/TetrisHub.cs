using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace TetrisServer.Hubs
{
    public static class UserHandler
    {
        public static HashSet<string> ConnectedIds = new HashSet<string>();
        public static HashSet<string> ReadyIds = new HashSet<string>();
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
        
        public async Task SyncRandom(int rand)
        {
            await Clients.Others.SendAsync("SyncRandom", rand);
        }
    }
}
