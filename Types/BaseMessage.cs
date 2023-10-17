namespace Types
{
    public class BaseMessage
    {
        public string Id { get; private set; }
        public DateTime MessageCreated { get; private set; }
        public string Content { get; set; }

        public BaseMessage(string content)
        {
            Id = Guid.NewGuid().ToString();
            MessageCreated = DateTime.UtcNow;

            Content = content;
        }
    }
}
