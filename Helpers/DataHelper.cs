using ContactBookPro.Data;
using Microsoft.EntityFrameworkCore;

namespace ContactBookPro.Helpers
{
    public static class DataHelper
    {
        //inject IService provider 
        public static async Task ManageDataAsync(IServiceProvider svcProvider)
        {
            //get an instance of the db application context
            var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();

            //migration: this is equivalent to update-databse
            await dbContextSvc.Database.MigrateAsync();

        }
    }
}
