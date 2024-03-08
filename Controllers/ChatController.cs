using AutoMapper;
using AutoMapper.Execution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TestFinal.Models;
using static TestFinal.ViewModel.ViewModels;
namespace TestFinal.Controllers
{
    public class ChatController : Controller
    {
        private MyContext _context;
        private readonly IMapper _mapper;

        public ChatController(MyContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;

        }

        [HttpPost("CreateChat")]
        public IActionResult CreateChat([FromBody] ChatRoomEditViewModel chat)
        {
            var ilgi = _context.UserChatRooms
                 .GroupBy(e => e.ChatRoomId)
                 .Select(b => new
                 {
                     ChatRoomId = b.Key,
                     ChatmemberIds = b.Select(c => c.ChatMemberId),
                 }).ToList();

            var chatexists = false;
            var exstingChatRoomId = 0;
            foreach (var member in ilgi)
            {

                var firstNotSecond = member.ChatmemberIds.Except(chat.MembersId);
                var secondNotFirst = chat.MembersId.Except(member.ChatmemberIds);

                chatexists = !firstNotSecond.Any() && !secondNotFirst.Any();

                if (chatexists)
                {
                    exstingChatRoomId = member.ChatRoomId;
                    break;
                }
            };
            if (exstingChatRoomId == 0)
            {
                ChatRoom chatRoom = new ChatRoom()
                {
                    IsGroupChat = false
                };
                _context.ChatRooms.Add(chatRoom);
                _context.SaveChanges();

                foreach (int id in chat.MembersId)
                {
                    UserChatRoom userChatRoom = new UserChatRoom()
                    {
                        ChatMemberId = id,
                        ChatRoomId = chatRoom.ChatRoomId,
                    };
                    _context.UserChatRooms.Add(userChatRoom);
                    _context.SaveChanges();
                }
                var mapped = _mapper.Map<ChatRoomDTO>(chatRoom);
                return Ok(mapped);
            }
            else
            {
                var chatRoom = _context.ChatRooms.FirstOrDefault(e => e.ChatRoomId == exstingChatRoomId);
                var mapped = _mapper.Map<ChatRoomDTO>(chatRoom);
                return Ok(mapped);
            }


        }
        [HttpGet("GetChats/{userId}")]
        public IActionResult GetChats(int userId)
        {
            var chatRooms = _context.ChatRooms.Include(e => e.UserChatRooms).Where(e => e.UserChatRooms.Any(b => b.ChatMemberId == userId)).ToList();
            var mapped = _mapper.Map<List<ChatRoomDTO>>(chatRooms);
            return Ok(mapped);
        }
        [HttpGet("GetChat/{ChatId}")]
        public IActionResult GetChat(int ChatId)
        {

            var chatRoom = _context.ChatRooms.FirstOrDefault(b => b.ChatRoomId == ChatId);
            var mapped = _mapper.Map<ChatRoomDTO>(chatRoom);
            return Ok(mapped);
        }
        [HttpGet("GetRoomMesage/{ChatId}")]
        public IActionResult GetRoomMesage(int ChatId)
        {

            var roomMessage = _context.RoomMessages.Include(m=>m.ChatRoom).Include(m => m.MessagesSender)
                .Where(b => b.ChatRoomId == ChatId).OrderBy(e=>e.Date).ToList();
            var mapped = _mapper.Map<List<RoomMessageDTO>>(roomMessage);
            return Ok(mapped);
        }
        [HttpPost("SendRoomMesage")]
        public IActionResult SendRoomMesage([FromBody] RoomMessageEditViewModel RoomMessageEditViewModel)
        {
            RoomMessage roomMessage = new RoomMessage()
            {
                MessagesSenderId = RoomMessageEditViewModel.MessagesSenderId,
                ChatRoomId = RoomMessageEditViewModel.ChatRoomId,
                Message = RoomMessageEditViewModel.Message,
                Date= DateTime.Now,
            };
            _context.RoomMessages.Add(roomMessage);
            _context.SaveChanges();

                
            var user = _mapper.Map<UserDTO>(_context.Users.FirstOrDefault(e=>e.UserId == roomMessage.MessagesSenderId));
           
            var mapped = _mapper.Map<RoomMessageDTO>(roomMessage);
            mapped.MessagesSender = user;
            return Ok(mapped);
        }

    }
}
