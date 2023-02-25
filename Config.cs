using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instagram
{
    internal class Config
    {
        public MySqlConnection GetConnection(string userId, string password, string database)
        {
            string connectionString = $"server=localhost;user id={userId};password={password};database={database};";

            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connectionString);
                conn.Open();
            }
            catch (MySqlException e)
            {
                e.GetBaseException();
            }

            return conn;
        }

        public MySqlConnection GetStandardConnection()
        {
            string connectionString = $"server=192.168.1.1;user id=username;password=password;database=images;";

            MySqlConnection conn = null;
            try
            {
                conn = new MySqlConnection(connectionString);
                conn.Open();
            }
            catch (MySqlException e)
            {
                e.GetBaseException();
            }

            return conn;
        }
    }
}
