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
    public class CategoriaSchedaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriaSchedaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/CategoriaScheda
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaScheda>>> GetCategorieSchede()
        {
            return await _context.CategorieSchede.ToListAsync();
        }

        // GET: api/CategoriaScheda/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaScheda>> GetCategoriaScheda(int id)
        {
            var categoriaScheda = await _context.CategorieSchede.FindAsync(id);

            if (categoriaScheda == null)
            {
                return NotFound();
            }

            return categoriaScheda;
        }

        // PUT: api/CategoriaScheda/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategoriaScheda(int id, CategoriaScheda categoriaScheda)
        {
            if (id != categoriaScheda.IdCategoriaScheda)
            {
                return BadRequest();
            }

            _context.Entry(categoriaScheda).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoriaSchedaExists(id))
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

        // POST: api/CategoriaScheda
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CategoriaScheda>> PostCategoriaScheda(CategoriaScheda categoriaScheda)
        {
            _context.CategorieSchede.Add(categoriaScheda);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCategoriaScheda", new { id = categoriaScheda.IdCategoriaScheda }, categoriaScheda);
        }

        // DELETE: api/CategoriaScheda/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategoriaScheda(int id)
        {
            var categoriaScheda = await _context.CategorieSchede.FindAsync(id);
            if (categoriaScheda == null)
            {
                return NotFound();
            }

            _context.CategorieSchede.Remove(categoriaScheda);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoriaSchedaExists(int id)
        {
            return _context.CategorieSchede.Any(e => e.IdCategoriaScheda == id);
        }
    }
}
