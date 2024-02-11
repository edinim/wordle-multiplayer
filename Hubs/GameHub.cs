using Microsoft.AspNetCore.SignalR;
using Models;
using Models.Enums;

namespace Hubs
{
    public class GameHub : Hub
    {

        public async Task JoinRoom(string room, Player player) {
            await Groups.AddToGroupAsync(Context.ConnectionId, room);
            await NewPlayer(room, player);
        }

        public async Task LeaveRoom(string room) {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, room);
        }

        public async Task ChatProgress(string room, string player, List<CellState> cellStates){
            await Clients.Group(room).SendAsync("ChatProgress", room, player, cellStates);
        }

        public async Task NewPlayer(string room, Player player){
            await Clients.Group(room).SendAsync("NewPlayer", room, player);
        }

        public async Task GameState(string room, GameState gameState){
            await Clients.Group(room).SendAsync("GameState", room, gameState);
        }

        public async Task RoundFinished(string room, Round round){
            await Clients.Group(room).SendAsync("RoundFinished", room, round);
        }

    }
}