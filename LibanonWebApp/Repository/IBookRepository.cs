using LibanonWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibanonWebApp.Repository
{
    public interface IBookRepository
    {
        IEnumerable<Book> GetBooksInShelf();
        IEnumerable<Book> GetBooksBorrowed();
        Book GetBookById(int id);
        bool Add(Book item);
        bool Update(Book item);
        void SendEmail(string MailTitle, string ToEmail, string MailContent);
        void UpdateBookStatus(Book item, bool status);
        void UpdateBookOtp(Book item, string otp);
        void UpdateBorrower(int id, Book item);
        void DeleteBorrower(int id);
        void UpdateBorrowLend(Book item, bool? confirmLend, bool? confirmBorrow);
        void UpdateBookRating(Book item);

    }
}
