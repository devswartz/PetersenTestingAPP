using Microsoft.Data.SqlClient;
using PetersenTestingAppLibrary.Classes;
using System.Data;

namespace PetersenTestingAppLibrary.Services
{
    public class TestingService
    {
        public Utils utils {  get; set; }
        public TestingService(Utils utils)
        {
            this.utils = utils;
        }
        public TimeZoneInfo centralzone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");



        public async Task<PetersenUser> GetUserInfo(string? azureId)
        {
            PetersenUser user = new PetersenUser();

            if (string.IsNullOrWhiteSpace(azureId)) return user;

            try
            {
                string query = @"SELECT * FROM DATAMIRROR.dbo.petersen_users where user_id=@azureId";
                using (SqlConnection conn = new SqlConnection(utils.sqlServerConnection))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        var p = cmd.Parameters.Add("@azureId", SqlDbType.NVarChar, 4000);
                        p.Value = azureId;

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {

                            if (await reader.ReadAsync())
                            {
                                user = new PetersenUser
                                {
                                    UserId = reader["user_id"].ToString(),
                                    Name = reader["name"].ToString(),
                                    EmployeeId = reader["EmployeeID"] == DBNull.Value ? string.Empty : reader["EmployeeID"].ToString(),
                                    Email = reader["email"].ToString(),
                                    petersen_testing = Convert.ToInt32(reader["petersen_testing"])

                                };

                            }

                        }
                    }

                }
            }
            catch (SqlException ex)
            {
                // Log as needed, then wrap/rethrow
                throw new InvalidOperationException("Error fetching user from database", ex);
            }

