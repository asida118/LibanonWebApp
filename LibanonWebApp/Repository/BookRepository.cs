using LibanonWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace LibanonWebApp.Repository
{
    public class BookRepository : IBookRepository
    {
        BookContext db;
        public BookRepository() => db = new BookContext();

        //Add new book
        public bool Add(Book item)
        {
            try
            {
                if (item == null)
                {
                    return false;
                }
                db.Books.Add(item);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //Get all book isn't borrowed
        public IEnumerable<Book> GetBooksInShelf() => db.Books.Where(b=>b.Status == false).OrderBy(b=>b.BookId);

        //Get all book is borrowed
        public IEnumerable<Book> GetBooksBorrowed() => db.Books.Where(b => b.Status == true).OrderBy(b => b.BookId);
        
        //Get the book by the bookId
        public Book GetBookById(int id) => db.Books.FirstOrDefault(x => x.BookId == id);

        //Update the book information
        public bool Update(Book item)
        {
            try
            {
                if (item == null)
                    return false;
                var book = db.Books.Find(item.BookId);

                book.Title = item.Title;
                book.Author = item.Author;
                book.Image = item.Image;
                book.Summary = item.Summary;
                book.ISBN.ISBNCode = item.ISBN.ISBNCode;
                book.Published = item.Published;

                db.SaveChanges();
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        
        //Update the book status(borrowed or not borrowed)
        public void UpdateBookStatus(Book item,bool status)
        {
            Book bookUpdateStatus = db.Books.Find(item.BookId);
            bookUpdateStatus.Status = status;

            db.SaveChanges();
        }

        //Update the otp of the book
        public void UpdateBookOtp(Book item,string otp)
        {
            Book bookUpdateOtp = db.Books.Find(item.BookId);
            bookUpdateOtp.Confirmation.OTP = otp;
            db.SaveChanges();
        }

        //Add borrower information into book when the book is borrowed
        public void UpdateBorrower(int id, Book item)
        {
            Book book = db.Books.Find(id);
            book.BrwName = item.BrwName;
            book.BrwEmail = item.BrwEmail;
            book.BrwPhone = item.BrwPhone;
            book.BrwNote = item.BrwNote;

            db.SaveChanges();
        }

        //Clear borrower information into book when the book is returned or cancel borrow
        public void DeleteBorrower(int id)
        {
            Book book = db.Books.Find(id);
            book.BrwName = null;
            book.BrwEmail = null;
            book.BrwPhone = null;
            book.BrwNote = null;

            db.SaveChanges();
        }

        //Update the status confirmLend or confirmBorrow when the book is borrowed or return back
        public void UpdateBorrowLend(Book item,bool? confirmLend, bool? confirmBorrow)
        {
            Book book = db.Books.Find(item.BookId);
            if(confirmLend != null)
                book.Confirmation.ConfirmLend = (bool)confirmLend;
            if (confirmBorrow != null)
                book.Confirmation.ConfirmBorrow = (bool)confirmBorrow;
            if (book.Confirmation.ConfirmBorrow == true && book.Confirmation.ConfirmLend == true)
                UpdateBookStatus(book, true);
            if (book.Confirmation.ConfirmBorrow == false && book.Confirmation.ConfirmLend == false)
                UpdateBookStatus(book, false);
            db.SaveChanges();
        }

        //Update the book rating score with average rating score
        public void UpdateBookRating(Book item)
        {
            Book book = db.Books.Find(item.BookId);
            double ratingScore = (item.ISBN.RatingScore + (book.ISBN.RatingScore * book.ISBN.RatingNumber)) / (book.ISBN.RatingNumber + 1);
            book.ISBN.RatingScore = Math.Round(ratingScore,2);
            book.ISBN.RatingNumber++;
            
            db.SaveChanges();
            RatingSameISBN(book);
        }

        //Update the book rating score which have the same ISBN code
        private void RatingSameISBN(Book item)
        {
            IEnumerable<Book> lstbooks = db.Books.Where(b=>b.ISBN.ISBNCode == item.ISBN.ISBNCode);
            foreach(var book in lstbooks)
            {
                book.ISBN.RatingScore = item.ISBN.RatingScore;
            }

            db.SaveChanges();
        }
     
        //Send email method
        public void SendEmail(string MailTitle, string ToEmail, string MailContent)
        {
            MailMessage mail = new MailMessage();
            mail.To.Add(ToEmail);
            mail.From = new MailAddress(ToEmail); 
            mail.Subject = MailTitle;
            mail.Body = MailContent;
            mail.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new System.Net.NetworkCredential("libanonWebapp@gmail.com", "Libanon123");
            smtp.EnableSsl = true;
            smtp.Send(mail);
        }
        
    }
}