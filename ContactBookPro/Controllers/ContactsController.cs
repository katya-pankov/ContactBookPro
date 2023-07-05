using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContactBookPro.Data;
using ContactBookPro.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using ContactBookPro.Enums;
using ContactBookPro.Services.Interfaces;
using ContactBookPro.Services;
using System.ComponentModel.DataAnnotations;
using ContactBookPro.Models.ViewModels;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ContactBookPro.Controllers
{
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IImageService _imageService;
        private readonly IAddressBookService _addressBookService;
        private readonly IEmailSender _emailService;

        public ContactsController(ApplicationDbContext context, UserManager<AppUser> userManager,
                                   IImageService imageService, IAddressBookService addressBookService,
                                    IEmailSender emailService)
        {
            _context = context;
            _userManager = userManager;
            _imageService = imageService;
            _addressBookService = addressBookService;
            _emailService = emailService;
        }

        // GET: Contacts
        [Authorize]
        public IActionResult Index(int categoryId, string swalMessage = null)
        {
            ViewData["SwalMessage"] = swalMessage;
            var contacts = new List<Contact>();
            string appUserId = _userManager.GetUserId(User);


            //return the userId and its associated contacts and categroies;
            AppUser appUser = _context.Users
                                      //this is looking at the Icollection Contacts on the AppUser Model
                                      .Include(c => c.Contacts)
                                      //this is looking at the Icollection categories on the contacts model. ThenInclude 
                                      //references the model in the previous include statement which in this case is 
                                      //contacts.
                                      .ThenInclude(c => c.Categories)
                                      //takes the first match
                                      .FirstOrDefault(u => u.Id == appUserId)!;

            var categories = appUser.Categories;
            // if it's 0 return All contacts (the first item from the dropdown)
            if (categoryId == 0)
            {
                contacts = appUser.Contacts.OrderBy(c => c.LastName)
                                           .ThenBy(c => c.FirstName)
                                           .ToList();
            }
            // if it's anything other than 0
            else
            {
                // tfiter categories by Id, order them and return the list
                contacts = appUser.Categories.FirstOrDefault(c => c.Id == categoryId)
                                   .Contacts.OrderBy(c => c.LastName)
                                   .ThenBy(c => c.FirstName)
                                   .ToList();
            }



            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", categoryId);
            // return contacts model to the view
            return View(contacts);
        }


        [Authorize]
        public IActionResult SearchContacts(string searchString)
        {
            string appUserId = _userManager.GetUserId(User);
            var contacts = new List<Contact>();

            AppUser appUser = _context.Users
                .Include(c => c.Contacts)
                .ThenInclude(c => c.Categories)
                .FirstOrDefault(u => u.Id == appUserId);

            if (String.IsNullOrEmpty(searchString))
            {
                contacts = appUser.Contacts
                    .OrderBy(c => c.LastName)
                    .ThenBy(c => c.FirstName)
                    .ToList();
            }
            else
            {
                contacts = appUser.Contacts.Where(c => c.FullName!.ToLower().Contains(searchString.ToLower()))
                    .OrderBy(c => c.LastName)
                    .ThenBy(c => c.FirstName)
                    .ToList();
            }

            ViewData["CategoryId"] = new SelectList(appUser.Categories, "Id", "Name", 0);

            return View(nameof(Index), contacts);

        }

        [Authorize]
        public async Task<IActionResult> EmailContact(int id)
        {
            //get our current user from our user Manager
            string appUserId = _userManager.GetUserId(User);
            Contact contact = await _context.Contacts.Where(c => c.Id == id && c.AppUserID == appUserId)
                                                        .FirstOrDefaultAsync();  
            if(contact == null)
            {
                return NotFound();
            }

            EmailData emailData = new EmailData()
            {
                EmailAddress = contact.Email,
                FirstName = contact.FirstName,
                LastName = contact.LastName

            };

            EmailContactViewModel model = new EmailContactViewModel()
            {
                Contact = contact,
                EmailData = emailData,
            }; 
                                                        
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EmailContact(EmailContactViewModel ecvm)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    await _emailService.SendEmailAsync(ecvm.EmailData.EmailAddress, ecvm.EmailData.Subject, ecvm.EmailData.Body);
                    return RedirectToAction("Index", "Contacts", new { swalMessage = "Success: Email Sent!"});
                }
                catch
                {
                    return RedirectToAction("Index", "Contacts", new { swalMessage = "Error: Email hasn't been sent!" });
                    throw;
                }
               
            }
            return View(ecvm);
        }

        
        
        // GET: Contacts/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // GET: Contacts/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            string appUserId = _userManager.GetUserId(User);

            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            ViewData["CategoryList"] = new MultiSelectList(await _addressBookService.GetUserCategoriesAsync(appUserId), "Id", "Name");
            return View();
        }

        // POST: Contacts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,BirthDate,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,ImageFile")] Contact contact, List<int> CategoryList)
        {
            ModelState.Remove("AppUserId");
            ModelState.Remove("Created");

            if (ModelState.IsValid)
            {
                contact.AppUserID = _userManager.GetUserId(User);
                //contact.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                contact.Created = DateTime.UtcNow;

                if (contact.BirthDate != null)
                {
                    contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc);
                }

                if (contact.ImageFile != null)
                {
                    contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                    contact.ImageType = contact.ImageFile.ContentType;
                }

                _context.Add(contact);
                await _context.SaveChangesAsync();

                //loop over all the selected categories
                foreach (int categoryId in CategoryList)
                {
                    await _addressBookService.AddContactToCategoryAsync(categoryId, contact.Id);

                }
                //save each category selected to the contactcategories table


                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Contacts/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            string appUserId = _userManager.GetUserId(User);


            // var contact = await _context.Contacts.FindAsync(id);
            var contact = await _context.Contacts.Where(c =>  c.Id == id && c.AppUserID == appUserId)
                                                 .FirstOrDefaultAsync();
            if (contact == null)
            {
                return NotFound();
            }
            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            ViewData["CategoryList"] = new MultiSelectList(await _addressBookService.GetUserCategoriesAsync(appUserId), "Id", "Name", await _addressBookService.GetContactCategoryIdsAsync(contact.Id));
            return View(contact);
        }

        // POST: Contacts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppUserID,FirstName,LastName,BirthDate,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,Created,ImageFile,ImageData,ImageType")] Contact contact, List<int> CategoryList)
        {
            if (id != contact.Id)
            {
                return NotFound();
            }
            // if all the values are valid
            if (ModelState.IsValid)
            {
                try
                {
                    // we need to properly format our date time so it's saved to PostgreSQL accepted format
                    if (contact.Created != null)
                    {
                        contact.Created = DateTime.SpecifyKind(contact.Created.Value, DateTimeKind.Utc);
                    }
                    //contact.Created = DateTime.SpecifyKind((DateTime)contact.Created, DateTimeKind.Utc);

                    if(contact.BirthDate != null)
                    {
                        contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc);
                    }

                    if (contact.ImageFile != null)
                    {
                        contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                        contact.ImageType = contact.ImageFile.ContentType;
                    }
                    _context.Update(contact);
                    await _context.SaveChangesAsync();

                    //save our categories
                    //remove the current categories
                    List<Category> oldCategories = (await _addressBookService.GetContactCategoriesAsync(contact.Id)).ToList();
                    //add the selected categories
                    foreach (var category in oldCategories)
                    {
                        await _addressBookService.RemoveContactFromCategoryASync(category.Id, contact.Id);
                    }

                    //add the selected categories
                    foreach(int categoryId in CategoryList)
                    {
                        await _addressBookService.AddContactToCategoryAsync(categoryId, contact.Id);
                    }

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AppUserID"] = new SelectList(_context.Users, "Id", "Id", contact.AppUserID);
            return View(contact);
        }

        // GET: Contacts/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            //get appUser Id
            string appUserId = _userManager.GetUserId(User);

            //you must be an owner of the account to delete any contacts
            var contact = await _context.Contacts
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(c => c.Id == id && c.AppUserID == appUserId);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //get appUser Id
            string appUserId = _userManager.GetUserId(User);

            var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id && c.AppUserID == appUserId);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync();
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContactExists(int id)
        {
            return (_context.Contacts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
