using Azure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SQL_Injection_Fix.Data;
using SQL_Injection_Fix.Entities;
using SQL_Injection_Fix.Migrations;
using System;
using System.Collections.Generic;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Net.Mime.MediaTypeNames;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SQL_Injection_Fix.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        // SQL injection is a type of security vulnerability that allows attackers to inject malicious SQL statements   
        // into a web application's input fields, with the aim of executing unauthorized SQL commands against a
        // database. It occurs when user input is not properly sanitized, and the input data is directly used in
        // constructing SQL queries to interact with the database. Attackers can exploit this vulnerability to bypass
        // authentication, retrieve sensitive data, modify or delete data, or even gain administrative access to the
        // database. Suppose we have a login form that takes a username and password and verifies them against a MSSQL
        // database:

        [HttpGet("{name}/{password}")]
        public string SignIn(string name, string password)
        {
            string result = String.Empty;

            using (ApplicationDbContext dbContext = new ApplicationDbContext())
            {
                #region Wrong Way

                // string query = "SELECT * FROM Users WHERE Name='" + name + "' AND Password='" + password + "'";

                //// In this example, the values of username and password are directly concatenated into the SQL query
                //// without any validation or sanitization. An attacker could exploit this vulnerability by entering a
                //// specially crafted input that modifies the intended SQL query to execute malicious commands, such as:
                //// username: admin' --
                //// password: anything

                //// This input would modify the SQL query to:
                //// SELECT* FROM users WHERE username = 'admin'--' AND password='anything'

                //// The double hyphens indicate that the rest of the query is treated as a comment and ignored by the
                //// database server, effectively bypassing the password check and logging in as the administrator.

                // var mssqlResult = dbContext.Users.FromSqlRaw(query).ToList();

                // if (mssqlResult.Count == 0)
                //     return "NOT FOUND";

                // result = "User found\n";

                // foreach (var entity in mssqlResult)
                // {

                //     result += entity.Name + " " + entity.Password + "\n\n";
                // }

                #endregion

                // To fix this vulnerability, we need to use parameterized queries / prepared statements or / stored
                // procedures, which separate the input data from the SQL query logic and prevent the input data from
                // being interpreted as executable code. Here's an example of how we could fix the code with different
                // ways:

                #region Parameterized Queries Way

                // var mssqlResult = dbContext.Users
                //     .FromSqlRaw("SELECT * FROM Users WHERE Name = {0} AND Password = {1}", name, password)
                //     .ToList();

                //// In this fixed code, the values of username and password are passed as parameters to the SQL query
                //// using the AddWithValue method, which ensures that the input data is properly sanitized and treated
                //// as plain text rather than executable code.

                // if (mssqlResult.Count == 0)
                //     return "NOT FOUND";

                // result = "User found\n";

                // foreach (var entity in mssqlResult)
                // {
                //     result += entity.Name + " " + entity.Password + "\n\n";
                // }

                #endregion

                #region Stored Procedures Way

                //// First, we would create a stored procedure in the database to handle the login logic:
                //// CREATE PROCEDURE login(@username VARCHAR(50), @password VARCHAR(50))
                //// AS
                //// BEGIN
                ////     SELECT* FROM Users WHERE Name = @username AND Password = @password;
                //// END
                //// Then, in our C# code, we would call the stored procedure and pass in the input data as parameters:

                // var mssqlResult = dbContext.Users.FromSqlRaw("EXECUTE login @param1, @param2",
                //         new SqlParameter("@param1", name),
                //         new SqlParameter("@param2", password)).ToList();

                //// In this fixed code, we're calling a stored procedure named login instead of directly executing a
                //// SQL query. The input data is passed as parameters to the stored procedure using the AddWithValue
                //// method, which ensures that the input data is properly sanitized and validated before being used in
                //// the SQL code

                // if (mssqlResult.Count == 0)
                //     return "NOT FOUND";

                // result = "User found\n";

                // foreach (var entity in mssqlResult)
                // {
                //     result += entity.Name + " " + entity.Password + "\n\n";
                // }

                #endregion

                #region Prepared Statement Way



                // We don't need to use command.Prepare() all the time, as it depends on the context and the frequency of
                // query execution. Here are two use cases where using prepared statements with command.Prepare() can be
                // beneficial:

                // 1. Batch processing: If you are executing the same query with different parameter values multiple times
                // within a loop or a batch processing operation, it can be more efficient to prepare the statement once
                // using command.Prepare() and then execute it multiple times with different parameter values.

                // 2. High - traffic applications: If your application is serving a large number of concurrent users and
                // executing the same queries frequently, using prepared statements with command.Prepare() can improve
                // performance by reducing the overhead of query compilation and optimization. In this case, you can prepare
                // the statement once during application startup and reuse the same command object with different parameter
                // values.


                // Create a DbParameter instance for the parameter
                var NameParameter =
                new SqlParameter("@Name", SqlDbType.NVarChar, 50 /* Size of NVarChar */);
                var Passwordarameter =
                new SqlParameter("@Password", SqlDbType.NVarChar, 50 /* Size of NVarChar */);
                NameParameter.Value = name;
                Passwordarameter.Value = password;
                using (var connection = dbContext.Database.GetDbConnection())
                {
                    connection.Open();

                    // Create the command and add parameters
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM Users WHERE Name = @Name AND Password = @Password";
                        command.Parameters.Add(NameParameter);
                        command.Parameters.Add(Passwordarameter);
                        command.Prepare();

                        using (var reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                                return "NOT FOUND";
                            while (reader.Read())
                            {
                                var user = new User
                                {
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Password = reader.GetString(reader.GetOrdinal("Password"))
                                };

                                result = user.Name + " " + user.Password;
                            }
                        }

                        // Change the parameter values and execute the prepared statement again
                        // NameParameter.Value = "Alice"; //// command.Parameters["@Name"].Value = "Jane Smith";
                        // Passwordarameter.Value = "password1";

                        // using (var reader = command.ExecuteReader())
                        // {
                        //     while (reader.Read())
                        //     {
                        //         var user = new User
                        //         {
                        //             Name = reader.GetString(reader.GetOrdinal("Name")),
                        //             Password = reader.GetString(reader.GetOrdinal("Password"))
                        //         };
                        //     }
                        // }

                    }
                }

                #endregion
            }
            return result;
        }

        [HttpGet]
        public string GetAllUsers()
        {
            string result = String.Empty;
            using (ApplicationDbContext dbContext = new ApplicationDbContext())
            {
                // Execute the query on the MSSQL database
                var mssqlResult = dbContext.Users.FromSqlRaw("SELECT * FROM Users").ToList();
                foreach (var entity in mssqlResult)
                {
                    result += entity.Name + " " + entity.Password + "\n\n";
                }
            }
            return result;
        }


        // POST api/<UserController>
        [HttpPost]
        public async void Post([FromBody] string value)
        {

            List<User> users = new List<User>() {
                new User("Alice", "password1"),
                new User("Bob", "password2"),
                new User("Charlie", "password3"),
                new User("Admin", "admin123")
            };

            using (ApplicationDbContext dbContext = new ApplicationDbContext())
            {
                if (dbContext.Users.Any())
                {
                    dbContext.Users.RemoveRange(dbContext.Users.ToList());
                }

                foreach (var user in users)
                {
                    dbContext.Users.Add(user);
                }

                dbContext.SaveChanges();
            }

        }
    }
}
