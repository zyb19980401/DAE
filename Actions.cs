using SqlSugar;

namespace CloudWebApiServer.Entitys
{
    public class Actions
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int action_id { get; set; }
        public string user_id { get; set; }
        public string action_type { get; set; }
        public string descirptions { get; set; }
    }
}
