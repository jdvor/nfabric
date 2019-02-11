namespace SampleApp.Http
{
    using System.Runtime.Serialization;

    [DataContract]
    public class JsonPlaceholderPost
    {
        [DataMember(Name = "id")]
        public int? Id { get; set; }

        [DataMember(Name = "userId")]
        public int UserId { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "body")]
        public string Body { get; set; }
    }
}
