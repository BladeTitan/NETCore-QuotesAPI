using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuotesApi.Data;
using QuotesApi.Models;

namespace QuotesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuotesController : ControllerBase
    {
        private QuotesDbContext _quotesDbContext;

        public QuotesController(QuotesDbContext quotesDbContext)
        {
            _quotesDbContext = quotesDbContext;
        }

        // GET: api/Quotes
        [HttpGet]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        public IActionResult Get(string sort)
        {
            IQueryable<Quote> quotes;

            switch(sort)
            {
                case "desc":
                    quotes = _quotesDbContext.Quotes.OrderByDescending(x => x.CreatedAt);
                    break;
                case "asc":
                    quotes = _quotesDbContext.Quotes.OrderBy(x => x.CreatedAt);
                    break;
                default:
                    quotes = _quotesDbContext.Quotes;
                    break;
            }

            return StatusCode(StatusCodes.Status200OK, quotes);
        }

        [HttpGet("[action]")]
        public IActionResult PagingQuote(int? pageNumber, int? pageSize)
        {
            var quotes = _quotesDbContext.Quotes;
            int currentPage = pageNumber ?? 1;
            int currentSize = pageSize ?? 5;

            return StatusCode(StatusCodes.Status200OK, quotes.Skip((currentPage - 1) * currentSize).Take(currentSize));
        }

        [HttpGet("[action]")]
        public IActionResult SearchQuote(string type)
        {
            var quotes = _quotesDbContext.Quotes.Where(q => q.Type.Contains(type));

            return StatusCode(StatusCodes.Status200OK, quotes);
        }

        [HttpGet("[action]")]
        public IActionResult MyQuotes(string type)
        {
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var quotes = _quotesDbContext.Quotes.Where(x => x.UserId == userId);

            return StatusCode(StatusCodes.Status200OK, quotes);
        }

        // GET: api/Quotes/5
        [HttpGet("{id}", Name = "Get")]
        public IActionResult Get(int id)
        {
            var quote = _quotesDbContext.Quotes.Find(id);

            if (quote == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, "Record Not Found");
            }

            return StatusCode(StatusCodes.Status200OK, quote);
        }
        
        [HttpGet("[action]/{id}")]
        public int Test(int id)
        {
            return id;
        }

        // POST: api/Quotes
        [HttpPost]
        public IActionResult Post([FromBody] Quote quote)
        {
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            quote.UserId = userId;
            _quotesDbContext.Quotes.Add(quote);
            _quotesDbContext.SaveChanges();
            return StatusCode(StatusCodes.Status201Created);
        }

        // PUT: api/Quotes/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Quote newQuote)
        {
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            var oldQuote = _quotesDbContext.Quotes.Find(id);
            if(oldQuote == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, "Record Not Found");
            }

            if (userId != oldQuote.UserId)
            {
                return StatusCode(StatusCodes.Status404NotFound, "You can't Update this Record.");
            }

            oldQuote.Title = newQuote.Title;
            oldQuote.Author = newQuote.Author;
            oldQuote.Description = newQuote.Description;
            oldQuote.Type = newQuote.Type;
            oldQuote.CreatedAt = newQuote.CreatedAt;

            _quotesDbContext.SaveChanges();
            return StatusCode(StatusCodes.Status200OK, "Record Updated Successfully");
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            var quote = _quotesDbContext.Quotes.Find(id);
            if (quote == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, "Record Not Found");
            }

            if (userId != quote.UserId)
            {
                return StatusCode(StatusCodes.Status404NotFound, "You can't Update this Record.");
            }

            _quotesDbContext.Remove(quote);
            _quotesDbContext.SaveChanges();
            return StatusCode(StatusCodes.Status200OK, "Record Successfully Deleted");
        }
    }
}
