using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Homework1
{
    public class User
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string Email { get; set; }
        [JsonIgnore]
        public IEnumerable<Post> Posts { get; set; }
        [JsonIgnore]
        public IEnumerable<Todo> Todos { get; set; }
        [JsonIgnore]
        public Address Adress { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Id: {Id}");
            builder.AppendLine($"Name: {Name}");
            builder.AppendLine($"Avatar: {Avatar}");
            builder.AppendLine($"Email: {Email}");
            builder.AppendLine($"CreatedAt {CreatedAt}");
            return builder.ToString();
        }
    }
}
