using MySql.Data.MySqlClient;
namespace cumulative01.Models
{
    public class SchoolDbContext
    {

        // These are all the readonly secret properties
        // Only the SchoolDb can use them

        private static string User { get { return "root"; } }
        private static string Password { get { return ""; } }
        private static string Database { get { return "school"; } }
        private static string Server { get { return "localhost"; } }
        private static string Port { get { return "3306"; } }

        //ConnectionString is a series of credentials used to connect to the database.
        protected static string ConnectionString
        {
            get
            {
                // convert zero datetime is a db connection setting which returns NULL if the date is 0000-00-00
                // this can allow C# to have an easier interpretation of the date (no date instead of 0 BCE)

                return "server = " + Server
                    + "; user = " + User
                    + "; database = " + Database
                    + "; port = " + Port
                    + "; password = " + Password
                    + "; convert zero datetime = True";
            }
        }
        // This is the method we actually use to get the database!
        /// <summary>
        /// Returns a connection to the school database.
        /// </summary>
        /// <example>
        /// private SchoolDbContext School = new SchoolDbContext();
        /// MySqlConnection Connection = School.AccessDatabase();
        /// </example>
        /// <returns>A MySqlConnection Object</returns>
        public MySqlConnection AccessDatabase()
        {
            // We are instantiating the MySqlConnection Class to create an object
            // the object is a specific connection to our school database on port 3306 of localhost
            return new MySqlConnection(ConnectionString);
        }


    }
}

