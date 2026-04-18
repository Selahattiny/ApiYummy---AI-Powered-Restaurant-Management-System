namespace ApiProjeKampi.WebApi.Dtos.MessageDto
{
    public class CreateMessageDto
    {
        public string NameSurname { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string MessageDetails { get; set; }
        public DateTime SendDate { get; set; }
        public bool IsREad { get; set; }
        public string Status { get; set; }
    }
}
