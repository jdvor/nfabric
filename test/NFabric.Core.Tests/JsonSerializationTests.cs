namespace NFabric.Core.Tests
{
    using NFabric.Core.Http;
    using NFabric.Core.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using Xunit;

    public class JsonSerializationTests
    {
        [Fact]
        public void Shortenning()
        {
            var sut = new JsonSerializationBuilder()
                .UseShortTypeNames(typeof(BlogInfo), typeof(BlogPost))
                .UsePrettyFormat()
                .Build();

            var data = BlogInfo.Create();
            var bytes = sut.Serialize(data);
            var s = Encoding.UTF8.GetString(bytes);
            Assert.Contains("BP.", s);

            var data2 = sut.Deserialize(bytes);
            Assert.True(data2 is BlogInfo);
        }

        [Fact]
        public void Shortenning2()
        {
            var sut = new JsonSerializationBuilder()
                .UseShortTypeNames(typeof(BlogInfo), typeof(BlogPost), typeof(Dictionary<string, object>))
                .UsePrettyFormat()
                .Build();

            var data = new Dictionary<string, object>
            {
                { "AAA", BlogInfo.Create() },
                { "BBB", 125.95 },
                { "CCC", new[] { "fff", "eee" } },
                { "DDD", BlogPost.Create() },
            };
            var bytes = sut.Serialize(data);
            var s = Encoding.UTF8.GetString(bytes);

            var data2 = sut.Deserialize(bytes);
        }

        [Fact]
        public void IgnoresNullsAndDoesNotIgnoreDefaultValues()
        {
            var sut = new JsonSerializationBuilder()
                .UsePrettyFormat()
                .Build();

            var r = new Response<BlogInfo>(HttpStatusCode.BadRequest, "something bad happend");
            var bytes = sut.Serialize(r);
            var str = Encoding.UTF8.GetString(bytes);

            Assert.Contains("isSuccess", str);
            Assert.DoesNotContain("exception", str);
        }
    }

    public class BlogInfo
    {
        public string Name { get; set; }

        public BlogPost[] Posts { get; set; }

        public static BlogInfo Create()
        {
            return new BlogInfo
            {
                Name = "Blog 1",
                Posts = new[]
                {
                    new BlogPost
                    {
                        Label = "Post 1",
                        Date = DateTimeOffset.Now,
                    },
                    new BlogPost
                    {
                        Label = "Post 2",
                        Date = DateTimeOffset.Now.AddDays(-3),
                    }
                },
            };
        }
    }

    public class BlogPost
    {
        public string Label { get; set; }

        public DateTimeOffset Date { get; set; }

        public static BlogPost[] Create()
        {
            return  new[]
            {
                new BlogPost
                {
                    Label = "Post 3",
                    Date = DateTimeOffset.Now.AddDays(-6),
                },
                new BlogPost
                {
                    Label = "Post 4",
                    Date = DateTimeOffset.Now.AddDays(-11),
                }
            };
        }
    }
}
