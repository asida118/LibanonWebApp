using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LibanonWebApp.Models;
using LibanonWebApp.Repository;

namespace LibanonWebApp.Controllers
{
    public class BookController : Controller
    {
        readonly IBookRepository bookRepository;
        public BookController(IBookRepository Bookrepository)
        {
            this.bookRepository = Bookrepository;
        }

        #region Show List book 
        
        //Show all book in the bookshelf
        [HttpGet]
        public ActionResult Index()
        {
            var lstBookShelf = bookRepository.GetBooksInShelf();
            return View(lstBookShelf);
        }

        //Show all the borrowed book
        [HttpGet]
        public ActionResult BorrowedBooks()
        {
            var lstBookBorrowed = bookRepository.GetBooksBorrowed();
            return View(lstBookBorrowed);
        }
        #endregion

        #region Create Book
        [HttpGet]
        public ActionResult Create() => View();

        [HttpPost]
        public ActionResult Create(Book book, HttpPostedFileBase Image)
        {
            if (Image != null)
            {
                if (Image.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(Image.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/Image"), fileName);
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                    Image.SaveAs(path);
                    book.Image = fileName;
                }
                if (bookRepository.Add(book))
                {
                    //Mail content
                    string title = "Create book verification";
                    string mailbody = "Hello "+ book.Owner.OwnerName;
                    mailbody += $"<br />You have sent the create book request, this is your book information:<br />BookName :{book.Title}<br />Author :{book.Author}<br />ISBN :{book.ISBN.ISBNCode}";
                    mailbody += "<br />Please click the following link to create your book:";
                    mailbody += "<br /><a href = '" + string.Format($"{Request.Url.Scheme}://{Request.Url.Authority}/Book/ActiveCreate/{book.BookId}") + "'>Click here to activate your book.</a>";
                    bookRepository.SendEmail(title, book.Owner.OwnerEmail, mailbody);

                    object notification = "Your create book request is stored, please check your email to verification";
                    return View("Notification", model: notification);
                }
                else
                    return RedirectToAction("Create");
            }
            else
            {
                ModelState.AddModelError("ImageUrl", "You must choose an image");
                return RedirectToAction("Create");
            }
        }

        [HttpGet]
        public ActionResult ActiveCreate(int id)
        {
            Book book = bookRepository.GetBookById(id);
            bookRepository.UpdateBookStatus(book,false);

            object notification = "Create book successfully";
            return View("Notification", model: notification);
        }
        #endregion

        #region Update book
        [HttpGet]
        public ActionResult Update(int id)
        {
            Book book = Session["BookUpdate"] as Book;
            if(book == null)
            {
                book = bookRepository.GetBookById(id);
            }
            if(TempData["WrongOtpMessage"] != null)
                ViewBag.OTPMessage = TempData["WrongOtpMessage"].ToString();

            return View(book);
        }

        [HttpPost]
        public ActionResult Update(Book book, HttpPostedFileBase Image)
        {
            var fileName = bookRepository.GetBookById(book.BookId).Image;
            book.Image = fileName;
            if (Image != null)
            {
                //Get name of image
                fileName = Path.GetFileName(Image.FileName);
                //Create fullpath of the image
                var path = Path.Combine(Server.MapPath("~/Content/Image"), fileName);
                //If the path had the file already, It would be deleted
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
                //Save the image by the path
                Image.SaveAs(path);
                book.Image = fileName;
            }
            
            string otp = GenerateOTP();
            bookRepository.UpdateBookOtp(book, otp);
            book.Confirmation.OTP = otp;
            string title = "Update Book Verification Code";
            string mailbody = "Hello " + book.Owner.OwnerName;
            mailbody += "<br />This is your update book verification code: "+otp;
            bookRepository.SendEmail(title, book.Owner.OwnerEmail, mailbody);
            Session["BookUpdate"] = book;

            return RedirectToAction("Update");
        }

        [HttpPost]
        public ActionResult ConfirmOtp(FormCollection formCollection)
        {
            Book book = Session["BookUpdate"] as Book;
            var otp = formCollection["OtpValue"];
            if(book.Confirmation.OTP == otp)
            {
                if (bookRepository.Update(book))
                {
                    Session["BookUpdate"] = null;
                    object notification = "Update book information successfully";
                    return View("Notification", model: notification);
                }
                else
                    return RedirectToAction("Update");
            }

            TempData["WrongOtpMessage"] = "The OTP is not correct";
            return RedirectToAction("Update",new {id = book.BookId });
        }
        #endregion

        #region Borrow book
        [HttpGet]
        public ActionResult BorrowBook(int id)
        {
            Book book = bookRepository.GetBookById(id);
            return View(book);
        }

        [HttpPost]
        public ActionResult BorrowBook(Book book)
        {
            TempData["borrower"] = book;
            string title = "Borrow Book Request Verification";
            string mailbody = "Hello " + book.BrwName;
            mailbody += $"<br /><br />You have sent the borrow book request, this is the book information:<br />BookName :{book.Title}<br />Author :{book.Author}<br />ISBN :{book.ISBN.ISBNCode}";
            mailbody += "<br />Please click the following link to confirm your borrow book request:";
            mailbody += "<br /><a href = '" + string.Format($"{Request.Url.Scheme}://{Request.Url.Authority}/Book/ConfirmBorrow/{book.BookId}") + "'>Click here to request borrow book.</a>";
            bookRepository.SendEmail(title, book.BrwEmail, mailbody);

            object notification = "Your borrow book request is stored, please check your email to verification";
            return View("Notification", model: notification);
        }

        [HttpGet]
        public ActionResult ConfirmBorrow(int id)
        {
            //Save borrower information
            Book book = TempData["borrower"] as Book;
            bookRepository.UpdateBorrower(id,book);
            
            //Send response mail
            string title = "Response the borrow book request";
            string mailbody = "Hello " + book.Owner.OwnerName;
            mailbody += $"<br /><br />The borrower {book.BrwName} want to borrow your book, this is the book information:<br />BookName :{book.Title}<br />Author :{book.Author}<br />ISBN :{book.ISBN.ISBNCode}";
            mailbody += "<br />Please click the following link to response to the borrow request:";
            mailbody += "<br /><a href = '" + string.Format($"{Request.Url.Scheme}://{Request.Url.Authority}/Book/AcceptBorrow/{book.BookId}") + "'>Accept borrow book request.</a>";
            mailbody += "<br /><a href = '" + string.Format($"{Request.Url.Scheme}://{Request.Url.Authority}/Book/CancelBorrow/{book.BookId}") + "'>Cancell borrow book request.</a>";
            bookRepository.SendEmail(title, book.Owner.OwnerEmail, mailbody);

            object notification = "Request book successfully and an Email has sent to the owner, Please waiting for owner response";
            return View("Notification", model: notification);
        }

        [HttpGet]
        public ActionResult AcceptBorrow(int id)
        {
            Book book = bookRepository.GetBookById(id);

            string title = "Borrowed Book Verification";
            //Owner mail
            string mailbodyOwn = $"Hello {book.Owner.OwnerName},";
            mailbodyOwn += $"<br /><br />You have accepted the borrow book request of: {book.BrwEmail}, This is your book information:";
            mailbodyOwn += $"<br />BookName :{book.Title}<br />Author :{book.Author}<br />Published: {book.Published}<br />ISBN :{book.ISBN.ISBNCode}";
            mailbodyOwn += "<br />Please click the following link to confirm the book is lent: ";
            mailbodyOwn += "<a href = '" + string.Format($"{Request.Url.Scheme}://{Request.Url.Authority}/Book/ReceiveBook?id={book.BookId}&ConfirmLend={true}") + "'>Book is lent</a>";
            bookRepository.SendEmail(title, book.Owner.OwnerEmail, mailbodyOwn);

            //Borrower mail
            string mailbodyBrw = "Hello borrower: "+book.BrwName;
            mailbodyBrw += $"<br /><br />Your book borrow request is accepted by : {book.Owner.OwnerEmail}, This is your book information:";
            mailbodyBrw += $"<br />BookName :{book.Title}<br />Author :{book.Author}<br />Published: {book.Published}<br />ISBN :{book.ISBN.ISBNCode}";
            mailbodyBrw += "<br />Please click the following link to confirm the book is borrowed: ";
            mailbodyBrw += "<a href = '" + string.Format($"{Request.Url.Scheme}://{Request.Url.Authority}/Book/ReceiveBook?id={book.BookId}&ConfirmBorrow={true}") + "'>Book is borrowed</a>";
            bookRepository.SendEmail(title, book.BrwEmail, mailbodyBrw);

            object notification = "You has accept the book request, Please check the Email to confirm the book is lent";
            return View("Notification", model: notification);
        }

        [HttpGet]
        public ActionResult ReceiveBook(int id,bool? ConfirmLend, bool? ConfirmBorrow)
        {
            Book book = bookRepository.GetBookById(id);
            bookRepository.UpdateBorrowLend(book,ConfirmLend,ConfirmBorrow);

            object notification = "Confirm the book is borrowed successfully.";
            return View("Notification", model: notification);
        }

        [HttpGet]
        public ActionResult CancelBorrow(int id)
        {
            Book book = bookRepository.GetBookById(id);

            string title = "Borrow Book Request is canceled";
            //Owner mail
            string mailbodyOwn = $"Hello {book.Owner.OwnerName} Owner of {book.Title}";
            mailbodyOwn += $"<br /><br />You have canceled the borrow book request of: " + book.BrwEmail;
            bookRepository.SendEmail(title, book.Owner.OwnerEmail, mailbodyOwn);

            //Borrower mail
            string mailbodyBrw = "Hello borrower: " + book.BrwName;
            mailbodyBrw += $"<br /><br />Your book borrow ({book.Title}) request is cancel by the owner: " + book.Owner.OwnerEmail;
            bookRepository.SendEmail(title, book.BrwEmail, mailbodyBrw);

            bookRepository.DeleteBorrower(book.BookId);
            object notification = "The book borrow request has been canceled ";
            return View("Notification", model: notification);
        }
        #endregion

        #region Return book
        [HttpGet]
        public ActionResult ReturnBook(int id)
        {
            Book book = bookRepository.GetBookById(id);
            return View(book);
        }

        [HttpPost]
        public ActionResult ReturnBook(Book book)
        {
            var email = bookRepository.GetBookById(book.BookId).BrwEmail;
            if(book.BrwEmail == email)
            {
                string title = "Return book request verification";
                string mailbody = "Hello " + book.BrwName;
                mailbody += "<br /><br />Please click the following link to make sure that you want to return the book:";
                mailbody += "<br /><a href = '" + string.Format($"{Request.Url.Scheme}://{Request.Url.Authority}/Book/ConfirmReturn/{book.BookId}") + "'>Click here to return book.</a>";
                bookRepository.SendEmail(title, book.BrwEmail, mailbody);

                object notification = "Your return book request is stored, please check your email to verification";
                return View("Notification", model: notification);
            }
            return View("ReturnBook");
        }

        [HttpGet]
        public ActionResult ConfirmReturn(int id)
        {
            Book book = bookRepository.GetBookById(id);
            string title = "Received book verification";
            //Owner mail
            string mailbodyOwn = $"Hello {book.Owner.OwnerName} Owner of {book.Title}";
            mailbodyOwn += "<br /><br />Your book is return by : " + book.BrwName;
            mailbodyOwn += $"<br />Please contact {book.BrwEmail} to receive book. ";
            mailbodyOwn += $"<br /><br />Please click the following link if the book is return to you. ";
            mailbodyOwn += "<a href = '" + string.Format($"{Request.Url.Scheme}://{Request.Url.Authority}/Book/ReceiveBookBack?id={book.BookId}&ConfirmLend={false}") + "'>Book is received back</a>";
            bookRepository.SendEmail(title, book.Owner.OwnerEmail, mailbodyOwn);

            //Borrower mail
            string mailbodyBrw = "Hello borrower: " + book.BrwName;
            mailbodyBrw += $"<br /><br />Your book return ({book.Title}) request is sent to system. " + book.Owner.OwnerEmail;
            mailbodyBrw += "<br />Here is the book owner information: ";
            mailbodyBrw += $"<br />Owner name :{book.Owner.OwnerName} <br />Owner Mail :{book.Owner.OwnerEmail} <br />Owner Phone :{book.Owner.OwnerPhone} ";
            mailbodyBrw += $"<br />Please click the following link if you has returned the book to the owner: ";
            mailbodyBrw += "<a href = '" + string.Format($"{Request.Url.Scheme}://{Request.Url.Authority}/Book/ReceiveBookBack?id={book.BookId}&ConfirmBorrow={false}") + "'>Book is returned back</a>";
            bookRepository.SendEmail(title, book.BrwEmail, mailbodyBrw);

            object notification = "The book is cofirm return back ";
            return View("Notification", model: notification);
        }

        [HttpGet]
        public ActionResult ReceiveBookBack(int id, bool? ConfirmLend, bool? ConfirmBorrow)
        {
            Book book = bookRepository.GetBookById(id);
            bookRepository.UpdateBorrowLend(book, ConfirmLend, ConfirmBorrow);
            bookRepository.DeleteBorrower(book.BookId);

            object notification = "The book owner has receive the book back";
            return View("Notification", model: notification);
        }
        #endregion

        #region Rating book
        [HttpGet]
        public ActionResult RatingBook(int id)
        {
            Book book = bookRepository.GetBookById(id);
            return View(book);
        }

        [HttpPost]
        public ActionResult RatingBook(Book book)
        {
            bookRepository.UpdateBookRating(book);
            return RedirectToAction("BorrowedBooks");
        }
        #endregion

        private string GenerateOTP()
        {
            Random random = new Random();
            int randNum = random.Next(111111, 1000000);
            string otp = randNum.ToString("D6");
            return otp;
        }
    }
}