using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanvasApp
{
    public class GroupCategory
    {
        public int id { get; set; }
        public string name { get; set; }
        public object role { get; set; }
        public string self_signup { get; set; }
        public string group_limit { get; set; }
        public string auto_leader { get; set; }
        public string context_type { get; set; }
        public int course_id { get; set; }
        public bool @protected { get; set; }
        public bool allows_multiple_memberships { get; set; }
        public bool is_member { get; set; }
    }
}
