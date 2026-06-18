using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InfoGiovani_Back.Models;
using InfoGiovani_Back.DTOs;

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
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSchede(int id, CreaEModificaSchedaDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Titolo))
            {
                return BadRequest("Il titolo è obbligatorio e non può essere vuoto.");
            }

            // 2. CONTROLLO (Opzionale): Lunghezza massima del titolo (es. massimo 100 caratteri)
            if (dto.Titolo.Length > 100)
            {
                return BadRequest("Il titolo non può superare i 100 caratteri.");
            }

            // 3. CONTROLLO: Verifica se esiste già una scheda con lo stesso titolo nel Database
            bool titoloGiaEsistente = await _context.Schede
                .AnyAsync(s => s.Titolo.ToLower() == dto.Titolo.ToLower().Trim()&& s.IdScheda != id);

            if (titoloGiaEsistente)
            {
                return BadRequest($"Esiste già una scheda con il titolo '{dto.Titolo}'. Scegli un titolo diverso.");
            }
            var schede = await _context.Schede.FindAsync(id);
            if (schede == null)
            {
                return NotFound();
            }

            // Aggiorna le proprietà permesse
            schede.CodNumerico = dto.CodNumerico;
            schede.CodAlfabetico = dto.CodAlfabetico;
            schede.Titolo = dto.Titolo;
            schede.Descrizione = dto.Descrizione;
            schede.IdEnte = dto.IdEnte;
            schede.DataScadenza = dto.DataScadenza;
            schede.IsPrivate = dto.IsPrivate;

            // Campi di tracciamento per la modifica
            schede.IdUtenteModifica = dto.IdUtenteLoggato;
            schede.DataUltimaModifica = DateTime.Now;

            _context.Entry(schede).State = EntityState.Modified;

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
        [HttpPost]
        public async Task<ActionResult<CreaEModificaSchedaDTO>> PostScheda(CreaEModificaSchedaDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Titolo))
            {
                return BadRequest("Il titolo è obbligatorio e non può essere vuoto.");
            }

            // 2. CONTROLLO (Opzionale): Lunghezza massima del titolo (es. massimo 100 caratteri)
            if (dto.Titolo.Length > 100)
            {
                return BadRequest("Il titolo non può superare i 100 caratteri.");
            }

            // 3. CONTROLLO: Verifica se esiste già una scheda con lo stesso titolo nel Database
            bool titoloGiaEsistente = await _context.Schede
                .AnyAsync(s => s.Titolo.ToLower() == dto.Titolo.ToLower().Trim());

            if (titoloGiaEsistente)
            {
                return BadRequest($"Esiste già una scheda con il titolo '{dto.Titolo}'. Scegli un titolo diverso.");
            }
            // Creiamo l'oggetto di database. IdScheda e DataScadenza vengono gestiti in automatico (private set)
            var schede = new Scheda
            {
                CodNumerico = dto.CodNumerico,
                CodAlfabetico = dto.CodAlfabetico,
                Titolo = dto.Titolo,
                Descrizione = dto.Descrizione,
                IdEnte = dto.IdEnte, // Valorizzato dall'utente che invia la richiesta
                IdUtenteCreazione = dto.IdUtenteLoggato,
                DataScadenza = dto.DataScadenza,
                IsPrivate = dto.IsPrivate
            };

            _context.Schede.Add(schede);
            await _context.SaveChangesAsync();

            // Mappiamo l'oggetto appena creato nel DTO di risposta
            var ruoloDto = new CreaEModificaSchedaDTO
            {
                CodNumerico = schede.CodNumerico,
                CodAlfabetico = schede.CodAlfabetico,
                Titolo = schede.Titolo,
                Descrizione = schede.Descrizione,
                IdEnte = schede.IdEnte,
                DataScadenza = schede.DataScadenza,
                IsPrivate = schede.IsPrivate
            };

            return CreatedAtAction(nameof(GetScheda), new { id = schede.IdScheda }, ruoloDto);
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
