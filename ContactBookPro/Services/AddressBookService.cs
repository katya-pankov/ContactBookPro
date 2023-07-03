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


        public async Task AddContactToCategoryAsync(int categoryId, int contactId)
        {
            try
            {
                //check to see if the category is in the contact already 
                if (!await IsContactInCategory(categoryId, contactId))
                {
                    Contact? contact = await _context.Contacts.FindAsync(contactId);
                    Category? category = await _context.Categories.FindAsync(categoryId);

                    if(category != null && contact != null)
                    {
                        category.Contacts.Add(contact);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception) 
            {
                throw;

            }
        }

        public async Task<ICollection<Category>> GetContactCategoriesAsync(int contactId)
        {
            try
            {
                Contact? contact = await _context.Contacts.Include(c => c.Categories).FirstOrDefaultAsync(c => c.Id == contactId);
                return contact.Categories;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<ICollection<int>> GetContactCategoryIdsAsync(int contactId)
        {
            try
            {
                //take the contact that is comining in. Make sure you include the categories associated with it.
                // filter that by contact Id
                var contact = await _context.Contacts.Include(c => c.Categories)
                                                    .FirstOrDefaultAsync(c => c.Id == contactId);

                // take the category class that comes back, filter that down to one column (id column). Return that as a list
                List<int> categoryIds = contact.Categories.Select(c => c.Id).ToList();
                return categoryIds;
            }
            catch (Exception)
            {
                throw;

            }
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

        public async Task<bool> IsContactInCategory(int categoryId, int contactId)
        {
            Contact? contact = await _context.Contacts.FindAsync(contactId);

            return await _context.Categories
                                 .Include(c => c.Contacts)
                                 .Where(c => c.Id == categoryId && c.Contacts.Contains(contact))
                                 .AnyAsync();
        }

        public Task<bool> IsContactInCategroy(int categoryId, int contactId)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveContactFromCategoryASync(int categoryId, int contactId)
        {
            try
            {
                //check if the category exists before we try to remove it
                if (await IsContactInCategory(categoryId, contactId))
                {
                    // we find the contact
                    Contact contact = await _context.Contacts.FindAsync(contactId);
                    // we find the category
                    Category category = await _context.Categories.FindAsync(categoryId);

                    // if they are nit null 
                    if ( category != null && contact != null) 
                    {
                        // go to category table and remove it from our contact
                        category.Contacts.Remove(contact);
                        await _context.SaveChangesAsync();
                        
                    }
                }
            }
            catch (Exception) 
            {
                throw;

            }

        }

        public IEnumerable<Contact> SearchForContacts(string searchString, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