            return user;

        }


        public async Task<List<TestHeader>> GetTopTestHeader()
        {

            await DataSyncTestHeader(); // sync Database with most update data

            List<TestHeader> list = new List<TestHeader>();
            string query = @"
                            with combinedDetails As (
                            select HeaderID, TestTime from PETERSENTESTING.dbo.TestDetail_InflationPressure
                            union all
                            select HeaderId, TestTime from PETERSENTESTING.dbo.TestDetail_BackPressureSeal
                            ),
                            latestDetail as (select HeaderID, max(TestTime) as LastTestDateTime from combinedDetails group by HeaderID)
                            select top 50 
                            CASE WHEN ISDATE(CONVERT(varchar(8), ORDDATE)) = 1 THEN CONVERT(datetime, CONVERT(varchar(8), ORDDATE), 112) ELSE NULL END AS ORDDATE,
                            CASE WHEN ISDATE(CONVERT(varchar(8), DueDate)) = 1 THEN CONVERT(datetime, CONVERT(varchar(8), DueDate), 112) ELSE NULL END AS DueDate,
                            h.*, ld.LastTestDateTime from PETERSENTESTING.dbo.TestHeader as h
                            left join latestDetail as ld
                            on h.HeaderID = ld.HeaderID
                            order by ld.LastTestDateTime desc
                            ";
            using (SqlConnection conn = new SqlConnection(utils.sqlServerConnection))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandTimeout = 500;

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync()) {
                        
                        while (await reader.ReadAsync()) {
                            list.Add(new TestHeader
                            {
                                HeaderId = Convert.ToInt32(reader["HeaderID"]),
                                ORDUNIQ = Convert.ToInt32(reader["ORDUNIQ"]),
                                ReferenceNumber = reader["ReferenceNumber"].ToString(),
                                Item = reader.IsDBNull(reader.GetOrdinal("Item")) ? null : reader["Item"].ToString(),
                                ItemDesc = reader.IsDBNull(reader.GetOrdinal("ItemDesc")) ? null : reader["ItemDesc"].ToString(),
                                RatedMaxInflationPSI = reader.IsDBNull(reader.GetOrdinal("RatedMaxInflationPSI")) ? null : reader["RatedMaxInflationPSI"].ToString(),
                                DeflatedOrInternalDiaInches = reader.IsDBNull(reader.GetOrdinal("DeflatedOrInternalDiaInches")) ? null : reader["DeflatedOrInternalDiaInches"].ToString(),
                                MaxInflatedDiaInches = reader.IsDBNull(reader.GetOrdinal("MaxInflatedDiaInches")) ? null : reader["MaxInflatedDiaInches"].ToString(),
                                Customer = reader.IsDBNull(reader.GetOrdinal("Customer")) ? null : reader["Customer"].ToString(),
                                CustomerName = reader.IsDBNull(reader.GetOrdinal("CustomerName")) ? null : reader["CustomerName"].ToString(),
                                Closed = reader.IsDBNull(reader.GetOrdinal("Closed")) ? (int?)null : Convert.ToInt32(reader["Closed"]),
                                ClosedDT = reader.IsDBNull(reader.GetOrdinal("ClosedDT")) ? (DateTime?)null : Convert.ToDateTime(reader["ClosedDT"]),
                                AudtTime = reader.IsDBNull(reader.GetOrdinal("AUDTTime")) ? null : Convert.ToDateTime(reader["AUDTTime"]),
                                AudtUser = reader.IsDBNull(reader.GetOrdinal("AUDTUser")) ? null : reader["AUDTUser"].ToString(),
                                ORDDate = reader.IsDBNull(reader.GetOrdinal("ORDDATE")) ? (DateTime?)null : Convert.ToDateTime(reader["ORDDATE"]),
                                DueDate = reader.IsDBNull(reader.GetOrdinal("DueDate")) ? (DateTime?)null : Convert.ToDateTime(reader["DueDate"]),
                                SONUM = reader.IsDBNull(reader.GetOrdinal("SONUM")) ? null : reader["SONUM"].ToString(),
                                TestNumber = reader.IsDBNull(reader.GetOrdinal("TestNumber")) ? null : reader["TestNumber"].ToString(),
                                NotForTest = reader.IsDBNull(reader.GetOrdinal("NotForTest")) ? 0: Convert.ToInt32(reader["NotForTest"]),
                                CustomParam = reader.IsDBNull(reader.GetOrdinal("CustomParam")) ? null : reader["CustomParam"].ToString(),
                                Channel = reader.IsDBNull(reader.GetOrdinal("Channel")) ? null : reader["Channel"].ToString(),
                                LastEntryDate = reader.IsDBNull(reader.GetOrdinal("LastTestDateTime")) ? (DateTime?)null : Convert.ToDateTime(reader["LastTestDateTime"]),
                            });


                        }

                    }

                }
            }

            return list;
        }

        public async Task DataSyncTestHeader()
        {
            using(SqlConnection conn = new SqlConnection(utils.sqlServerConnection))
            {
                await conn.OpenAsync();

                using(SqlCommand cmd = new SqlCommand("SyncTestHeader", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    await cmd.ExecuteNonQueryAsync();

                }

            }
        }

        
        public async Task<List<TestHeader>> GetFilteredHeaders(string? MoRef, string? ItemNo, int? status)
        {

            await DataSyncTestHeader(); // sync Database with most update data


            List<TestHeader> list = new List<TestHeader>();
            string query = @"select top 6000 

                            HeaderId,ORDUNIQ,ReferenceNumber,Item,ItemDesc,RatedMaxInflationPSI,DeflatedOrInternalDiaInches,MaxInflatedDiaInches,Customer,CustomerName,Closed,ClosedDT,AUDTTime,AUDTUser,
                            CASE WHEN ISDATE(CONVERT(varchar(8), ORDDATE)) = 1 THEN CONVERT(datetime, CONVERT(varchar(8), ORDDATE), 112) ELSE NULL END AS ORDDATE,
                            CASE WHEN ISDATE(CONVERT(varchar(8), DueDate)) = 1 THEN CONVERT(datetime, CONVERT(varchar(8), DueDate), 112) ELSE NULL END AS DueDate,
                            SONUM,TestNumber,NotForTest,CustomParam,Channel

                            from PETERSENTESTING.dbo.TestHeader h
	                        where (@MoRef is null or h.ReferenceNumber like @MoRef+'%')
	                        and (@ItemNo is null or h.Item like @ItemNo+'%')
	                        and ( @Status=2 or (@Status=0 and h.Closed=0) or (@Status=1 and h.Closed=1)) order by ORDDATE, DueDate desc";

            using (SqlConnection conn = new SqlConnection(utils.sqlServerConnection))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandTimeout = 2000;
                    cmd.Parameters.AddWithValue("@MoRef", (object?)MoRef ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ItemNo", (object?)ItemNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", (object?)status ?? DBNull.Value);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new TestHeader
                            {
                                HeaderId = Convert.ToInt32(reader["HeaderID"]),
                                ORDUNIQ = Convert.ToInt32(reader["ORDUNIQ"]),
                                ReferenceNumber = reader["ReferenceNumber"].ToString(),
                                Item = reader.IsDBNull(reader.GetOrdinal("Item")) ? null : reader["Item"].ToString(),
                                ItemDesc = reader.IsDBNull(reader.GetOrdinal("ItemDesc")) ? null : reader["ItemDesc"].ToString(),
                                RatedMaxInflationPSI = reader.IsDBNull(reader.GetOrdinal("RatedMaxInflationPSI")) ? null : reader["RatedMaxInflationPSI"].ToString(),
                                DeflatedOrInternalDiaInches = reader.IsDBNull(reader.GetOrdinal("DeflatedOrInternalDiaInches")) ? null : reader["DeflatedOrInternalDiaInches"].ToString(),
                                MaxInflatedDiaInches = reader.IsDBNull(reader.GetOrdinal("MaxInflatedDiaInches")) ? null : reader["MaxInflatedDiaInches"].ToString(),
                                Customer = reader.IsDBNull(reader.GetOrdinal("Customer")) ? null : reader["Customer"].ToString(),
                                CustomerName = reader.IsDBNull(reader.GetOrdinal("CustomerName")) ? null : reader["CustomerName"].ToString(),
                                Closed = reader.IsDBNull(reader.GetOrdinal("Closed")) ? (int?)null : Convert.ToInt32(reader["Closed"]),
                                ClosedDT = reader.IsDBNull(reader.GetOrdinal("ClosedDT")) ? (DateTime?)null : Convert.ToDateTime(reader["ClosedDT"]),
                                AudtTime = reader.IsDBNull(reader.GetOrdinal("AUDTTime")) ? null : Convert.ToDateTime(reader["AUDTTime"]),
                                AudtUser = reader.IsDBNull(reader.GetOrdinal("AUDTUser")) ? null : reader["AUDTUser"].ToString(),
                                ORDDate = reader.IsDBNull(reader.GetOrdinal("ORDDATE")) ? (DateTime?)null : Convert.ToDateTime(reader["ORDDATE"]),
                                DueDate = reader.IsDBNull(reader.GetOrdinal("DueDate")) ? (DateTime?)null : Convert.ToDateTime(reader["DueDate"]),
                                SONUM = reader.IsDBNull(reader.GetOrdinal("SONUM")) ? null : reader["SONUM"].ToString(),
                                TestNumber = reader.IsDBNull(reader.GetOrdinal("TestNumber")) ? null : reader["TestNumber"].ToString(),
                                NotForTest = reader.IsDBNull(reader.GetOrdinal("NotForTest")) ? 0 : Convert.ToInt32(reader["NotForTest"]),
                                CustomParam = reader.IsDBNull(reader.GetOrdinal("CustomParam")) ? null : reader["CustomParam"].ToString(),
                                Channel = reader.IsDBNull(reader.GetOrdinal("Channel")) ? null : reader["Channel"].ToString()

                            });

                        }
                    }

                }

            }
            return list;

        }

        public async Task<TestHeader> GetItemdata(int HeaderId)
        {
            TestHeader data = new TestHeader();
            using(SqlConnection conn = new SqlConnection(utils.sqlServerConnection))
            {
                await conn.OpenAsync();
                string query = @"SELECT 
                                HeaderId,ORDUNIQ,ReferenceNumber,Item,ItemDesc,RatedMaxInflationPSI,DeflatedOrInternalDiaInches,MaxInflatedDiaInches,Customer,CustomerName,Closed,ClosedDT,AUDTTime,AUDTUser,
                            CASE WHEN ISDATE(CONVERT(varchar(8), ORDDATE)) = 1 THEN CONVERT(datetime, CONVERT(varchar(8), ORDDATE), 112) ELSE NULL END AS ORDDATE,
                            CASE WHEN ISDATE(CONVERT(varchar(8), DueDate)) = 1 THEN CONVERT(datetime, CONVERT(varchar(8), DueDate), 112) ELSE NULL END AS DueDate,
                            SONUM,TestNumber,NotForTest,CustomParam,Channel
                                from PETERSENTESTING.dbo.TestHeader where HeaderId=@HeaderId";
                using(SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@HeaderId", HeaderId);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync()) {
                        if (await reader.ReadAsync()) {
                            data = new TestHeader()
                            {
                                HeaderId = Convert.ToInt32(reader["HeaderID"]),
                                ORDUNIQ = Convert.ToInt32(reader["ORDUNIQ"]),
                                ReferenceNumber = reader["ReferenceNumber"].ToString(),
                                Item = reader.IsDBNull(reader.GetOrdinal("Item")) ? null : reader["Item"].ToString(),
                                ItemDesc = reader.IsDBNull(reader.GetOrdinal("ItemDesc")) ? null : reader["ItemDesc"].ToString(),
                                RatedMaxInflationPSI = reader.IsDBNull(reader.GetOrdinal("RatedMaxInflationPSI")) ? null : reader["RatedMaxInflationPSI"].ToString(),
                                DeflatedOrInternalDiaInches = reader.IsDBNull(reader.GetOrdinal("DeflatedOrInternalDiaInches")) ? null : reader["DeflatedOrInternalDiaInches"].ToString(),
                                MaxInflatedDiaInches = reader.IsDBNull(reader.GetOrdinal("MaxInflatedDiaInches")) ? null : reader["MaxInflatedDiaInches"].ToString(),
                                Customer = reader.IsDBNull(reader.GetOrdinal("Customer")) ? null : reader["Customer"].ToString(),
                                CustomerName = reader.IsDBNull(reader.GetOrdinal("CustomerName")) ? null : reader["CustomerName"].ToString(),
                                Closed = reader.IsDBNull(reader.GetOrdinal("Closed")) ? (int?)null : Convert.ToInt32(reader["Closed"]),
                                ClosedDT = reader.IsDBNull(reader.GetOrdinal("ClosedDT")) ? (DateTime?)null : Convert.ToDateTime(reader["ClosedDT"]),
                                AudtTime = reader.IsDBNull(reader.GetOrdinal("AUDTTime")) ? null : Convert.ToDateTime(reader["AUDTTime"]),
                                AudtUser = reader.IsDBNull(reader.GetOrdinal("AUDTUser")) ? null : reader["AUDTUser"].ToString(),
                                ORDDate = reader.IsDBNull(reader.GetOrdinal("ORDDATE")) ? (DateTime?)null : Convert.ToDateTime(reader["ORDDATE"]),
                                DueDate = reader.IsDBNull(reader.GetOrdinal("DueDate")) ? (DateTime?)null : Convert.ToDateTime(reader["DueDate"]),
                                SONUM = reader.IsDBNull(reader.GetOrdinal("SONUM")) ? null : reader["SONUM"].ToString(),
                                TestNumber = reader.IsDBNull(reader.GetOrdinal("TestNumber")) ? null : reader["TestNumber"].ToString(),
                                NotForTest = reader.IsDBNull(reader.GetOrdinal("NotForTest")) ? 0 : Convert.ToInt32(reader["NotForTest"]),
                                CustomParam = reader.IsDBNull(reader.GetOrdinal("CustomParam")) ? null : reader["CustomParam"].ToString(),
                                Channel = reader.IsDBNull(reader.GetOrdinal("Channel")) ? null : reader["Channel"].ToString()
                            };

                        }
                    
                    }

                }

            }

            return data;

        }



        public async Task SyncTestDetail()
        {
            using(SqlConnection conn = new SqlConnection(utils.sqlServerConnection))
            {
                await conn.OpenAsync();
                using(SqlCommand cmd = new SqlCommand("SyncTestDetailsdata", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    await cmd.ExecuteNonQueryAsync();

                }
            }
        }

        public async Task<List<TestDetail_InflationPressure>> GetTestDetailInflationPressure(int headerId)
        {

            await SyncTestDetail();

            var list = new List<TestDetail_InflationPressure>();

            const string query = @" SELECT *
                                FROM PETERSENTESTING.dbo.TestDetail_InflationPressure
                                WHERE HeaderID = @HeaderID
                                ORDER BY TestDate, TestTime";

            using (var conn = new SqlConnection(utils.sqlServerConnection))
            {
                await conn.OpenAsync();

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@HeaderID", headerId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new TestDetail_InflationPressure
                            {
                                DetailID = Convert.ToInt32(reader["DetailID"]),
                                HeaderID = Convert.ToInt32(reader["HeaderID"]),
                                ReferenceNumber = reader["ReferenceNumber"].ToString(),
                                ORDUNIQ =  Convert.ToInt32(reader.GetDecimal(reader.GetOrdinal("ORDUNIQ"))),
                                Item = reader["Item"].ToString(),
                                TestDate = reader.IsDBNull(reader.GetOrdinal("TestDate"))? DateTime.Now: reader.GetDateTime(reader.GetOrdinal("TestDate")),
                                TestTime = reader.IsDBNull(reader.GetOrdinal("TestTime"))? DateTime.Now: reader.GetDateTime(reader.GetOrdinal("TestTime")),
                                StartTestDateTime = reader.IsDBNull(reader.GetOrdinal("StartTestDateTime"))? DateTime.Now: reader.GetDateTime(reader.GetOrdinal("StartTestDateTime")),
                                InflatedPSIG = reader.IsDBNull(reader.GetOrdinal("InflatedPSIG"))? 0m: reader.GetDecimal(reader.GetOrdinal("InflatedPSIG")),
                                InflationMedium = reader["InflationMedium"].ToString(),
                                AreaTempFahrenheit = reader.IsDBNull(reader.GetOrdinal("AreaTempFahrenheit"))? 0m: reader.GetDecimal(reader.GetOrdinal("AreaTempFahrenheit")),
                                Comments = reader["Comments"].ToString(),
                                AUDTUser = reader["AUDTUser"].ToString(),
                                AUDTTime = reader.IsDBNull(reader.GetOrdinal("AUDTTime"))? DateTime.Now: reader.GetDateTime(reader.GetOrdinal("AUDTTime")),
                                TestNumber = reader["TestNumber"].ToString(),
                                Initial = reader["Initial"].ToString(),
                                TestEndDate = reader.IsDBNull(reader.GetOrdinal("TestEndDate"))? DateTime.Now: reader.GetDateTime(reader.GetOrdinal("TestEndDate")),
                                TestEndTime = reader.IsDBNull(reader.GetOrdinal("TestEndTime"))? DateTime.Now: reader.GetDateTime(reader.GetOrdinal("TestEndTime")),
                                TestStatus = reader["TestStatus"].ToString(),
                                FailureReason = reader["FailureReason"].ToString(),
                                EndTestDateTime = reader.IsDBNull(reader.GetOrdinal("EndTestDateTime"))? null : reader.GetDateTime(reader.GetOrdinal("EndTestDateTime"))
                            });
                        }
                    }
                }
            }

            return list;
        }


        public async Task<TestDetail_InflationPressure> GetItemInflationPressure(int DetailID)
        {

            TestDetail_InflationPressure data = new TestDetail_InflationPressure();

            const string query = @" SELECT *
                                FROM PETERSENTESTING.dbo.TestDetail_InflationPressure
                                WHERE DetailID=@DetailID";

            using (var conn = new SqlConnection(utils.sqlServerConnection))
            {
                await conn.OpenAsync();

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DetailID", DetailID);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            data = (new TestDetail_InflationPressure
                            {
                                DetailID = Convert.ToInt32(reader["DetailID"]),
                                HeaderID = Convert.ToInt32(reader["HeaderID"]),
                                ReferenceNumber = reader["ReferenceNumber"].ToString(),
                                ORDUNIQ = Convert.ToInt32(reader.GetDecimal(reader.GetOrdinal("ORDUNIQ"))),
                                Item = reader["Item"].ToString(),
                                TestDate = reader.IsDBNull(reader.GetOrdinal("TestDate")) ? DateTime.Now : reader.GetDateTime(reader.GetOrdinal("TestDate")),
                                TestTime = reader.IsDBNull(reader.GetOrdinal("TestTime")) ? DateTime.Now : reader.GetDateTime(reader.GetOrdinal("TestTime")),
                                StartTestDateTime = reader.IsDBNull(reader.GetOrdinal("StartTestDateTime")) ? DateTime.Now : reader.GetDateTime(reader.GetOrdinal("StartTestDateTime")),
                                InflatedPSIG = reader.IsDBNull(reader.GetOrdinal("InflatedPSIG")) ? 0m : reader.GetDecimal(reader.GetOrdinal("InflatedPSIG")),
                                InflationMedium = reader["InflationMedium"].ToString(),
                                AreaTempFahrenheit = reader.IsDBNull(reader.GetOrdinal("AreaTempFahrenheit")) ? 0m : reader.GetDecimal(reader.GetOrdinal("AreaTempFahrenheit")),
                                Comments = reader["Comments"].ToString(),
                                AUDTUser = reader["AUDTUser"].ToString(),
                                AUDTTime = reader.IsDBNull(reader.GetOrdinal("AUDTTime")) ? DateTime.Now : reader.GetDateTime(reader.GetOrdinal("AUDTTime")),
                                TestNumber = reader["TestNumber"].ToString(),
                                Initial = reader["Initial"].ToString(),
                                TestEndDate = reader.IsDBNull(reader.GetOrdinal("TestEndDate")) ? DateTime.Now : reader.GetDateTime(reader.GetOrdinal("TestEndDate")),
                                TestEndTime = reader.IsDBNull(reader.GetOrdinal("TestEndTime")) ? DateTime.Now : reader.GetDateTime(reader.GetOrdinal("TestEndTime")),
                                TestStatus = reader["TestStatus"].ToString(),
                                FailureReason = reader["FailureReason"].ToString(),
                                EndTestDateTime = reader.IsDBNull(reader.GetOrdinal("EndTestDateTime")) ? null : reader.GetDateTime(reader.GetOrdinal("EndTestDateTime")),
                                AUDUser = reader.IsDBNull(reader.GetOrdinal("AUDUser")) ? "0e5640ac-c5d0-4900-b12b-c84760332716" : reader.GetOrdinal("AUDUser").ToString(),
                            });
                        }
                    }
                }
            }

            return data;
        }


        public async Task<List<PetersenUser>> GetAllValidUser()
        {
            List<PetersenUser> list = new List<PetersenUser>();
            using (SqlConnection conn = new SqlConnection(utils.sqlServerConnection))
            {
                await conn.OpenAsync();
                string query = "SELECT * FROM DATAMIRROR.dbo.petersen_users";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync()) {
                        
                        while (await reader.ReadAsync()){

                            list.Add(new PetersenUser
                            {
                                UserId = reader["user_id"].ToString(),
                                Name = reader["name"].ToString(),
                                EmployeeId = reader["EmployeeID"] == DBNull.Value ? string.Empty : reader["EmployeeID"].ToString(),
                                Email = reader["email"].ToString(),
                                petersen_testing = Convert.ToInt32(reader["petersen_testing"])

                            });

                        }
                    
                    }


                }
            }
            return list;

        }





    }
}
