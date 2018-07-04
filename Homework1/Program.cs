using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;

namespace Homework1
{
    class Program
    {
        private static IEnumerable<User> CompleteUsers;

        static void Main(string[] args)
        {
            string link = "https://5b128555d50a5c0014ef1204.mockapi.io/";
            var client = new WebClient();
            string usersJson = client.DownloadString(link + "users");
            var users = JsonConvert.DeserializeObject<IEnumerable<User>>(usersJson);

            string postsJson = client.DownloadString(link + "posts");
            var posts = JsonConvert.DeserializeObject<IEnumerable<Post>>(postsJson);

            string todosJson = client.DownloadString(link + "todos");
            var todos = JsonConvert.DeserializeObject<IEnumerable<Todo>>(todosJson);

            string addressesJson = client.DownloadString(link + "address");
            var address = JsonConvert.DeserializeObject<IEnumerable<Address>>(addressesJson);

            string commentsJson = client.DownloadString(link + "comments");
            var comments = JsonConvert.DeserializeObject<IEnumerable<Comment>>(commentsJson);

            CompleteUsers = from user in users
                            join post in
                            (from post in posts
                             join comment in comments on post.Id equals comment.PostId into commentGroup
                             select new Post
                             {
                                 Id = post.Id,
                                 Title = post.Title,
                                 Body = post.Body,
                                 CreatedAt = post.CreatedAt,
                                 Likes = post.Likes,
                                 UserId = post.UserId,
                                 Comments = commentGroup
                             })
                             on user.Id equals post.UserId into postGroup
                            join todo in todos on user.Id equals todo.UserId into todoGroup
                            join adress in address on user.Id equals adress.UserId
                            select new User
                            {
                                Name = user.Name,
                                Id = user.Id,
                                Avatar = user.Avatar,
                                CreatedAt = user.CreatedAt,
                                Email = user.Email,
                                Posts = postGroup,
                                Todos = todoGroup,
                                Adress = adress
                            };
            
            Console.WriteLine("Hello!");

            bool continuation = true;
            while (continuation)
            {
                int numberofTask = ChooseTask();
                switch (numberofTask)
                {
                    case 0:
                        continuation = false;
                        break;
                    case 1:
                        int number = ReturnNumber(1, CompleteUsers.Count());
                        int result = GetCommentCountForUser(number);
                        Console.WriteLine("Coment count for user:" + result);
                        break;
                    case 2:
                        int number2 = ReturnNumber(1, CompleteUsers.Count());
                        var result2 = GetShortCommentsForUser(number2);
                        foreach (var comment in result2)
                        {
                            Console.WriteLine(comment.Body);
                        }
                        break;
                    case 3:
                        int number3 = ReturnNumber(1, CompleteUsers.Count());
                        var result3 = GetIncompleteTodosForUser(number3);
                        foreach (var i in result3)
                            Console.WriteLine("Id: " + i.Id + " Name: " + i.Name);
                        break;
                    case 4:
                        var result4 = SortUsersByNameAndTodoNameLenght();
                        foreach (var i in result4)
                        {
                            Console.WriteLine("UserName: " + i.Name + $"Todo items: {(i.Todos.Any() ? string.Empty : "None")}");
                            foreach (var b in i.Todos)
                                Console.WriteLine(b.Name);
                        }
                        break;
                    case 5:
                        int number5 = ReturnNumber(1, CompleteUsers.Count());
                        var result5 = GetFilteredInfo(number5);
                        Console.WriteLine(result5.user);
                        Console.WriteLine($"Last post: {(result5.lastPost != null ? $"Id: {result5.lastPost.Id}; Title: {result5.lastPost.Title}; Comment count: {result5.lastPostCommentAmmount}" : "None")}");
                        Console.WriteLine($"Unfinished todo count: {result5.unfinishedTodosCount}");
                        Console.WriteLine($"Post with longest comments: {(result5.postWithBigCommentCount != null ? $"Id : {result5.postWithBigCommentCount.Id}; Title: {result5.postWithBigCommentCount.Title}; Comment count: {result5.postWithBigCommentCount.Comments.Length}" : "None")}");
                        Console.WriteLine($"Post with most likes: {(result5.postWithManyLikes != null ? $"Id: {result5.postWithManyLikes.Id}; Title: {result5.postWithManyLikes.Title}; Likes count: {result5.postWithManyLikes.Likes}" : "None")}");
                        break;
                    case 6:
                        int number6 = ReturnNumber(1, posts.Count());
                        var result6 = GetPostInfo(number6);
                        Console.WriteLine($"Post {number6} longest comment: {(result6.LongestComment != null ? result6.LongestComment.Body : "None")}");
                        Console.WriteLine($"Post {number6} most liked comment: {(result6.MostLikedComment != null ? $"like count {result6.MostLikedComment.Likes} \r\nText: {result6.MostLikedComment.Body}" : "None")}");
                        Console.WriteLine($"Post {number6} unliked small comment count: {result6.AmountComment}");
                        break;
                    default:
                        Console.WriteLine("You have entered wrong number!");
                        break;
                }
            }
        }

