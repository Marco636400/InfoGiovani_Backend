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
    public class UtenteController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UtenteController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Utente
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Utente>>> GetUtenti()
        {
            return await _context.Utenti.ToListAsync();
        }

        // GET: api/Utente/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Utente>> GetUtente(int id)
        {
            var utente = await _context.Utenti.FindAsync(id);

            if (utente == null)
            {
                return NotFound();
            }

            return utente;
        }

        // PUT: api/Utente/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUtente(int id, Utente utente)
        {
            if (id != utente.IdUtente)
            {
                return BadRequest();
            }

            _context.Entry(utente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UtenteExists(id))
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

        // POST: api/Utente
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Utente>> PostUtente(Utente utente)
        {
            _context.Utenti.Add(utente);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUtente", new { id = utente.IdUtente }, utente);
        }

        // DELETE: api/Utente/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUtente(int id)
        {
            var utente = await _context.Utenti.FindAsync(id);
            if (utente == null)
            {
                return NotFound();
            }

            _context.Utenti.Remove(utente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UtenteExists(int id)
        {
            return _context.Utenti.Any(e => e.IdUtente == id);
        }
    }
}
