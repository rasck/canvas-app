using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanvasApp
{
    public class Group
    {
        public int id { get; set; }
        public string name { get; set; }
        public string max_membership { get; set; }
        public bool is_public { get; set; }
        public string join_level { get; set; }
        public int group_category_id { get; set; }
        public object description { get; set; }
        public string members_count { get; set; }
        public string storage_quota_mb { get; set; }
        public string context_type { get; set; }
        public int course_id { get; set; }
        public object avatar_url { get; set; }
        public object role { get; set; }
        public Leader leader { get; set; }
        public bool has_submission { get; set; }
        public bool concluded { get; set; }

        public IEnumerable<User> userList { get; set; }
    }
}
