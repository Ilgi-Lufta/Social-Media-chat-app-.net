using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using TestFinal.Models;
using static TestFinal.ViewModel.ViewModels;

namespace TestFinal.HubConfig
{
    public class MessageHub : Hub
    {
        private MyContext _context;
        private readonly IMapper _mapper;

        public MessageHub(MyContext context, IMapper mapper) {
            _context = context;
            _mapper = mapper;
        }
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        public async Task SendMessage2(MessageDTO message, string connectionId)
        {
            var sender = _context.Users.FirstOrDefault(e => e.UserId == message.MessagesSenderId);
            if (sender != null)
            {
                var mappedSender = _mapper.Map<UserDTO>(sender);
                message.MessagesSender = mappedSender;
            }
            var reciver = _context.Users.FirstOrDefault(e => e.UserId == message.MessagesReciverId);
            if (reciver != null)
            {
                var mappedReciver = _mapper.Map<UserDTO>(sender);
                message.MessagesReciver = mappedReciver;
            }


            await Clients.Client(connectionId).SendAsync("ReceiveMessage2",message);
        }

        public async Task AddToGroup(string groupName)
        {

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            

            await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has joined the group {groupName}.");
        }
        public async Task MessageGroup(RoomMessageDTO msg)
        {

            //await Groups.AddToGroupAsync(Context.ConnectionId, groupName);


            await Clients.Group(msg.ChatRoomId.ToString()).SendAsync("ReciveGroupMessage", msg);
        }

        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has left the group {groupName}.");
        }

        public string GetConnectionId() => Context.ConnectionId;
    }
}

