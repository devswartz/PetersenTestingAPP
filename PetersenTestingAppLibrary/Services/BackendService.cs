using Microsoft.Data.SqlClient;
using PetersenTestingAppLibrary.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetersenTestingAppLibrary.Services
{
    public class BackendService
    {
        public Dictionary<string, PetersenUser> Users { get; set; } = new Dictionary<string, PetersenUser>();
        public Utils utils { get; set; }
        public PetersenUser currentUser { get; set; }
        private string azureId { get; set; }

        public BackendService(Utils utils)
        {
            this.utils = utils;
            Users = GetAllUsers();
        }

        public async Task SetCurrentUser(string id)
        {
            if (Users.ContainsKey(id)) { 
                currentUser = Users[id];
            }
        }

        public async Task Initialize(string userId)
        {
            if (azureId == null) {
                azureId = userId;
                await SetCurrentUser(userId);
            }

        }

        public Dictionary<string, PetersenUser> GetAllUsers()
        {
            DataTable dt = new DataTable();
            Dictionary<string, PetersenUser> Users = new Dictionary<string, PetersenUser>();

            using(SqlConnection cn = new SqlConnection(utils.sqlServerConnection))
            {
                cn.Open();
                //string sqlText = $@"SELECT * FROM employee.dbo.employees order by first_name, last_name";
                string sqlText = $@"Select * from DATAMIRROR.dbo.PETERSEN_USERS order by name";
                using(var adapter = new SqlDataAdapter(sqlText, cn))
                {
                    adapter.SelectCommand.CommandTimeout = 100;
                    adapter.Fill(dt);
                }

            }

            foreach(DataRow dr in dt.Rows)
            {
                PetersenUser user = new PetersenUser();

                user.UserId = dr["user_id"].ToString();
                user.Name = dr["name"].ToString();
                user.EmployeeId = dr.IsNull("EmployeeID")? null: dr["EmployeeID"].ToString();
                user.Email = dr["email"].ToString();
                user.petersen_testing = Convert.ToInt32(dr["petersen_testing"]);
                Users.Add(user.UserId,user);

            }

            return Users;

        }


    }
}
