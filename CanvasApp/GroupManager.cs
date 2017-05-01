using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanvasApp
{
    public class GroupManager
    {

        private CanvasApiHelper apiHelper;
        private ExcelBuilder excelBuilder;

        public GroupManager()
        {
            apiHelper = new CanvasApiHelper();
            excelBuilder = new ExcelBuilder();
        }

        private async Task<IEnumerable<GroupCategory>> GetGroupCategories(Http http, string courseId)
        {
            http.Url = new Uri(@"https://ucn.instructure.com/api/v1/courses/" + courseId + "/group_categories");
            try
            {
                var result = await apiHelper.GetContent<GroupCategory>(http);

                return result;
            }
            catch (Exception)
            {
                Console.WriteLine("Could not get group categories");
                throw;
            }

        }

        private async Task<IDictionary<GroupCategory, IEnumerable<Group>>> GetGroups(Http http, IEnumerable<GroupCategory> groupCategoryList)
        {
            // Each course can have mulitple group categories. Each group categories has x number of groups
            IDictionary<GroupCategory, IEnumerable<Group>> groupList = new Dictionary<GroupCategory, IEnumerable<Group>>();
            try
            {
                foreach (var groupCategory in groupCategoryList)
                {
                    http.Url = new Uri(@"https://ucn.instructure.com/api/v1/group_categories/" + groupCategory.id + "/groups");

                    var result = await apiHelper.GetContent<Group>(http);

                    groupList.Add(groupCategory, result);
                }

                return groupList;
            }
            catch (Exception)
            {
                Console.WriteLine("Could not get groups");
                throw;
            }

        }

        private async Task<IDictionary<GroupCategory, IEnumerable<Group>>> GetUsers(Http http, IDictionary<GroupCategory, IEnumerable<Group>> groupGroupList)
        {
            try
            {
                foreach (var groupList in groupGroupList)
                {
                    List<User> userList = new List<User>();
                    foreach (var group in groupList.Value)
                    {
                        http.Url = new Uri(@"https://ucn.instructure.com/api/v1/groups/" + group.id + "/users");

                        group.userList = await apiHelper.GetContent<User>(http);
                    }
                }
                return groupGroupList;
            }
            catch (Exception)
            {
                Console.WriteLine("Could not enrich groups with user data");
                throw;
            }
        }

        private async Task<IDictionary<GroupCategory, IEnumerable<Group>>> GetGroupsWithUsers(Http http, string courseId, string classname)
        {
            // Each canvas course can have mulitple group categories
            IEnumerable<GroupCategory> groupCats = await GetGroupCategories(http, courseId);
            // Each group category can have mulitple groups
            IDictionary<GroupCategory, IEnumerable<Group>> groups = await GetGroups(http, groupCats);
            // The groups in canvas domain model does not know its participants, but the participants know what group they are in, we invert this here
            // Enrich the groups with users
            IDictionary<GroupCategory, IEnumerable<Group>> groupsWithUsers = await GetUsers(http, groups);

            return groupsWithUsers;
        }

        public async Task CreateExcelSheetWithGroups(Http http, string courseId, string classname)
        {
            var groupsWithUsers = await GetGroupsWithUsers(http, courseId, classname);

            // We can now create the excel sheet with group information
            await Task.Run(() => excelBuilder.CreateExcelSheet(
                excelBuilder.CreateUserDateTable(groupsWithUsers),
                classname));
        }
    }
}
