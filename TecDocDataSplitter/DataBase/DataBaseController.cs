using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TecDocDataSplitter.DataBase
{
    public class DataBaseController
    {
        private readonly MySqlConnection _MyConnection;

        public DataBaseController(MySqlConnection connection)
        {
            this._MyConnection = (MySqlConnection)connection.Clone();
        }

        public DataBaseController(string connectionString)
            :this(new MySqlConnection(connectionString))
        {
            
        }

        ~DataBaseController()
        {
            if(this._MyConnection != null)
            {
                this._MyConnection.Close();
                this._MyConnection.Dispose();
            }

            GC.Collect(2, GCCollectionMode.Forced);
        }
    }
}
