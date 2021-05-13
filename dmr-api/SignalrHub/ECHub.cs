using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMR_API._Repositories.Interface;
using DMR_API._Services.Interface;
using DMR_API.Helpers;
using DMR_API.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DMR_API.SignalrHub
{
    public class ECHub : Hub
    {
        private readonly static ConnectionMapping<UserConnection> _connections =
       new ConnectionMapping<UserConnection>();

        private readonly IToDoListService _todoService;
        private readonly IMailingService _mailingService;
        private readonly IStirRawDataService _stirRawDataService;
        private readonly IStirRawDataRepository _stirRawDataRepository;
        private readonly IMailExtension _emailService;
        public ECHub(
            IToDoListService todoService,
            IMailingService mailingService,
            IStirRawDataService stirRawDataService,
            IStirRawDataRepository stirRawDataRepository,
            IMailExtension emailService

            )
        {
            _todoService = todoService;
            _mailingService = mailingService;
            _stirRawDataService = stirRawDataService;
            _stirRawDataRepository = stirRawDataRepository;
            _emailService = emailService;
        }
        public async Task Message(string data)
        {
            StirRawData obj = JsonConvert.DeserializeObject<StirRawData>(data);
            await _stirRawDataService.Add(obj);
            //try
            //{
            //    var rawData = await _stirRawDataRepository.FindAll(x => x.Building.Equals(obj.Building) && x.MachineID == obj.MachineID)
            //         .Select(x => x.Sequence)
            //         .Distinct().OrderByDescending(x => x).FirstOrDefaultAsync();
            //    sequence = rawData;
            //    if (rawData == 0)
            //    {
            //        obj.Sequence = 1;
            //        await _stirRawDataService.Add(obj);
            //    } else
            //    {
            //        obj.Sequence = rawData + 1;
            //        await _stirRawDataService.Add(obj);
            //    }

            //}
            //catch
            //{
            //    throw ;
            //}
        }

        public async Task JoinHub(int machineID)
        {
            await Task.CompletedTask;
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        public async Task Welcom(string scalingMachineID, string message, string unit)
        {
            await Clients.All.SendAsync("Welcom", scalingMachineID, message, unit);
        }
        public async Task WeighingScale(string scalingMachineID, string message, string unit, string building)
        {
            var groupName = building;
            await Clients.Group(groupName).SendAsync("ReceiveAmountWeighingScale", scalingMachineID, message, unit, building);
        }

        public async Task JoinGroup(string building)
        {
            var groupName = building;
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var id = Context.ConnectionId;
            Console.WriteLine($"Client ID: {id} joined hub name {building}");
        }
        public async Task SendMail(string scalingMachineID)
        {

            var file = await _todoService.ExportExcelToDoListWholeBuilding();
            var subject = "Mixing Room Report";
            var fileName = $"mixingRoomReport{DateTime.Now.ToString("MMddyyyy")}.xlsx";
            var message = "Please refer to the Mixing Room Report";
            var mailList = new List<string>
            {
                //"mel.kuo@shc.ssbshoes.com",
                //"maithoa.tran@shc.ssbshoes.com",
                //"andy.wu@shc.ssbshoes.com",
                //"sin.chen@shc.ssbshoes.com",
                //"leo.doan@shc.ssbshoes.com",
                //"heidy.amos@shc.ssbshoes.com",
                //"bonding.team@shc.ssbshoes.com",
                //"Ian.Ho@shc.ssbshoes.com",
                //"swook.lu@shc.ssbshoes.com",
                //"damaris.li@shc.ssbshoes.com",
                //"peter.tran@shc.ssbshoes.com"
            };
            if (file != null || file.Length > 0)
            {
                await _emailService.SendEmailWithAttactExcelFileAsync(mailList, subject, message, fileName, file);
            }
        }
        public async Task AskMailing()
        {
            var mailingList = await _mailingService.GetAllAsync();
            await Clients.Group("Mailing").SendAsync("ReceiveMailing", mailingList);
        }
        public async Task Mailing()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Mailing");
            var mailingList = await _mailingService.GetAllAsync();
            await Clients.Group("Mailing").SendAsync("ReceiveMailing", mailingList);
        }

        public async Task CheckOnline(int userID, string username)
        {
            var keyBase = new UserConnection { ID = userID, UserName = username };
            var connectionBaseID = _connections.FindConnection(keyBase);
            if (connectionBaseID == null)
            {
                _connections.Add(keyBase, Context.ConnectionId);
                await Groups.AddToGroupAsync(Context.ConnectionId, "Online");

                var entries = _connections.GetKey().Select(x => x.UserName).Distinct().ToList();
                var usernames = string.Join(",", entries);
                await Clients.Group("Online").SendAsync("Online", entries.Count);
                await Clients.Group("Online").SendAsync("UserOnline", usernames);
            }
            else
            {
                _connections.Add(keyBase, Context.ConnectionId);
                await Groups.AddToGroupAsync(Context.ConnectionId, "Online");

                var entries = _connections.GetKey().Select(x => x.UserName).Distinct().ToList();
                var usernames = string.Join(",", entries);
                await Clients.Group("Online").SendAsync("Online", entries.Count);
                await Clients.Group("Online").SendAsync("UserOnline", usernames);
            }
        }

        public async Task JoinReloadDispatch()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "ReloadDispatch");
        }
        public async Task ReloadDispatch()
        {
            await Clients.Group("ReloadDispatch").SendAsync("ReloadDispatch");
        }
        public async Task JoinReloadTodo()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "ReloadTodo");
        }
        public async Task ReloadTodo()
        {
            await Clients.Group("ReloadTodo").SendAsync("ReloadTodo");
        }
        public async Task Todolist(int buildingID)
        {
            await Clients.All.SendAsync("ReceiveTodolist", buildingID);
        }
        public async Task CreatePlan()
        {
            await Clients.All.SendAsync("ReceiveCreatePlan");
        }
        public override async Task OnConnectedAsync()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Client OnConnectedAsync: {Context.ConnectionId}");
            Console.ResetColor();
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var keyBase = _connections.FindKeyByValue2(Context.ConnectionId);

            if (keyBase != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
                Console.ResetColor();
                _connections.RemoveKeyAndValue(keyBase, Context.ConnectionId);

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Online");
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SignalR Users");
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Mailing");
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "ReloadDispatch");
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "ReloadTodo");


                var entries = _connections.GetKey().Select(x => x.UserName).Distinct().ToList();
                var usernames = string.Join(",", entries);
                await Clients.Group("Online").SendAsync("Online", entries.Count);
                await Clients.Group("Online").SendAsync("UserOnline", usernames);
            }
            await base.OnDisconnectedAsync(exception);
        }

        //return list of all active connections
        public List<string> GetAllActiveConnections()
        {
            return new List<string>();
        }
        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has joined the group {groupName}.");
        }

        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has left the group {groupName}.");
        }
    }
}