using hw1.Models;
using Microsoft.EntityFrameworkCore;

namespace hw1
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var context = new BookStoreContext())
            {
                // // Чтение данных
                var books = context.Books.Include(b => b.Author).ToList();
                foreach (var book in books) 
                {
                    Console.WriteLine($"Title: {book.Title}, Author: {book.Author?.Name}");
                }

                // Запись данных
                // var author = new Author { Name = "New Author" };
                // var newBook = new Book { Title = "New Book", Author = author }; 
                // context.Authors.Add(author);
                // context.Books.Add(newBook);
                // context.SaveChanges();
            }
        }
    }
}
