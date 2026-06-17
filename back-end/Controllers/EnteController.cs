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
    public class EnteController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EnteController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Ente
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ente>>> GetEnti()
        {
            return await _context.Enti.ToListAsync();
        }

        // GET: api/Ente/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Ente>> GetEnte(int id)
        {
            var ente = await _context.Enti.FindAsync(id);

            if (ente == null)
            {
                return NotFound();
            }

            return ente;
        }

        // PUT: api/Ente/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEnte(int id, Ente ente)
        {
            if (id != ente.IdEnte)
            {
                return BadRequest();
            }

            _context.Entry(ente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EnteExists(id))
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

        // POST: api/Ente
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Ente>> PostEnte(Ente ente)
        {
            _context.Enti.Add(ente);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEnte", new { id = ente.IdEnte }, ente);
        }

        // DELETE: api/Ente/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnte(int id)
        {
            var ente = await _context.Enti.FindAsync(id);
            if (ente == null)
            {
                return NotFound();
            }

            _context.Enti.Remove(ente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EnteExists(int id)
        {
            return _context.Enti.Any(e => e.IdEnte == id);
        }
    }
}
