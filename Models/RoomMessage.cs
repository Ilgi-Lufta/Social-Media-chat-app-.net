namespace TestFinal.Models
{
    public class RoomMessage
    {
        public int RoomMessageId { get; set; }
        public int MessagesSenderId { get; set; }
        public User? MessagesSender { get; set; }
        public int ChatRoomId { get; set; }
        public ChatRoom? ChatRoom { get; set; }
        public string Message { get; set; }
        public DateTime? Date { get; set; }
       
    }
}
