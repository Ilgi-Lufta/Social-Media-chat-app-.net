namespace TestFinal.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public int MessagesSenderId { get; set; }
        public User? MessagesSender { get; set; }
        public int MessagesReciverId { get; set; }
        public User? MessagesReciver { get; set; }
        public string message { get; set; }
        public DateTime? Date { get; set; }
    }
}
