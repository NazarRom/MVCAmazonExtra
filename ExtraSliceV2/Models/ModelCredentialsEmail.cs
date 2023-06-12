namespace MVCAmazonExtra.Models
{
    public class ModelCredentialsEmail
    {
        public string User { get; set; }

        public string Password { get; set; }    

        public int Port { get; set; }

        public string Host { get; set; }

        public bool Enablessl { get; set; }

        public bool DefaultCredentials { get; set; }

        public string Api { get; set; }
    }
}
