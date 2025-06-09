using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionAxion.Repositories
{
    public interface IDatabaseProvider
    {
        IDbConnection OpenConnection();
        void InitializeDatabase();
        DataTable ExecuteSelect(string query);
        int ExecuteNonQuery(string query);
        bool ExecuteScalarAsBool(string query);
        void CloseConnection();
    }
}
