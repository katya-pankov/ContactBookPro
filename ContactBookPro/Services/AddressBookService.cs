using ContactBookPro.Data;
using ContactBookPro.Models;
using ContactBookPro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContactBookPro.Services
{
    public class AddressBookService : IAddressBookService
    {
        private readonly ApplicationDbContext _context;

        public AddressBookService(ApplicationDbContext context)
        {
            _context = context;
        }


        public Task AddContactToCategoryAsync(int categoryId, int contactId)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<Category>> GetContactCategoriesAsync(int contactId)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<int>> GetContactCategoryIdsAsync(int contactId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Category>> GetUserCategoriesAsync(string userId)
        {
            //make a blank list of categories
            List<Category> categories = new List<Category>();
            try
            {
                categories = await _context.Categories.Where(c => c.AppUserId == userId)
                                                .OrderBy(c => c.Name)
                                                .ToListAsync();
            }
            catch
            {
                throw;
            }

            return categories;
        }

        public Task<bool> IsContactInCategory(int categoryId, int contactId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsContactInCategroy(int categoryId, int contactId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveContactFromCategoryASync(int categoryId, int contactId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Contact> SearchForContacts(string searchString, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
