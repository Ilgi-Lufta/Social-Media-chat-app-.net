using System.ComponentModel.DataAnnotations;
using TestFinal.Models;

namespace TestFinal.ViewModel
{
    public class ViewModels
    {
        public class PostDTO
        {
            public int PostId { get; set; }
            public string Description { get; set; }
            public string Myimage { get; set; }
            public int UserId { get; set; }
            public UserDTO Creator { get; set; }

            public List<LikeDTO> Likes { get; set; } = new List<LikeDTO>();

            public List<CommentDTO> Comments { get; set; } = new List<CommentDTO>();

            public DateTime CreatedAt { get; set; } = DateTime.Now;
            public DateTime UpdatedAt { get; set; } = DateTime.Now;
        }

        public class UserDTO
        {
            public int UserId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string UserName { get; set; }
            public string Myimage { get; set; }

        //    public List<RequestDTO> RequestsReciver { get; set; } = new List<RequestDTO>();
        //    public List<RequestDTO> RequestsSender { get; set; } = new List<RequestDTO>();
           // public List<LikeDTO> Likes { get; set; } = new List<LikeDTO>();
           // public List<CommentDTO> Comments { get; set; } = new List<CommentDTO>();
            //public List<PostDTO> CreatedPost { get; set; } = new List<PostDTO>();
        }

        public class LikeDTO
        {
            public int LikeId { get; set; }
            public int UserId { get; set; }
            public int PostId { get; set; }
            public UserDTO UseriQePelqen { get; set; }
        }

        public class CommentDTO
        {
            public int CommentId { get; set; }
            public string Content { get; set; }
            public int UserId { get; set; }
            public UserDTO UseriQekomenton { get; set; }
        }

        public class RequestDTO
        {
            public int RequestId { get; set; }
            public bool Accepted { get; set; } = false;
            public int SenderId { get; set; }
            public UserDTO? Sender { get; set; }
            public int ReciverId { get; set; }

            public UserDTO? Reciver { get; set; }
            public DateTime CreatedAt { get; set; } = DateTime.Now;
            public DateTime UpdatedAt { get; set; } = DateTime.Now;

        }
        public class postEditViewModel
        {
            public IFormFile File { get; set; }
            public string Description { get; set; }

        }
        public class MessageDTO
        {
            public int MessageId { get; set; }
            public int MessagesSenderId { get; set; }
            public UserDTO? MessagesSender { get; set; }
            public int MessagesReciverId { get; set; }
            public UserDTO? MessagesReciver { get; set; }
            public string message { get; set; }
            public DateTime Date { get; set; } = DateTime.Now;

        }
        public class ChatRoomEditViewModel
        {
            public List<int> MembersId { get; set; } = new List<int>();
            public string? Name { get; set; } 
        }
        public class ChatRoomDTO
        {
            public int ChatRoomId { get; set; }
            public string? Name { get; set; }
            //public List<RoomMessage> Messages { get; set; } = new List<RoomMessage>();
            //public List<UserChatRoom> UserChatRooms { get; set; } = new List<UserChatRoom>();
            public bool IsGroupChat { get; set; }
        }
        public class RoomMessageDTO
        {
            public int RoomMessageId { get; set; }
            public int MessagesSenderId { get; set; }
            public UserDTO? MessagesSender { get; set; }
            public int ChatRoomId { get; set; }
            public ChatRoomDTO? ChatRoom { get; set; }
            public string Message { get; set; }
            public DateTime? Date { get; set; }
        }
        public class RoomMessageEditViewModel
        {
            public int MessagesSenderId { get; set; }
 
            public int ChatRoomId { get; set; }

            public string Message { get; set; }

        }

    }
   
}
