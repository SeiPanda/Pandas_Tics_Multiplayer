using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Hubs
{
    public class TicTacToeHub : Hub
    {
        public static List<Player> ConnectedPlayers = new List<Player>();
        public static List<Field> Fields = new List<Field>(); 
        
        public override async Task OnConnectedAsync()
        {
            var username = Context.GetHttpContext()!.Request.Query["username"]!;
            var connectionId = Context.ConnectionId;
            
            var player= new Player
            {
                Name = username!,
                Id = connectionId
            };

            ConnectedPlayers.Add(player);

            if (ConnectedPlayers.Count == 1)
            {
                player.Sign = "x";
                player.CurrentTurn = true;
            }
            else
            {
                player.Sign = "o";
                player.CurrentTurn = false;
            }

            if (ConnectedPlayers.Count <= 2)
            {
                await base.OnConnectedAsync();
            }

            if (ConnectedPlayers.Count == 2)
            {
                for (int i = 0; i < 9; i++)
                {
                    var field = new Field();
                    field.Id = i;
                    field.Sign = "";
                    field.Win = false;

                    Fields.Add(field);
                }
                await Clients.All.SendAsync("AllPlayerReady", ConnectedPlayers);
            }
        }

        public async Task UpdateField(int fieldIndex)
        {
            if (fieldIndex > 8 || fieldIndex < 0)
            {
                return;
            }
            
            var id = Context.ConnectionId;
            var field = Fields[fieldIndex];
            field.Sign = ConnectedPlayers.Find(x => x.Id == id)!.Sign;
            
            await Clients.All.SendAsync("ReciveFieldIdToUpdate", Fields);
            
            CheckFieldsForWin();
        }

        public void CheckFieldsForWin()
        {
            var clickedFields = Fields.FindAll(field => field.Clicked);
            var isWinner = false;
            int[] winningBoxes = new int[3];
            List<int> fieldValues = new List<int>();

            if (clickedFields.Count >= 3)
            {
                foreach (var field in Fields)
                {
                    var sign = -1;
                    if(field.Sign == ConnectedPlayers[0].Sign){
                        sign = 0;
                    }

                    if(field.Sign == ConnectedPlayers[1].Sign){
                        sign = 1;
                    }
                    fieldValues.Add(sign);
                }
                // TODO directly set .Win on field
                if( fieldValues[0] != -1 && fieldValues[0] == fieldValues[1] && fieldValues[1] == fieldValues[2] ) {
                    isWinner = true;
                    winningBoxes[0] = 0;
                    winningBoxes[1] = 1;
                    winningBoxes[2] = 2;
                }

                if( fieldValues[0] != -1 && fieldValues[0] == fieldValues[3] && fieldValues[3] == fieldValues[6] ) {
                    isWinner = true;
                    winningBoxes[0] = 0;
                    winningBoxes[1] = 3;
                    winningBoxes[2] = 6;
                }

                if( fieldValues[0] != -1 && fieldValues[0] == fieldValues[4] && fieldValues[4] == fieldValues[8] ) {
                    isWinner = true;
                    winningBoxes[0] = 0;
                    winningBoxes[1] = 4;
                    winningBoxes[2] = 8;
                }

                if( fieldValues[1] != -1 && fieldValues[1] == fieldValues[4] && fieldValues[4] == fieldValues[7] ) {
                    isWinner = true;
                    winningBoxes[0] = 1;
                    winningBoxes[1] = 4;
                    winningBoxes[2] = 7;
                }

                if( fieldValues[2] != -1 && fieldValues[2] == fieldValues[4] && fieldValues[4] == fieldValues[6] ) {
                    isWinner = true;
                    winningBoxes[0] = 4;
                    winningBoxes[1] = 6;
                    winningBoxes[2] = 2;
                }

                if( fieldValues[3] != -1 && fieldValues[3] == fieldValues[4] && fieldValues[4] == fieldValues[5] ) {
                    isWinner = true;
                    winningBoxes[0] = 3;
                    winningBoxes[1] = 4;
                    winningBoxes[2] = 5;
                }
                if( fieldValues[2] != -1 && fieldValues[2] == fieldValues[5] && fieldValues[5] == fieldValues[8] ) {
                    isWinner = true;
                    winningBoxes[0] = 6;
                    winningBoxes[1] = 5;
                    winningBoxes[2] = 2;
                }
                if( fieldValues[6] != -1 && fieldValues[6] == fieldValues[7] && fieldValues[7] == fieldValues[8] ) {
                    isWinner = true;
                    winningBoxes[0] = 6;
                    winningBoxes[1] = 7;
                    winningBoxes[2] = 8;
                }
            }
            
            if(isWinner)
            {
                ShowWinner(winningBoxes);
            }else {

                if (clickedFields.Count == 9)
                {
                    Draw();
                }
                UpdatePlayers();
            }
        }
        public async Task ShowWinner(int[] winningBoxes)
        {
            foreach (var x in winningBoxes)
            {
                Fields[x].Win = true;
            }
            var winner = ConnectedPlayers.Find(x => x.Id == Context.ConnectionId);
           winner.Wins++;

           await Clients.All.SendAsync("ReciveWinner", Fields, ConnectedPlayers);
           
           ResetFields();
         
        }
        public async Task Draw()
        {
            await Clients.All.SendAsync("DrawGame");
            ResetFields();
        }
        public async Task UpdatePlayers()
        {
            foreach (var player in ConnectedPlayers)
            {
                player.CurrentTurn = !player.CurrentTurn;
            }
            await Clients.All.SendAsync("ReciveUpdatedPlayers", ConnectedPlayers);
        }

        public void ResetFields()
        {
            foreach (var field in Fields)
            {
                field.Sign = "";
                field.Win = false;
            }
        }
        
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
            ConnectedPlayers.Clear();
            Fields.Clear();
        }
    }
}