using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace TetrisServer.Hubs
{
    public static class UserHandler
    {
        public static HashSet<string> ConnectedIds = new HashSet<string>();
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
            return base.OnDisconnectedAsync(exception);
        }

        public async Task ServerState()
        {
            await Clients.Caller.SendAsync("ServerState", UserHandler.ConnectedIds.Count);
        }
        
        public async Task DropShape()
        {
            await Clients.Others.SendAsync("DropShape");
        }

        public async Task RotateShape(string direction)
        {
            await Clients.Others.SendAsync("RotateShape", direction);
        }

        public async Task MoveShape(string moveDirection)
        {
            await Clients.Others.SendAsync("MoveShape", moveDirection);
        }
        
        public async Task ReadyUp(int seed)
        {
            await Clients.Others.SendAsync("ReadyUp", seed);
        }
    }
}
