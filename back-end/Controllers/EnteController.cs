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
using Microsoft.AspNetCore.Authorization;
using InfoGiovani_Back.Services;

namespace back_end.Controllers
{
    [Route("[controller]")]
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
        public async Task<ActionResult<IEnumerable<GetEnteDTO>>> GetEnti([FromQuery] string? ricerca = null)
        {
            var query = _context.Enti.AsQueryable();

            if (string.IsNullOrWhiteSpace(ricerca))
            {
                var enti = await query
                    .Select(r => new GetEnteDTO
                    {
                        IdEnte = r.IdEnte,
                        Nome = r.Nome,
                        DescrizioneEnte = r.DescrizioneEnte,
                        IdCitta = r.IdCitta,
                        Telefono1 = r.Telefono1,
                        Telefono2 = r.Telefono2,
                        Fax = r.Fax,
                        Email = r.Email,
                        Indirizzo = r.Indirizzo,
                        Url = r.Url,
                        Contatto = r.Contatto,
                        IdUtenteCreazione = r.IdUtenteCreazione,
                        DataCreazione = r.DataCreazione,
                        IdUtenteModifica = r.IdUtenteModifica,
                        DataUltimaModifica = r.DataUltimaModifica
                    })
                    .ToListAsync();

                return Ok(enti);
            }

            //ricerca a 3 livelli su titolo, descrizione e luogo
            var candidati = await query
                .Select(r => new
                {
                    Dto = new GetEnteDTO
                    {
                        IdEnte = r.IdEnte,
                        Nome = r.Nome,
                        DescrizioneEnte = r.DescrizioneEnte,
                        IdCitta = r.IdCitta,
                        Telefono1 = r.Telefono1,
                        Telefono2 = r.Telefono2,
                        Fax = r.Fax,
                        Email = r.Email,
                        Indirizzo = r.Indirizzo,
                        Url = r.Url,
                        Contatto = r.Contatto,
                        IdUtenteCreazione = r.IdUtenteCreazione,
                        DataCreazione = r.DataCreazione,
                        IdUtenteModifica = r.IdUtenteModifica,
                        DataUltimaModifica = r.DataUltimaModifica
                    },
                    NomeCitta = r.Citta != null ? r.Citta.NomeCitta : null
                })
                .ToListAsync();

            var risultati = candidati
                .Select(c => new
                {
                    c.Dto,
                    Punteggio = RicercaTestualeService.CalcolaPunteggio(ricerca, c.Dto.Nome, c.Dto.DescrizioneEnte, c.NomeCitta)
                })
                .Where(x => x.Punteggio.HasValue)
                .OrderBy(x => x.Punteggio!.Value)
                .ThenBy(x => x.Dto.Nome) 
                .Select(x => x.Dto)
                .ToList();

            return Ok(risultati);
        }

        // GET: api/Ente/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GetEnteDTO>> GetEnte(int id)
        {
            var ente = _context.Enti
            .Where(r => r.IdEnte == id)
            .Select(r => new GetEnteDTO
            {
                IdEnte = r.IdEnte,
                Nome = r.Nome,
                DescrizioneEnte = r.DescrizioneEnte,
                IdCitta = r.IdCitta,
                Telefono1 = r.Telefono1,
                Telefono2 = r.Telefono2,
                Fax = r.Fax,
                Email = r.Email,
                Indirizzo = r.Indirizzo,
                Url = r.Url,
                Contatto = r.Contatto,
                IdUtenteCreazione = r.IdUtenteCreazione,
                DataCreazione = r.DataCreazione,
                IdUtenteModifica = r.IdUtenteModifica,
                DataUltimaModifica = r.DataUltimaModifica
            })
            .FirstOrDefaultAsync();

            if (ente == null)
            {
                return NotFound();
            }

            return Ok(ente);
        }
        // PUT: api/Ente/5
        [Authorize(Policy = "Entity")]
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
        [Authorize(Policy = "Entity")]
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
        [Authorize(Policy = "Entity")]
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
