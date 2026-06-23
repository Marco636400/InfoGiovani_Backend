using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InfoGiovani_Back.Models;
using InfoGiovani_Back.DTOs;
using Microsoft.AspNetCore.Authorization;
using InfoGiovani_Back.Middleware;

namespace back_end.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RuoliController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RuoliController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Ruoli        
        //[Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetRuoliDTO>>> GetRuoli()
        {
            var ruoli = await _context.Ruoli
                .Select(r => new GetRuoliDTO
                {
                    IdRuolo = r.IdRuolo,
                    NomeRuolo = r.NomeRuolo,
                    CanCreateUser = r.CanCreateUser,
                    CanCreateEntity = r.CanCreateEntity,
                    CanViewCard = r.CanViewCard,
                    IdUtenteCreazione = r.IdUtenteCreazione,
                    DataCreazione = r.DataCreazione,
                    IdUtenteModifica = r.IdUtenteModifica,
                    DataUltimaModifica = r.DataUltimaModifica
                })
                .ToListAsync();

            return Ok(ruoli);
        }

        // GET: api/Ruoli/5
        //[Authorize(Policy = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<GetRuoliDTO>> GetRuoli(int id)
        {
            var ruoli = await _context.Ruoli
            .Where(r => r.IdRuolo == id)
            .Select(r => new GetRuoliDTO
            {
                IdRuolo = r.IdRuolo,
                NomeRuolo = r.NomeRuolo,
                CanCreateUser = r.CanCreateUser,
                CanCreateEntity = r.CanCreateEntity,
                CanViewCard = r.CanViewCard,
                IdUtenteCreazione = r.IdUtenteCreazione,
                DataCreazione = r.DataCreazione,
                IdUtenteModifica = r.IdUtenteModifica,
                DataUltimaModifica = r.DataUltimaModifica
            })
            .FirstOrDefaultAsync();

            if (ruoli == null)
            {
                return NotFound();
            }

            return Ok(ruoli);
        }

        // PUT: api/Ruoli/5
        //[Authorize(Policy = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRuoli(int id, CreaEModificaRuoliDTO dto)
        {
            var identita = HttpContext.Items[IdentitaUtente.HttpContextKey] as IdentitaUtente;
            var ruoli = await _context.Ruoli.FindAsync(id);
            if (ruoli == null)
            {
                return NotFound();
            }
            if (identita == null)
                return BadRequest("Utente non trovato");

            // Aggiorna le proprietà permesse
            ruoli.NomeRuolo = dto.NomeRuolo;
            ruoli.CanCreateUser = dto.CanCreateUser;
            ruoli.CanCreateEntity = dto.CanCreateEntity;
            ruoli.CanViewCard = dto.CanViewCard;

            // Campi di tracciamento per la modifica
            ruoli.IdUtenteModifica = identita.IdUtente;
            ruoli.DataUltimaModifica = DateTime.Now;

            _context.Entry(ruoli).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RuoliExists(id))
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

        // POST: api/Ruoli
        //[Authorize(Policy = "Admin")]
        [HttpPost]
        public async Task<ActionResult<CreaEModificaRuoliDTO>> PostRuoli(CreaEModificaRuoliDTO dto)
        {
            var identita = HttpContext.Items[IdentitaUtente.HttpContextKey] as IdentitaUtente;
            if (identita == null)
                return BadRequest("Utente non trovato");            // Creiamo l'oggetto di database. IdRuolo e DataCreazione vengono gestiti in automatico (private set)
            var ruoli = new Ruoli
            {
                NomeRuolo = dto.NomeRuolo,
                CanCreateUser = dto.CanCreateUser,
                CanCreateEntity = dto.CanCreateEntity,
                CanViewCard = dto.CanViewCard,
                IdUtenteCreazione = identita.IdUtente // Valorizzato dall'utente che invia la richiesta
            };

            _context.Ruoli.Add(ruoli);
            await _context.SaveChangesAsync();

            // Mappiamo l'oggetto appena creato nel DTO di risposta
            var ruoloDto = new CreaEModificaRuoliDTO
            {
                NomeRuolo = ruoli.NomeRuolo,
                CanCreateUser = ruoli.CanCreateUser,
                CanCreateEntity = ruoli.CanCreateEntity,
                CanViewCard = ruoli.CanViewCard,
            };

            return CreatedAtAction(nameof(GetRuoli), new { id = ruoli.IdRuolo }, ruoloDto);
        }

        // DELETE: api/Ruoli/5
        //[Authorize(Policy = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRuoli(int id)
        {
            var ruoli = await _context.Ruoli.FindAsync(id);
            if (ruoli == null)
            {
                return NotFound();
            }

            bool haUtenti = await _context.Utenti.AnyAsync(u => u.IdRuolo == id);
            if (haUtenti)
                return Conflict("Utenti hanno questo ruolo assegnato");

            _context.Ruoli.Remove(ruoli);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RuoliExists(int id)
        {
            return _context.Ruoli.Any(e => e.IdRuolo == id);
        }
    }
}