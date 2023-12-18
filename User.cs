using SqlSugar;
using System.Security.Principal;

namespace CloudWebApiServer.Entitys
{
    public class User
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int user_id { get; set; }
        public string user_name { get; set; }
    }
}
