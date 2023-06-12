namespace MVCAmazonExtra.Models
{
    public class ModelLamdbaCorreo
    {
        public string Asunto { get; set; }

        public string Email { get; set; }

        public string Body { get; set; }

        public List<string>? Attachments { get; set; }
    }
}
