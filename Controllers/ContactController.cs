using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using FirstIterationProductRelease.Models;

namespace FirstIterationProductRelease.Controllers
{
    public class ContactController : Controller
    {

        [HttpGet]
        public ActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendEmail(ContactDetails model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Configuring the email
                    var fromAddress = new MailAddress("youremail@example.com", "Your Name");
                    var toAddress = new MailAddress("destinationemail@example.com");
                    const string fromPassword = "yourpassword";
                    string subject = model.Subject;
                    string body = $"Name: {model.Name}\nEmail: {model.Email}\n\nMessage:\n{model.Message}";

                    var smtp = new SmtpClient
                    {
                        Host = "smtp.example.com",  // Change to your SMTP server
                        Port = 587,  // Change the port according to your server
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                    };

                    using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body
                    })
                    {
                        smtp.Send(message);
                    }

                    TempData["Message"] = "Your message has been sent!";
                    return RedirectToAction("Contact");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "There was an issue sending your message. Please try again later.");
                    return View("Contact", model);
                }
            }

            // If model is invalid, return to view with errors
            return View("Contact", model);
        }
    }
}
