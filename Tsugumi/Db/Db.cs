using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using System.Threading.Tasks;

namespace Tsugumi.Db
{
    public class Db
    {
        public Db()
        {
            R = RethinkDB.R;
        }

        public async Task InitAsync(string dbName)
        {
            this.dbName = dbName;
            conn = await R.Connection().ConnectAsync();
            if (!await R.DbList().Contains(dbName).RunAsync<bool>(conn))
                R.DbCreate(dbName).Run(conn);
            if (!await R.Db(dbName).TableList().Contains("Relations").RunAsync<bool>(conn))
                R.Db(dbName).TableCreate("Relations").Run(conn);
        }

        private RethinkDB R;
        private Connection conn;
        private string dbName;
    }
}