        /// <summary>
        /// Task6: Получить следующую структуру(передать Id поста в параметры)
        /// Пост
        /// Самый длинный коммент поста
        /// Самый залайканный коммент поста
        /// Количество комментов под постом где или 0 лайков или длина текста< 80
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        private static dynamic GetPostInfo(int postId)
        {
            var pos = CompleteUsers.SelectMany(x => x.Posts).Where(i => i.Id == postId).Select(post => (new
            {
                LongestComment = post.Comments.OrderByDescending(a => a.Body.Length).FirstOrDefault(),
                MostLikedComment = post.Comments.OrderByDescending(s => s.Likes).FirstOrDefault(),
                AmountComment = post.Comments.Where(o => o.Likes == 0 || o.Body.Length < 80).Count()
            })).FirstOrDefault();
            return pos;
        }

        /// <summary>
        /// Task5: 
        /// Получить следующую структуру(передать Id пользователя в параметры)
        /// User
        /// Последний пост пользователя(по дате)
        /// Количество комментов под последним постом
        /// Количество невыполненных тасков для пользователя
        /// Самый популярный пост пользователя(там где больше всего комментов с длиной текста больше 80 символов)
        /// Самый популярный пост пользователя(там где больше всего лайков)
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private static dynamic GetFilteredInfo(int userId)
        {
            var curentUser = CompleteUsers.Where(x => x.Id == userId).Select(user => new
            {
                user,
                lastPost = user.Posts.OrderBy(post => post.CreatedAt).FirstOrDefault(),
                lastPostCommentAmmount = user.Posts.OrderByDescending(post => post.CreatedAt).FirstOrDefault()?.Comments.Count() ?? 0,
                unfinishedTodosCount = user.Todos.Count(todo => !todo.IsComplete),
                postWithBigCommentCount = user.Posts.OrderByDescending(post => post.Comments.Count(comment => comment.Body.Length > 80)).FirstOrDefault(),
                postWithManyLikes = user.Posts.OrderByDescending(post => post.Likes).FirstOrDefault(),
            }).FirstOrDefault();
            return curentUser;
        }

        /// <summary>
        /// Task4: Получить список пользователей по алфавиту(по возрастанию) с отсортированными todo items по длине name(по убыванию)
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<User> SortUsersByNameAndTodoNameLenght()
        {
            var userList = CompleteUsers.OrderBy(a => a.Name).Select(user => new User
            {
                Name = user.Name,
                Id = user.Id,
                Avatar = user.Avatar,
                CreatedAt = user.CreatedAt,
                Email = user.Email,
                Posts = user.Posts,
                Todos = user.Todos.OrderByDescending(a => a.Name.Length),
                Adress = user.Adress,
            });
            return userList;
        }

        /// <summary>
        /// Task3: Получить список(id, name) из списка todos которые выполнены для конкретного пользователя(по айди)
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private static IEnumerable<dynamic> GetIncompleteTodosForUser(int userId)
        {
            var listidname = CompleteUsers.FirstOrDefault(x => x.Id == userId).Todos.Where(i => i.IsComplete).Select(todo => new { todo.Id, todo.Name });
            return listidname;
        }

        /// <summary>
        /// Task2: Получить список комментов под постами конкретного пользователя(по айди), где body коммента<50 символов(список из комментов)
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private static IEnumerable<Comment> GetShortCommentsForUser(int userId)
        {
            var listcom = CompleteUsers.FirstOrDefault(x => x.Id == userId).Posts.SelectMany(post => post.Comments).Where(c => c.Body.Length < 50);
            return listcom;
        }

        /// <summary>
        /// Task1: Получить количество комментов под постами конкретного пользователя(по айди)(список из пост - количество)
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// 
        private static int GetCommentCountForUser(int userId)
        {
            var amount = CompleteUsers.FirstOrDefault(x => x.Id == userId).Posts.Where(i => i.UserId == userId).Select(c => c.Comments).Count();
            return (amount);
        }

        public static int ChooseTask()
        {
            Console.WriteLine("What task do you want to try? \r\nFor choosing task please enter number from 1 to 6.");
            Console.WriteLine("Enter 0 to exit.");
            int number = Convert.ToInt32(Console.ReadLine());
            return number;
        }

        public static int ReturnNumber(int min, int max)
        {
            Console.WriteLine($"Please enter ID from {min} to {max}:");
            int number = Convert.ToInt32(Console.ReadLine());
            return number;
        }

    }
}
