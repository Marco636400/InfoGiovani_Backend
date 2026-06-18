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
    public class UtenteController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UtenteController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Utente
        [HttpGet]
        public async Task<IActionResult> GetUtenti(int idUtenteLoggato)
        {
            // Prendiamo tutti gli utenti DAL DATABASE TRANNE quello loggato
            var utentiFiltrati = await _context.Utenti
                .Where(u => u.IdUtente != idUtenteLoggato)
                .Select(u => new
                {
                    IdUtente = u.IdUtente,
                    Nome = u.Nome,
                    Cognome = u.Cognome,
                    Username = u.Username,
                    Disabilita = u.Disabilita,
                    IdRuolo = u.IdRuolo,
                    IdUtenteCreazione = u.IdUtenteCreazione,
                    DataCreazione = u.DataCreazione,
                    IdUtenteModifica = u.IdUtenteModifica,
                    DataUltimaModifica = u.DataUltimaModifica,
                    UltimoLogin = u.UltimoLogin,
                    NomeUtente = u.NomeUtente
                })
                .ToListAsync();

            // Se la lista è vuota (ovvero non ci sono ALTRI utenti registrati), restituisce NotFound
            if (!utentiFiltrati.Any())
            {
                return NotFound("Nessun altro utente trovato nel sistema.");
            }

            return Ok(utentiFiltrati);
        }

        // GET: api/Utente/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Utente>> GetUtente(int id)
        {
            var utente = await _context.Utenti.FindAsync(id);

            if (utente == null)
            {
                return NotFound();
            }

            return utente;
        }

        // PUT: api/Utente/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUtente(int id, Utente utente, ModificaUtenteDTO dto)
        {
            if (id != utente.IdUtente)
            {
                return BadRequest();
            }

            if (await _context.Utenti.AnyAsync(u => u.Username == dto.Username))
                return Conflict(new { error = "Username già in uso" });


            _context.Entry(utente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UtenteExists(id))
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

        // POST: api/Utente
        [HttpPost]
        public async Task<ActionResult<Utente>> PostUtente(CreazioneUtenteDTO dto)
        {
            if (await _context.Utenti.AnyAsync(u => u.Username == dto.Username))
                return Conflict(new { error = "Username già in uso" });

            var utente = new Utente
            {
                Nome = dto.Nome,
                Cognome = dto.Cognome,
                Username = dto.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IdRuolo = dto.IdRuolo,
                IdUtenteCreazione = dto.IdUtenteLoggato
            };

            _context.Utenti.Add(utente);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUtente", new { id = utente.IdUtente }, utente);
        }
        // DELETE: api/Utente/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUtente(int id, int idUtenteLoggato)//sostituire idutenteloggato con utente loggato jwt
        {
            // 1. CONTROLLO CRUCIALE: L'utente non può auto-eliminarsi
            if (id == idUtenteLoggato)
            {
                return BadRequest("Operazione non consentita: non puoi eliminare il tuo stesso account.");
            }

            // 2. Cerchiamo l'utente nel database
            var utente = await _context.Utenti.FindAsync(id);
            if (utente == null)
            {
                return NotFound($"Utente con ID {id} non trovato.");
            }

            // 3. Se supera i controlli, procediamo con l'eliminazione
            _context.Utenti.Remove(utente);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        private bool UtenteExists(int id)
        {
            return _context.Utenti.Any(e => e.IdUtente == id);
        }
    }
}