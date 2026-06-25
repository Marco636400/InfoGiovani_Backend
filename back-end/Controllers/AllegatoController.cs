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

    [Route("[controller]")]

    [ApiController]
    public class AllegatoController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AllegatoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Allegato/5
        [HttpGet("{idScheda}")]
        public async Task<IActionResult> GetAllegati(int idScheda)
        {
            var query = _context.Allegati.AsQueryable();

            var risultato = await query
                .Select(a => new
                {
                    IdAllegato = a.IdAllegato,
                    IdScheda = a.IdScheda,
                    Nome = a.Nome,
                    Url = a.Url
                })
                .ToListAsync();

            return Ok(risultato);
        }

        // GET: api/Allegato/5/2
        [HttpGet("{id},{idScheda}")]
        public async Task<IActionResult> GetAllegato(int id, int idScheda)
        {
            // Cerchiamo l'allegato che corrisponde a idAllegato e idScheda
            var allegato = await _context.Allegati
                .Where(a => a.IdAllegato == id && a.IdScheda == idScheda)
                .Select(a => new
                {
                    IdAllegato = a.IdAllegato,
                    IdScheda = a.IdScheda,
                    Nome = a.Nome,
                    Url = a.Url
                })
                .FirstOrDefaultAsync();

            if (allegato == null)
            {
                return NotFound("Allegato non trovato o non associato a questa scheda.");
            }

            return Ok(allegato);
        }

        // PUT: api/Allegato/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAllegato(int id, ModificaAllegatoDTO dto)
        {
            var allegato = await _context.Allegati.FindAsync(id);
            if (allegato == null)
            {
                return BadRequest();
            }

            if (dto.IdScheda != null)
                allegato.IdScheda = (int)dto.IdScheda;
            if (!string.IsNullOrEmpty(dto.Nome))
                allegato.Nome = dto.Nome;
            if (!string.IsNullOrEmpty(dto.Url))
                allegato.Url = dto.Url;

            _context.Entry(allegato).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AllegatoExists(id))
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
        // POST: api/Allegato
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<CreaAllegatoDTO>> PostAllegato(CreaAllegatoDTO dto)
        {
            if (string.IsNullOrEmpty(dto.Url))
            {
                return BadRequest("L'URL è obbligatorio");
            }
            if (string.IsNullOrEmpty(dto.Nome))
            {
                return BadRequest("Il nome è obbligatorio");
            }

            var allegato = new Allegato
            {
                IdScheda = dto.IdScheda,
                Nome = dto.Nome,
                Url = dto.Url
            };

            _context.Allegati.Add(allegato);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetAllegato", new { id = allegato.IdAllegato, idScheda = allegato.IdScheda }, allegato);
        }
        // DELETE: api/Allegato/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAllegato(int id)
        {
            var allegato = await _context.Allegati.FindAsync(id);
            if (allegato == null)
            {
                return NotFound();
            }

            _context.Allegati.Remove(allegato);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        private bool AllegatoExists(int id)
        {
            return _context.Allegati.Any(e => e.IdAllegato == id);
        }
    }
}