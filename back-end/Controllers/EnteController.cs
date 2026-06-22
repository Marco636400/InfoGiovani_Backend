using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InfoGiovani_Back.Models;
using InfoGiovani_Back.DTOs;
using InfoGiovani_Back.Middleware;


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
            return Ok(await _context.Enti.ToListAsync());
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
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEnti(int id, CreaEModificaEnteDTO dto)
        {
            var identita = HttpContext.Items[IdentitaUtente.HttpContextKey] as IdentitaUtente;
            if (identita == null)
                return BadRequest("Utente non trovato");
            var enti = await _context.Enti.FindAsync(id);
            if (enti == null)
            {
                return NotFound();
            }

            enti.Nome = dto.Nome;
            enti.DescrizioneEnte = dto.DescrizioneEnte;
            enti.Telefono1 = dto.Telefono1;
            enti.Telefono2 = dto.Telefono2;
            enti.Fax = dto.Fax;
            enti.Email = dto.Email;
            enti.Indirizzo = dto.Indirizzo;
            enti.Url = dto.Url;
            enti.Contatto = dto.Contatto;
            enti.IdCitta = dto.IdCitta;
            enti.IdUtenteModifica = identita.IdUtente;
            enti.DataUltimaModifica = DateTime.Now;

            _context.Entry(enti).State = EntityState.Modified;

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
        [HttpPost]
        public async Task<ActionResult<CreaEModificaEnteDTO>> PostEnte(CreaEModificaEnteDTO dto)
        {
            var identita = HttpContext.Items[IdentitaUtente.HttpContextKey] as IdentitaUtente;
            if (identita == null)
                return BadRequest("Utente non trovato");
            var ente = new Ente
            {
                Nome = dto.Nome,
                DescrizioneEnte = dto.DescrizioneEnte,
                Telefono1 = dto.Telefono1,
                Telefono2 = dto.Telefono2,
                Fax = dto.Fax,
                Email = dto.Email,
                Indirizzo = dto.Indirizzo,
                Url = dto.Url,
                Contatto = dto.Contatto,
                IdUtenteCreazione = identita.IdUtente,
                IdCitta = dto.IdCitta
            };

            _context.Enti.Add(ente);
            await _context.SaveChangesAsync();

            // Mappiamo l'oggetto appena creato nel DTO di risposta
            var ruoloDto = new CreaEModificaEnteDTO
            {
                Nome = ente.Nome,
                DescrizioneEnte = ente.DescrizioneEnte,
                Telefono1 = ente.Telefono1,
                Telefono2 = ente.Telefono2,
                Fax = ente.Fax,
                Email = ente.Email,
                Indirizzo = ente.Indirizzo,
                Url = ente.Url,
                Contatto = ente.Contatto,
                IdCitta = ente.IdCitta
            };

            return CreatedAtAction(nameof(GetEnte), new { id = ente.IdEnte }, ruoloDto);
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

            // Controllo: l'ente non può essere eliminato se ha schede collegate
            bool haSchedeCollegate = await _context.Schede.AnyAsync(s => s.IdEnte == id);
            if (haSchedeCollegate)
            {
                return Conflict("Impossibile eliminare l'ente: sono presenti schede collegate");
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
