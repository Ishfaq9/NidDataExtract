using IronOcr;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NidDataExtract_3.Models;
using System;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text;

namespace NidDataExtract_3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;
        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        //[HttpPost]
        //public async Task<IActionResult> ProcessNID(IFormFile nidImage)
        //{
        //    if (nidImage == null || nidImage.Length == 0)
        //        return BadRequest("Invalid image");

        //    // You can add OCR logic here later (e.g., send to Python OCR API or Tesseract.NET)
        //    await Task.Delay(5000); // Simulating processing time

        //    var result = new NidImageResult
        //    {
        //        নাম = "মোঃ রফিকুল ইসলাম",
        //        Name = "MD RAFIQUL ISLAM",
        //        পিতা = "মোঃ আব্দুল হালিম",
        //        মাতা = "মোছাঃ সেলিনা ইসলাম",
        //        স্বামী = "মোঃ রফিকুল ইসলাম",
        //        স্ত্রী = "মোছাঃ সেলিনা ইসলাম",
        //        DateOfBirth = "1985-01-01",
        //        IDNO = "1998876543210"
        //    };


        //    return Json(result);
        //}

        /// 2nd method to process NID image all okay

        //[HttpPost]
        //public async Task<IActionResult> ProcessNID(IFormFile nidImage)
        //{
        //    if (nidImage == null || nidImage.Length == 0)
        //        return BadRequest("Invalid image");

        //    byte[] imageBytes;
        //    using (var ms = new MemoryStream())
        //    {
        //        await nidImage.CopyToAsync(ms);
        //        imageBytes = ms.ToArray();
        //    }

        //    using var client = new HttpClient();
        //    var content = new ByteArrayContent(imageBytes);
        //    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

        //    var response = await client.PostAsync("https://localhost:44301/api/NIDImageDataExtract/GetDataFromBytes", content);

        //    if (!response.IsSuccessStatusCode)
        //        return BadRequest("OCR service failed.");

        //    var responseContent = await response.Content.ReadAsStringAsync();
        //    dynamic result = JsonConvert.DeserializeObject<Response>(responseContent);

        //    var NidImageResult = new NidImageResult
        //    {
        //        নাম = result.ObjResponse.নাম,
        //        Name = result.ObjResponse.name,
        //        পিতা = result.ObjResponse.পিতা,
        //        মাতা = result.ObjResponse.মাতা,
        //        স্বামী = result.ObjResponse.স্বামী,
        //        স্ত্রী = result.ObjResponse.স্ত্রী,
        //        DateOfBirth = result.ObjResponse.dateOfBirth,
        //        IDNO = result.ObjResponse.idno

        //    };

        //    return Json(NidImageResult);
        //}

        //[HttpPost]
        //public async Task<IActionResult> ProcessNID([FromBody] byte[] imageBytes, [FromQuery] string version)
        //{
        //    if (imageBytes == null || imageBytes.Length == 0)
        //        return BadRequest("Invalid image data");

        //    try
        //    {
        //        // Save the image to wwwroot/uploads/
        //        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        //        if (!Directory.Exists(uploadsFolder))
        //            Directory.CreateDirectory(uploadsFolder);

        //        string fileName = Guid.NewGuid().ToString() + ".jpg"; // Unique file name
        //        string filePath = Path.Combine(uploadsFolder, fileName);

        //        await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

        //        // Call OCR processing
        //        dynamic result = await RunOCRWithImagePath(filePath, version);

        //        if (!result.IsSuccess)
        //            return BadRequest(result.Message);

        //        // Prepare response
        //        var NidImageResult = new NidImageResult
        //        {
        //            নাম = result.ObjResponse.নাম,
        //            Name = result.ObjResponse.Name,
        //            পিতা = result.ObjResponse.পিতা,
        //            মাতা = result.ObjResponse.মাতা,
        //            স্বামী = result.ObjResponse.স্বামী,
        //            স্ত্রী = result.ObjResponse.স্ত্রী,
        //            DateOfBirth = result.ObjResponse.DateOfBirth,
        //            IDNO = result.ObjResponse.IDNO
        //        };

        //        // Handle fallback for spouse fields
        //        if (NidImageResult.স্বামী == "No data found")
        //            NidImageResult.স্বামী = NidImageResult.স্ত্রী;
        //        if (NidImageResult.স্ত্রী == "No data found")
        //            NidImageResult.স্ত্রী = NidImageResult.স্বামী;

        //        return Json(NidImageResult);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Server error: {ex.Message}");
        //    }
        //}



        [HttpPost]
        public async Task<IActionResult> ProcessNID(IFormFile nidImage, string version)
        {
            if (nidImage == null || nidImage.Length == 0)
                return BadRequest("Invalid image");

            try
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string fileName = Path.GetFileName(nidImage.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create)) 
                {
                    await nidImage.CopyToAsync(stream);
                }

                //bool result2 = IronOcr.License.IsValidLicense("IRONSUITE.ISHFAQ.RAHMAN9.GMAIL.COM.19895-29B53E8CD7-A4PU5XP-U62D7SSEJT4V-3UZN5H3WTUJC-YNDHDDNBZBJ6-IGVC56X6G4KQ-E5DCEKJJHF3Y-CQ6GHRY3D2TR-UOZTV3-TYBYZWUUFTWPUA-DEPLOYMENT.TRIAL-EVKLAP.TRIAL.EXPIRES.16.JUL.2025");
                //IronOcr.License.LicenseKey = "IRONSUITE.ISHFAQ.RAHMAN9.GMAIL.COM.19895-29B53E8CD7-A4PU5XP-U62D7SSEJT4V-3UZN5H3WTUJC-YNDHDDNBZBJ6-IGVC56X6G4KQ-E5DCEKJJHF3Y-CQ6GHRY3D2TR-UOZTV3-TYBYZWUUFTWPUA-DEPLOYMENT.TRIAL-EVKLAP.TRIAL.EXPIRES.16.JUL.2025";
                //var Ocr = new IronTesseract();

                //Ocr.Language =OcrLanguage.English;

                //string extractedText;
                //using (var input = new OcrInput(filePath))
                //{
                //    input.ToGrayScale();
                //    //input.Contrast();
                //    //input.EnhanceResolution();
                //    //input.Deskew();
                //    //input.Rotate(0);         
                //    input.DeNoise();


                //    var result1 = Ocr.Read(input);
                //    extractedText = result1.Text;
                //}

                //var result1 = Ocr.Read(filePath);
                //string text = result1.Text;

                dynamic result = await RunOCRWithImagePath(filePath, version);
                if (!result.IsSuccess)
                    return BadRequest(result.Message);


                var NidImageResult = new NidImageResult
                {
                    নাম = result.ObjResponse.নাম,
                    Name = result.ObjResponse.Name,
                    পিতা = result.ObjResponse.পিতা,
                    মাতা = result.ObjResponse.মাতা,
                    স্বামী = result.ObjResponse.স্বামী,
                    স্ত্রী = result.ObjResponse.স্ত্রী,
                    DateOfBirth = result.ObjResponse.DateOfBirth,
                    IDNO = result.ObjResponse.IDNO

                };
                if(NidImageResult.স্বামী == "No data found")
                    NidImageResult.স্বামী = NidImageResult.স্ত্রী;
                if (NidImageResult.স্ত্রী == "No data found")
                    NidImageResult.স্বামী = NidImageResult.স্বামী;


                return Json(NidImageResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        private async Task<Response> RunOCRWithImagePath(string imagePath, string version)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return new Response { IsSuccess = false, Status = "Failed", Message = "Image path is required." };

            //string scriptPath = $"D:\\source\\NidDataExtract_3\\NidDataExtract_3\\Scripts\\Combine_{version.ToUpper()}.py";
            //string scriptPath = Path.Combine(_env.ContentRootPath, "Scripts", $"Combine_{version.ToUpper()}.py");
            //string scriptPath = "D:\\source\\NidDataExtract_3\\NidDataExtract_3\\Scripts\\combine_Tesse_easy_4.py";
            string scriptPath = Path.Combine(_env.WebRootPath, "Scripts", $"Combine_{version.ToUpper()}.py");

            string pythonExe = "C:\\Program Files\\Python312\\python.exe";

            var start = new ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments = $"\"{scriptPath}\" \"{imagePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            try
            {
                using var process = Process.Start(start)!;
                string error = await process.StandardError.ReadToEndAsync();
                string output = await process.StandardOutput.ReadToEndAsync();

                var result = JsonConvert.DeserializeObject<NidImageResult>(output);
                if (result == null)
                    return new Response { IsSuccess = false, Status = "Failed", Message = error };

                return new Response
                {
                    IsSuccess = true,
                    Status = "Success",
                    Message = "NID data extracted.",
                    ObjResponse = result
                };


            }
            catch (Exception ex)
            {
                return new Response { IsSuccess = false, Status = "Exception", Message = ex.Message };
            }
        }
    }

    //https://github.com/UB-Mannheim/tesseract/wiki
    //pip install easyocr pytesseract pillow
    //pip install easyocr pytesseract pillow
    //https://github.com/tesseract-ocr/tessdata/blob/main/ben.traineddata
}
