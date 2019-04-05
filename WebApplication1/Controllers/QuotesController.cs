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
        /// List all public gists sorted by most recently updated to least recently updated.
        /// </summary>
        /// <remarks>With pagination, you can fetch up to 3000 gists. For example, you can fetch 100 pages with 30 gists per page or 30 pages with 100 gists per page.</remarks>
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
        /// List all public gists sorted by most recently updated to least recently updated.
        /// </summary>
        /// <remarks>With pagination, you can fetch up to 3000 gists. For example, you can fetch 100 pages with 30 gists per page or 30 pages with 100 gists per page.</remarks>
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
        /// List all public gists sorted by most recently updated to least recently updated.
        /// </summary>
        /// <remarks>With pagination, you can fetch up to 3000 gists. For example, you can fetch 100 pages with 30 gists per page or 30 pages with 100 gists per page.</remarks>
        [HttpGet("[action]")]
        [ProducesResponseType(200, Type = typeof(Quote[]))]
        public IActionResult SearchQuote(string type)
        {
            var quotes = _quotesDbContext.Quotes.Where(q => q.Type.Contains(type));

            return StatusCode(StatusCodes.Status200OK, quotes);
        }

        /// <summary>
        /// List all public gists sorted by most recently updated to least recently updated.
        /// </summary>
        /// <remarks>With pagination, you can fetch up to 3000 gists. For example, you can fetch 100 pages with 30 gists per page or 30 pages with 100 gists per page.</remarks>
        [HttpGet("[action]")]
        [ProducesResponseType(200, Type = typeof(Quote[]))]
        public IActionResult MyQuotes(string type)
        {
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var quotes = _quotesDbContext.Quotes.Where(x => x.UserId == userId);

            return StatusCode(StatusCodes.Status200OK, quotes);
        }
        
        /// <summary>
        /// List all public gists sorted by most recently updated to least recently updated.
        /// </summary>
        /// <remarks>With pagination, you can fetch up to 3000 gists. For example, you can fetch 100 pages with 30 gists per page or 30 pages with 100 gists per page.</remarks>
        [HttpGet("{id}", Name = "Get")]
        [ProducesResponseType(200, Type = typeof(Quote))]
        [ProducesResponseType(404, Type = typeof(void))]
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
        /// List all public gists sorted by most recently updated to least recently updated.
        /// </summary>
        /// <remarks>With pagination, you can fetch up to 3000 gists. For example, you can fetch 100 pages with 30 gists per page or 30 pages with 100 gists per page.</remarks>
        [HttpGet("[action]/{id}")]
        public int Test(int id)
        {
            return id;
        }

        /// <summary>
        /// List all public gists sorted by most recently updated to least recently updated.
        /// </summary>
        /// <remarks>With pagination, you can fetch up to 3000 gists. For example, you can fetch 100 pages with 30 gists per page or 30 pages with 100 gists per page.</remarks>
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
        /// List all public gists sorted by most recently updated to least recently updated.
        /// </summary>
        /// <remarks>With pagination, you can fetch up to 3000 gists. For example, you can fetch 100 pages with 30 gists per page or 30 pages with 100 gists per page.</remarks>
        [HttpPut("{id}")]
        [ProducesResponseType(200, Type = typeof(void))]
        [ProducesResponseType(403, Type = typeof(void))]
        [ProducesResponseType(404, Type = typeof(void))]
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
        /// List all public gists sorted by most recently updated to least recently updated.
        /// </summary>
        /// <remarks>With pagination, you can fetch up to 3000 gists. For example, you can fetch 100 pages with 30 gists per page or 30 pages with 100 gists per page.</remarks>
        [HttpDelete("{id}")]
        [ProducesResponseType(200, Type = typeof(void))]
        [ProducesResponseType(403, Type = typeof(void))]
        [ProducesResponseType(404, Type = typeof(void))]
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
