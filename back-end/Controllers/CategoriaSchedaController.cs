using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InfoGiovani_Back.Models;
using Microsoft.AspNetCore.Authorization;
using InfoGiovani_Back.DTOs;

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

        // GET: api/CategoriaScheda/5
        [HttpGet("{CategoriaId}")]
        public async Task<ActionResult<IEnumerable<Scheda>>> GetCategorieSchede(int CategoriaId)
        {
            var famigliari = await _context.CategorieSchede
               .Where(f => f.IdCategoria == CategoriaId)
               .Join(
                   _context.Schede,
                   f => f.IdScheda,
                   c => c.IdScheda,
                   (f, c) => c
               )
               .ToListAsync();

            return Ok(famigliari);
        }

        // GET: api/CategoriaScheda/5/3
        [HttpGet("{categoriaId}/{schedaId}")]
        public async Task<ActionResult<Scheda>> GetCategoriaScheda(int categoriaId, int schedaId)
        {
            var scheda = await _context.CategorieSchede
                .Where(f => f.IdCategoria == categoriaId && f.IdScheda == schedaId)
                .Select(f => f.Scheda)
                .FirstOrDefaultAsync();

            if (scheda == null)
                return NotFound();

            return Ok(scheda);
        }

        // POST: api/Categoria Scheda
        [HttpPost]
        public async Task<ActionResult<CreazioneCategoriaSchedaDTO>> PostCategoriaScheda(CreazioneCategoriaSchedaDTO dto)
        {
            // Creiamo l'oggetto di database.
            var categorieSchede = new CategoriaScheda
            {
                IdCategoria = dto.IdCategoria,
                IdScheda = dto.IdScheda,
            };

            _context.CategorieSchede.Add(categorieSchede);
            await _context.SaveChangesAsync();

            // Mappiamo l'oggetto appena creato nel DTO di risposta
            var ruoloDto = new CreazioneCategoriaSchedaDTO
            {
                IdCategoria = categorieSchede.IdCategoria,
                IdScheda = categorieSchede.IdScheda
            };

            return CreatedAtAction(nameof(GetCategoriaScheda), new { id = categorieSchede.IdScheda }, ruoloDto);
        }

        // DELETE: api/CategoriaScheda/5/3
        [HttpDelete("{categoriaId}/{schedaId}")]
        public async Task<IActionResult> DeleteCategoriaScheda(int categoriaId, int schedaId)
        {
            var categoriaScheda = await _context.CategorieSchede
                .FirstOrDefaultAsync(f => f.IdCategoria == categoriaId && f.IdScheda == schedaId);

            if (categoriaScheda == null)
                return NotFound();

            _context.CategorieSchede.Remove(categoriaScheda);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
