namespace TestFinal.Models
{
    public class ChatRoom
    {
        public int ChatRoomId { get; set; }
        public string? Name { get; set; }
        public List<RoomMessage> Messages { get; set; } = new List<RoomMessage>();
        public List<UserChatRoom> UserChatRooms { get; set; } = new List<UserChatRoom>();
        public bool IsGroupChat { get; set; }
    }
}
