namespace ApiProjeKampi.WebApi.Dtos.MessageDto
{
    public class GetByIdMessageDto
    {
        public int MessageId { get; set; }
        public string NameSurname { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string MessageDetails { get; set; }
        public DateTime SendDate { get; set; }
        public bool IsREad { get; set; }
        public string Status { get; set; }
    }
}
