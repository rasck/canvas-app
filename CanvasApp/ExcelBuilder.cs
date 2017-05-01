using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanvasApp
{
    public class ExcelBuilder
    {
        /// <summary>
        /// Create a datatable for each Group Category in a Canvas course
        /// </summary>
        /// <param name="userGroupList"></param>
        /// <returns></returns>
        public IEnumerable<DataTable> CreateUserDateTable(IDictionary<GroupCategory, IEnumerable<Group>> userGroupList)
        {
            foreach (var groupCategoryKeyVal in userGroupList)
            {
                DataTable dt = new DataTable();
                dt.TableName = groupCategoryKeyVal.Key.name;
                dt.Columns.Add("Navn");
                dt.Columns.Add("UCN mail");
                dt.Columns.Add("Gruppenavn");
                dt.Columns.Add("GruppeLink");

                string groupLink = @"https://ucn.instructure.com/groups/";

                foreach (Group group in groupCategoryKeyVal.Value)
                {
                    foreach (User user in group.userList)
                    {
                        dt.Rows.Add(user.name, user.login_id, group.name, groupLink + group.id);
                    }
                }
                yield return dt;
            }
        }

        /// <summary>
        /// Creates an excel sheet where each datatable has its own worksheet with the same name as the name of the datatable.
        /// </summary>
        /// <param name="dtList"></param>
        /// <param name="fileName"></param>
        public async Task CreateExcelSheet(IEnumerable<DataTable> dtList, string fileName)
        {
            //http://epplus.codeplex.com/
            //http://zeeshanumardotnet.blogspot.dk/2010/08/creating-advanced-excel-2007-reports-on.html

            using (ExcelPackage p = new ExcelPackage())
            {
                p.Workbook.Properties.Title = fileName;
                int wsIndex = 1;
                foreach (var dt in dtList)
                {
                    string title = dt.TableName;

                    p.Workbook.Worksheets.Add(title);
                    ExcelWorksheet ws = p.Workbook.Worksheets[wsIndex];
                    ws.Name = title;
                    int rowIndex = 1;
                    int colIndex = 1;

                    // Headers
                    foreach (DataColumn dc in dt.Columns)
                    {
                        ws.Cells[rowIndex, colIndex].Value = dc.ColumnName;
                        colIndex++;
                    }

                    // Data
                    foreach (DataRow dr in dt.Rows)
                    {
                        colIndex = 1;
                        rowIndex++;
                        foreach (DataColumn dc in dt.Columns)
                        {
                            var cell = ws.Cells[rowIndex, colIndex];
                            cell.Value = dr[dc.ColumnName];
                            colIndex++;
                        }
                    }
                    wsIndex++;
                }

                //Generate a File with  fileName + a random guid in its name
                Byte[] bin = p.GetAsByteArray();
                string file = ".\\" + fileName + " - " + Guid.NewGuid().ToString() + ".xlsx";
                await Task.Run(() => File.WriteAllBytes(file, bin));
            }
        }
    }
}