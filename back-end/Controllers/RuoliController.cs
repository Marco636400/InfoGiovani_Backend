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

namespace back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RuoliController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RuoliController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Ruoli
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ruoli>>> GetRuoli()
        {
            return await _context.Ruoli.ToListAsync();
        }

        // GET: api/Ruoli/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Ruoli>> GetRuoli(int id)
        {
            var ruoli = await _context.Ruoli.FindAsync(id);

            if (ruoli == null)
            {
                return NotFound();
            }

            return ruoli;
        }

        // PUT: api/Ruoli/5
        //[Authorize(Policy = "CanCreateUser")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRuoli(int id, CreaEModificaRuoliDTO dto)
        {
            var ruoli = await _context.Ruoli.FindAsync(id);
            if (ruoli == null)
            {
                return NotFound();
            }

            // Aggiorna le proprietà permesse
            ruoli.NomeRuolo = dto.NomeRuolo;
            ruoli.CanCreateUser = dto.CanCreateUser;
            ruoli.CanCreateEntity = dto.CanCreateEntity;
            ruoli.CanViewCard = dto.CanViewCard;

            // Campi di tracciamento per la modifica
            ruoli.IdUtenteModifica = dto.IdUtenteLoggato;
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
        //[Authorize(Policy = "CanCreateUser")]
        [HttpPost]
        public async Task<ActionResult<CreaEModificaRuoliDTO>> PostRuoli(CreaEModificaRuoliDTO dto)
        {
            // Creiamo l'oggetto di database. IdRuolo e DataCreazione vengono gestiti in automatico (private set)
            var ruoli = new Ruoli
            {
                NomeRuolo = dto.NomeRuolo,
                CanCreateUser = dto.CanCreateUser,
                CanCreateEntity = dto.CanCreateEntity,
                CanViewCard = dto.CanViewCard,
                IdUtenteCreazione = dto.IdUtenteLoggato // Valorizzato dall'utente che invia la richiesta
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
                return BadRequest("Utenti hanno questo ruolo assegnato");

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