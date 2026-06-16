using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InfoGiovani_Back.Models;
using Microsoft.AspNetCore.Authorization;

namespace back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CittaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CittaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Citta
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Citta>>> GetCitta()
        {
            return await _context.Citta.ToListAsync();
        }

        // GET: api/Citta/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Citta>> GetCitta(int id)
        {
            var citta = await _context.Citta.FindAsync(id);

            if (citta == null)
            {
                return NotFound();
            }

            return citta;
        }

        // PUT: api/Citta/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCitta(int id, Citta citta)
        {
            if (id != citta.IdCitta)
            {
                return BadRequest();
            }

            _context.Entry(citta).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CittaExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Citta
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Citta>> PostCitta(Citta citta)
        {
            _context.Citta.Add(citta);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CittaExists(citta.IdCitta))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetCitta", new { id = citta.IdCitta }, citta);
        }

        // DELETE: api/Citta/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCitta(int id)
        {
            var citta = await _context.Citta.FindAsync(id);
            if (citta == null)
            {
                return NotFound();
            }

            _context.Citta.Remove(citta);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CittaExists(int id)
        {
            return _context.Citta.Any(e => e.IdCitta == id);
        }
    }
}
