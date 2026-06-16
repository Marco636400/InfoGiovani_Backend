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
    public class SchedaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SchedaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Scheda
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Scheda>>> GetSchede()
        {
            return await _context.Schede.ToListAsync();
        }

        // GET: api/Scheda/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Scheda>> GetScheda(int id)
        {
            var scheda = await _context.Schede.FindAsync(id);

            if (scheda == null)
            {
                return NotFound();
            }

            return scheda;
        }

        // PUT: api/Scheda/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutScheda(int id, Scheda scheda)
        {
            if (id != scheda.IdScheda)
            {
                return BadRequest();
            }

            _context.Entry(scheda).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SchedaExists(id))
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

        // POST: api/Scheda
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Scheda>> PostScheda(Scheda scheda)
        {
            _context.Schede.Add(scheda);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetScheda", new { id = scheda.IdScheda }, scheda);
        }

        // DELETE: api/Scheda/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScheda(int id)
        {
            var scheda = await _context.Schede.FindAsync(id);
            if (scheda == null)
            {
                return NotFound();
            }

            _context.Schede.Remove(scheda);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SchedaExists(int id)
        {
            return _context.Schede.Any(e => e.IdScheda == id);
        }
    }
}
