using Conduit.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Conduit
{
    //[Route("api/[controller]")]
    [ApiController]
    public class NamesController : ControllerBase
    {
        private static List<User> UserDb = new List<User>();
        
        public NamesController()
        {
            UserDb.Add(new User("Yousef", "yousef@yousef.com", "password", "123123123", "", "", new List<User>()));
        }

        [HttpGet]
        [Route("MVC/user")]
        public ActionResult GetCurrentUser()
        {
            User user = UserDb.FirstOrDefault(user => user.Token == this.Request.Headers["Authorization"]);
            return new JsonResult(new UserRequestEnv<User>(user));
        }

        [HttpPost]
        [Route("MVC/user")]
        public ActionResult Register([FromBody] UserRequestEnv<UserRequest> req)
        {
            User user = new User (req.User.Username, req.User.Email, req.User.Password, $"{Guid.NewGuid()}", "", "", new List<User>());
            UserDb.Add(user);
            return new JsonResult(new UserRequestEnv<User>(user));
        }

        [HttpPut]
        [Route("/MVC/user")]
        public async void UpdateUser()
        {
            var body = "";

            using (var reader = new StreamReader(this.Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }

            var req = JsonSerializer.Deserialize<UserRequestEnv<UserRequest>>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var user = GetUserByToken(this.Request.Headers["Authorization"], UserDb);

            OldUser OldUser = new OldUser(user);

            if (req.User.Username != "")
            {
                OldUser.Username = req.User.Username;
            }

            if (req.User.Email != "")
            {
                OldUser.Email = req.User.Email;
            }

            if (req.User.Password != "")
            {
                OldUser.Password = req.User.Password;
            }

            if (req.User.Email != "")
            {
                OldUser.Email = req.User.Email;
            }

            UserDb.Remove(user);
            var resp = new User(OldUser.Username, OldUser.Email, OldUser.Password, $"{this.Request.Headers["Authorization"]}", "", "", OldUser.follow);
            UserDb.Add(resp);
            await this.Response.WriteAsJsonAsync(new UserRequestEnv<User>(resp));
            //await new JsonResult(new UserRequestEnv<User>(user));
        }

        [HttpPost]
        [Route("/MVC/user/login")]
        public ActionResult LogIn([FromBody] UserRequestEnv<UserRequest> req)
        {

            var validator = new UserRequestValidator();
            var result = validator.Validate(req.User); 
            
            if(result.IsValid)
            {
                var user = UserDb.FirstOrDefault(x => x.Email == req.User.Email);
                if (user.Password == req.User.Password)
                {
                    //user.Token = Guid.NewGuid();
                    var userWithNewToken = new User(user.UserName, user.Email, user.Password, Guid.NewGuid().ToString(), "", "", user.Follow);
                    UserDb.Remove(user);
                    UserDb.Add(userWithNewToken);

                    return new JsonResult(new UserRequestEnv<User>(userWithNewToken));
                }
                else return BadRequest(new UserRequestEnv<string>("Invalid Password!!"));
            }

            // return new Empty User
            return BadRequest(result.Errors);
        }

        private User GetUserByToken(string Token, List<User> Users)
        {
            var user = Users.FirstOrDefault(user => user.Token == Token);

            return user;
        }
    }
}
