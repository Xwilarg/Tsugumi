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
            if (!await R.Db(dbName).TableList().Contains("Version").RunAsync<bool>(conn))
            {
                R.Db(dbName).TableCreate("Version").Run(conn);
                string[] versions = await FateGOModule.GetVersions();
                await R.Db(dbName).Table("Version").Insert(R.HashMap("id", "1")
                    .With("android", versions[0])
                    .With("ios", versions[1])
                    ).RunAsync(conn);
            }
            if (!await R.Db(dbName).TableList().Contains("Relations").RunAsync<bool>(conn))
            {
                R.Db(dbName).TableCreate("Relations").Run(conn);
                foreach (var elem in await FateGOModule.GetAllServantRelations())
                {
                    await R.Db(dbName).Table("Relations").Insert(R.HashMap("id", FateGOModule.GetId(elem.Key))
                        .With("name", elem.Key)
                        .With("relations", string.Join(",", elem.Value))
                        ).RunAsync(conn);
                }
            }
        }

        private RethinkDB R;
        private Connection conn;
        private string dbName;
    }
}
