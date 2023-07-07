using ContactBookPro.Services.Interfaces;

namespace ContactBookPro.Services
{
    public class ImageService : IImageService
    {
        // suffixes to describe the byte array
        private readonly string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" };
        // if we don't find an image we will display the dafault image to the user
        private readonly string defaultImage = "img/DefaultContactImage.png";
        public string ConvertByteArrayToFile(byte[] fileData, string extension)
        {
            //if the file is already in the database, we need to create an appropriate image tag that the user can use
            if (fileData == null) return defaultImage;

            try
            {
                //use buil-in function to convert fileData to base64
                string imageBase64Data = Convert.ToBase64String(fileData);
                //return a string that is a valid image tag for the image
                return string.Format($"data:{extension};base64,{imageBase64Data}");
            }
            catch (Exception) 
            {
                throw;
            }
        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            try 
            {
                //create a new instance of the MemoryStream class using the using statement. The MemoryStream class is used to store the file contents in memory.
                using MemoryStream memoryStream = new();
                // asynchronously copy the contents of the file (which is an IFormFile representing the uploaded file)
                // to the memoryStream. It reads the file data and writes it to the MemoryStream.
                await file.CopyToAsync(memoryStream);
                //convert the content of the memoryStream to a byte array by calling the ToArray() method.
                //The byte array represents the file content in the form of binary data.
                byte[] byteFile = memoryStream.ToArray();
                //returns the resulting byte array representing the file content.
                return byteFile;
            }
            catch (Exception)
            { 
                throw; 
            }
        }
    }
}
