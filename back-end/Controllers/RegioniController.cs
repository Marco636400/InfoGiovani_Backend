using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InfoGiovani_Back.Models;

namespace back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegioniController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RegioniController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Regioni
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Regioni>>> GetRegioni()
        {
            return await _context.Regioni.ToListAsync();
        }

        // GET: api/Regioni/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Regioni>> GetRegioni(int id)
        {
            var regioni = await _context.Regioni.FindAsync(id);

            if (regioni == null)
            {
                return NotFound();
            }

            return regioni;
        }

        // PUT: api/Regioni/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRegioni(int id, Regioni regioni)
        {
            if (id != regioni.IdRegione)
            {
                return BadRequest();
            }

            _context.Entry(regioni).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RegioniExists(id))
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

        // POST: api/Regioni
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Regioni>> PostRegioni(Regioni regioni)
        {
            _context.Regioni.Add(regioni);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (RegioniExists(regioni.IdRegione))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetRegioni", new { id = regioni.IdRegione }, regioni);
        }

        // DELETE: api/Regioni/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegioni(int id)
        {
            var regioni = await _context.Regioni.FindAsync(id);
            if (regioni == null)
            {
                return NotFound();
            }

            _context.Regioni.Remove(regioni);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RegioniExists(int id)
        {
            return _context.Regioni.Any(e => e.IdRegione == id);
        }
    }
}
