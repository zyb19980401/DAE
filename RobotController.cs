using CloudWebApiServer.Entitys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NetTaste;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;
using Unclassified.Net;

namespace CloudWebApiServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RobotController : ControllerBase
    {
        private static Dictionary<string,User> TOKENDIC = new Dictionary<string, User>();
        private static List<AsyncTcpListener<RobotTcpServerClient>> tcpServerList;

        [HttpGet("GetUsers")]
        public IEnumerable<User> GetUsers()
        {
            return SqlSugarHelper.Query<User>("select * from [User]");
        }

        [HttpPost("AddUser")]
        public Result AddUser([FromBody] User user)
        {
            SqlSugarHelper.InsertUser(user);
            return new Result { Code = 1, Message = "Success", Data = null };
        }

        [HttpGet("GetUserToken")]
        public Result GetToken(string userId)
        {
            var user =  SqlSugarHelper.Query<User>($"select * from [User] where user_id = {userId}").FirstOrDefault();
            if (user == null)
            {
                return new Result { Code = -1, Message = "User does not exist" };
            }
            var token = Guid.NewGuid().ToString();
            TOKENDIC.Add(token, user);
            return new Result { Code = 1, Message = "Success",Data = token };
        }


        [HttpGet("StartTCPServer")]
        public Result StartTCPServer(string token)
        {
            var user = TOKENDIC[token];

            var tcpServer1 = new AsyncTcpListener<RobotTcpServerClient>()
            {
                Port = 9000,
            };

            if (tcpServerList != null && tcpServerList.Count > 0)
            {
                foreach (var item in tcpServerList)
                {
                    item.Stop(true);
                }
            }
            tcpServerList = new List<AsyncTcpListener<RobotTcpServerClient>>();
            tcpServerList.Add(tcpServer1);
            foreach (var item in tcpServerList)
            {
                item.RunAsync();
            }
            return new Result { Code = 1, Message = "Success" };
        }

        [HttpPost("SendChat")]
        public async Task<Result> SendChat([FromBody] RequestChat requestChat)
        {
            var user = TOKENDIC[requestChat.token];

            var action = new Actions
            {
                user_id = user.user_id.ToString(),
                action_type = requestChat.actionType,
                descirptions = requestChat.text
            };
            byte[] byteArray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(action));

            if (requestChat.actionType == "RobotClient1")
            {
                var tcpClient = CacheManager.GetItem<RobotTcpServerClient>("19000");
                await tcpClient.Send(new ArraySegment<byte>(byteArray));
            }

            return new Result { Code = 1, Message = "Success", Data = requestChat.token };
        }

        [HttpGet("GetResult")]
        public Result GetResult(string token,string actionType)
        {
            var user = TOKENDIC[token];
            var action = SqlSugarHelper.Query<Actions>($"select * from [Actions] where [user_id] = '{user.user_id.ToString()}' and action_type='{actionType}'").FirstOrDefault();
            if (action == null)
            {
                return new Result { Code = -1, Message = "Error User Actions not exist", Data = action };
            }
            return new Result { Code = 1, Message = "Success", Data = action };
        }

    }
}