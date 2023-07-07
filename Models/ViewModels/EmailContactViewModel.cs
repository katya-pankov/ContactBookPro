namespace ContactBookPro.Models.ViewModels
{
    //this class holds the contact and whatever was in the email
    public class EmailContactViewModel
    {
        public Contact? Contact { get; set; }
        public EmailData? EmailData { get; set; }
    }
}
