using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using System;
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
            if (!await R.Db(dbName).TableList().Contains("Relation").RunAsync<bool>(conn))
            {
                R.Db(dbName).TableCreate("Relation").Run(conn);
                foreach (var elem in await FateGOModule.GetAllServantRelations())
                {
                    await R.Db(dbName).Table("Relation").Insert(R.HashMap("id", FateGOModule.GetId(elem.Key))
                        .With("name", elem.Key)
                        .With("relations", string.Join(",", elem.Value))
                        ).RunAsync(conn);
                }
            }
        }

        public async Task UpdateDb()
        {
            string[] versions = await FateGOModule.GetVersions();
            await R.Db(dbName).Table("Version").Update(R.HashMap("id", "1")
                .With("android", versions[0])
                .With("ios", versions[1])
                ).RunAsync(conn);
            foreach (var elem in await FateGOModule.GetAllServantRelations())
            {
                await R.Db(dbName).Table("Relation").Update(R.HashMap("id", FateGOModule.GetId(elem.Key))
                    .With("name", elem.Key)
                    .With("relations", string.Join(",", elem.Value))
                    ).RunAsync(conn);
            }
        }

        public async Task<bool> AreVersionSame()
        {
            string[] currVersions = await FateGOModule.GetVersions();
            dynamic versions = await R.Db(dbName).Table("Version").Get("1").RunAsync(conn);
            return ((string)versions.android == currVersions[0] && (string)versions.ios == currVersions[1]);
        }

        private RethinkDB R;
        private Connection conn;
        private string dbName;
    }
}
