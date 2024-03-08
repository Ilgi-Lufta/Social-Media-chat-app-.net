namespace TestFinal.Models
{
    public class UserChatRoom
    {
        public int UserChatRoomId { get; set; }
        public int ChatMemberId { get; set; }
        public User? ChatMember { get; set; }
        public int ChatRoomId { get; set; }
        public ChatRoom? ChatRoom { get; set; }
    }
}
