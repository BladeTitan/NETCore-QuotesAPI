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

        /// <summary>
        /// List all Quotes which can be sorted desc or asc.
        /// </summary>
        /// <param name="sort"></param>
        [HttpGet]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        [ProducesResponseType(200, Type = typeof(Quote[]))]
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

        /// <summary>
        /// List quotes in paginated chunks.
        /// </summary>
        /// <param name="pageNumber">Defaults to 5 when no value is entered.</param>
        /// <param name="pageSize">Defaults to 1 if no value has been entered.</param>
        [HttpGet("[action]")]
        [ProducesResponseType(200, Type = typeof(Quote[]))]
        public IActionResult PagingQuote(int? pageNumber, int? pageSize)
        {
            var quotes = _quotesDbContext.Quotes;
            int currentPage = pageNumber ?? 1;
            int currentSize = pageSize ?? 5;

            return StatusCode(StatusCodes.Status200OK, quotes.Skip((currentPage - 1) * currentSize).Take(currentSize));
        }

        /// <summary>
        /// List all quotes which are of the specified type.
        /// </summary>
        /// <param name="type"></param>
        [HttpGet("[action]")]
        [ProducesResponseType(200, Type = typeof(Quote[]))]
        public IActionResult SearchQuote(string type)
        {
            var quotes = _quotesDbContext.Quotes.Where(q => q.Type.Contains(type));

            return StatusCode(StatusCodes.Status200OK, quotes);
        }

        /// <summary>
        /// List all quotes that have been uploaded by the authenticated user.
        /// </summary>
        /// <param name="type"></param>
        [HttpGet("[action]")]
        [ProducesResponseType(200, Type = typeof(Quote[]))]
        public IActionResult MyQuotes(string type)
        {
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var quotes = _quotesDbContext.Quotes.Where(x => x.UserId == userId);

            return StatusCode(StatusCodes.Status200OK, quotes);
        }

        /// <summary>
        /// List a specific quote.
        /// </summary>
        /// <param name="id">The stored id of the quote object.</param>
        [HttpGet("{id}", Name = "Get")]
        [ProducesResponseType(200, Type = typeof(Quote))]
        [ProducesResponseType(404)]
        public IActionResult Get(int id)
        {
            var quote = _quotesDbContext.Quotes.Find(id);

            if (quote == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, "Record Not Found");
            }

            return StatusCode(StatusCodes.Status200OK, quote);
        }

        /// <summary>
        /// Insert a new Quote into the database.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(void))]
        public IActionResult Post([FromBody] Quote quote)
        {
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            quote.UserId = userId;
            _quotesDbContext.Quotes.Add(quote);
            _quotesDbContext.SaveChanges();
            return StatusCode(StatusCodes.Status201Created);
        }

        /// <summary>
        /// Update a specific Quote in the database.
        /// </summary>
        /// <param name="id">The id of the quote.</param>
        [HttpPut("{id}")]
        [ProducesResponseType(200, Type = typeof(void))]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
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
                return StatusCode(StatusCodes.Status403Forbidden, "You can't Update this Record.");
            }

            oldQuote.Title = newQuote.Title;
            oldQuote.Author = newQuote.Author;
            oldQuote.Description = newQuote.Description;
            oldQuote.Type = newQuote.Type;
            oldQuote.CreatedAt = newQuote.CreatedAt;

            _quotesDbContext.SaveChanges();
            return StatusCode(StatusCodes.Status200OK, "Record Updated Successfully");
        }

        /// <summary>
        /// Delete a specific Quote in the database.
        /// </summary>
        /// <param name="id">The id of the quote.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(200, Type = typeof(void))]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
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
                return StatusCode(StatusCodes.Status403Forbidden, "You can't Update this Record.");
            }

            _quotesDbContext.Remove(quote);
            _quotesDbContext.SaveChanges();
            return StatusCode(StatusCodes.Status200OK, "Record Successfully Deleted");
        }
    }
}
