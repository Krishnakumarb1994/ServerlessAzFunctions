using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System;

namespace GetEmployeeFunction
{
    public static class GetEmployeeFunction
    {
        [FunctionName("GetEmployeeFunction")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("Get Employee function processing a request.");
            Mapper map = new Mapper();
            List<EmpModel> EmpList = new List<EmpModel>();
            var successful = false;
            try
            {
                dynamic data = await req.Content.ReadAsAsync<object>();
                using (var connection = map.connection())
                {
                    SqlCommand com = new SqlCommand("GetEmployees", connection);
                    com.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter da = new SqlDataAdapter(com);
                    DataTable dt = new DataTable();

                    connection.Open();
                    da.Fill(dt);
                    connection.Close();
                    foreach (DataRow dr in dt.Rows)
                    {
                        EmpList.Add(
                            new EmpModel
                            {
                                Empid = Convert.ToInt32(dr["Id"]),
                                FirstName = Convert.ToString(dr["FirstName"]),
                                LastName = Convert.ToString(dr["LastName"]),
                                Age = Convert.ToString(dr["Age"]),
                                DateofBirth = Convert.ToString(dr["DateofBirth"]),
                                Address = Convert.ToString(dr["Address"]),
                                IsActive = Convert.ToString(dr["IsActive"])
                            }
                            );
                    }
                    successful = true;
                }
            }
            catch(Exception e)
            {
                log.Error(e.InnerException.ToString());
                successful = false;
            }
            return !successful
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Unable to Fetch Record's from Database!")
                : req.CreateResponse(HttpStatusCode.OK, EmpList);
        }
    }

    public static class CreateEmployeeFunction
    {
        [FunctionName("CreateEmployeeFunction")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("Create Employee function processing a request.");

            var successful = false;
            try
            {
                dynamic data = await req.Content.ReadAsAsync<object>();
                EmpModel emp = new EmpModel();
                Mapper map = new Mapper();
                emp = map.DataMapper(data);
                successful = map.ExecuteSQLSP("AddNewEmpDetails", emp);
            }
            catch(Exception e)
            {
                log.Error(e.InnerException.ToString());
                successful = false;
            }

            return !successful
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Unable to insert employee into database!")
                : req.CreateResponse(HttpStatusCode.OK, "Employee Created successfully!");
        }
    }

    public static class UpdateEmployeeFunction
    {
        [FunctionName("UpdateEmployeeFunction")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("Update Employee function processing a request.");

            var successful = false;
            try
            {
                dynamic data = await req.Content.ReadAsAsync<object>();
                EmpModel emp = new EmpModel();
                Mapper map = new Mapper();
                emp = map.DataMapper(data);
                successful = map.ExecuteSQLSP("UpdateEmpDetails", emp);
            }
            catch(Exception e)
            {
                log.Error(e.InnerException.ToString());
                successful = false;
            }

            return !successful
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Unable to Update employee into database!")
                : req.CreateResponse(HttpStatusCode.OK, "Employee Updated successfully!");
        }
    }

    public static class DeleteEmployeeFunction
    {
        [FunctionName("DeleteEmployeeFunction")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("Delete Employee function processing a request.");
            Mapper map = new Mapper();
            var successful = false;
            try
            {
                string ID = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "ID", true) == 0)
                .Value;
                dynamic data = await req.Content.ReadAsAsync<object>();
                using (var connection = map.connection())
                {
                    SqlCommand com = new SqlCommand("DeleteEmpById", connection);
                    com.CommandType = CommandType.StoredProcedure;
                    com.Parameters.AddWithValue("@EmpId", ID);
                    connection.Open();
                    int i = com.ExecuteNonQuery();
                    if (i > 0)
                    { successful = true; }
                    connection.Close();
                }
            }
            catch(Exception e)
            {
                log.Error(e.InnerException.ToString());
                successful = false;
            }

            return !successful
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Unable to Delete Record from Database!")
                : req.CreateResponse(HttpStatusCode.OK, "Data Deleted successfully!");
        }
    }
    public static class GetEmployeeCount
    {
        [FunctionName("GetEmployeeCount")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("Get Employee function processing a request.");
            int value = 0;
            var successful = false;
            Mapper map = new Mapper();
            try
            {
                dynamic data = await req.Content.ReadAsAsync<object>();
                string stmt = "SELECT MAX(Id) FROM dbo.Employee";
                SqlConnection connection = map.connection();
                using (SqlCommand cmdCount = new SqlCommand(stmt, connection))
                {
                    connection.Open();
                    value = (int)cmdCount.ExecuteScalar();
                    connection.Close();
                }
                successful = true;
            }
            catch(Exception e)
            {
                log.Error(e.InnerException.ToString());
                successful = false;
            }

            return !successful
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Unable to fetch count Record from Database!")
                : req.CreateResponse(HttpStatusCode.OK, value);
        }
    }
    public class EmpModel
    {
        public int Empid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Age { get; set; }
        public string DateofBirth { get; set; }
        public string Address { get; set; }
        public string IsActive { get; set; }
    }

    public class Mapper
    {
        public EmpModel DataMapper(dynamic data)
        {
            EmpModel emp = new EmpModel();
            emp.Empid = data.Id != null ? data.Id : null;
            emp.FirstName = data.FirstName != null ? data.FirstName : null;
            emp.LastName = data.LastName != null ? data.LastName : null;
            emp.Age = data.Age != null ? data.Age : null;
            emp.DateofBirth = data.DateofBirth != null ? data.DateofBirth : null;
            emp.Address = data.Address != null ? data.Address : null;
            emp.IsActive = data.IsActive != null ? data.IsActive : null;
            return emp;
        }
        public SqlConnection connection()
        {
            string connectionString = null;
            SqlConnection conn;
            connectionString = "Server=tcp:cloudpocserver.database.windows.net,1433;Initial Catalog=azurepocdb;Persist Security Info=False;User ID=pocadmin;Password=database@1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30; ";
            conn = new SqlConnection(connectionString);
            return conn;
        }
        public bool ExecuteSQLSP(string procName, EmpModel emp)
        {
            bool successful = false;
            using (var conn = connection())
            {
                SqlCommand com = new SqlCommand(procName, conn);
                com.CommandType = CommandType.StoredProcedure;
                com.Parameters.AddWithValue("@EmpId", emp.Empid);
                com.Parameters.AddWithValue("@FirstName", emp.FirstName);
                com.Parameters.AddWithValue("@LastName", emp.LastName);
                com.Parameters.AddWithValue("@Age", emp.Age);
                com.Parameters.AddWithValue("@DateofBirth", emp.DateofBirth);
                com.Parameters.AddWithValue("@Address", emp.Address);
                com.Parameters.AddWithValue("@IsActive", emp.IsActive);
                conn.Open();
                int i = com.ExecuteNonQuery();
                conn.Close();
                if (i >= 1)
                { successful = true; }
                else
                { successful = false; }
            }
            return successful;
        }
    }
}
