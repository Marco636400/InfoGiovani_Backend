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
    public class UtenteController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UtenteController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Utente
        //[Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetUtenti()
        {
            var identita = HttpContext.Items[IdentitaUtente.HttpContextKey] as IdentitaUtente;
            if (identita == null)
                return BadRequest("Utente non trovato");

            var query = _context.Utenti.AsQueryable();

            bool puoVedereDisabilitate = identita?.CanCreateUser ?? false;

            if (!puoVedereDisabilitate)
                query = query.Where(s => !s.Disabilita);

            var utentiFiltrati = await query
                            .Where(u => u.IdUtente != identita.IdUtente)
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
        //[Authorize(Policy = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetUtente(int id)
        {
            var identita = HttpContext.Items[IdentitaUtente.HttpContextKey] as IdentitaUtente;
            if (identita == null)
                return BadRequest("Utente non trovato");

            var query = _context.Utenti.AsQueryable();

            bool puoVedereDisabilitate = identita?.CanCreateUser ?? false;

            if (!puoVedereDisabilitate)
                query = query.Where(s => !s.Disabilita);


            var utente = await query
                .Where(u => u.IdUtente == id)
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
                }).FirstOrDefaultAsync();

            if (utente == null)
            {
                return NotFound();
            }

            return Ok(utente);
        }

        // PUT: api/Utente/5
        //[Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUtente(int id, ModificaUtenteDTO dto)
        {
            var identita = HttpContext.Items[IdentitaUtente.HttpContextKey] as IdentitaUtente;
            if (identita == null)
                return BadRequest("Utente non trovato");
            var utente = await _context.Utenti.FindAsync(id);
            if (utente == null)
            {
                return NotFound();
            }

            // Controllo univocità username, escludendo l'utente stesso
            bool usernameInUso = await _context.Utenti
                .AnyAsync(u => u.Username == dto.Username && u.IdUtente != id);
            if (usernameInUso)
            {
                return Conflict(new { error = "Username già in uso" });
            }

            utente.Nome = dto.Nome ?? utente.Nome;
            utente.Cognome = dto.Cognome;
            if (!string.IsNullOrWhiteSpace(dto.Username))
                utente.Username = dto.Username;
            if (!string.IsNullOrWhiteSpace(dto.Password))
                utente.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            utente.IdRuolo = dto.IdRuolo;
            utente.Disabilita = dto.Disabilita;
            utente.IdUtenteModifica = identita.IdUtente;
            utente.DataUltimaModifica = DateTime.Now;

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

        //post
        //[Authorize(Policy = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Utente>> PostUtente(CreazioneUtenteDTO dto)
        {
            var identita = HttpContext.Items[IdentitaUtente.HttpContextKey] as IdentitaUtente;
            if (identita == null)
                return BadRequest("Utente non trovato");
            if (string.IsNullOrWhiteSpace(dto.Username))
            {
                return BadRequest("L'Username è obbligatorio e non può essere vuoto.");
            }
            if (await _context.Utenti.AnyAsync(u => u.Username == dto.Username))
                return Conflict(new { error = "Username già in uso" });

            var utente = new Utente
            {
                Nome = dto.Nome,
                Cognome = dto.Cognome,
                Username = dto.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IdRuolo = dto.IdRuolo,
                IdUtenteCreazione = identita.IdUtente
            };

            _context.Utenti.Add(utente);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUtente", new { id = utente.IdUtente });
        }
        // DELETE: api/Utente/5
        //[Authorize(Policy = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUtente(int id)
        {
            var identita = HttpContext.Items[IdentitaUtente.HttpContextKey] as IdentitaUtente;
            if (identita == null)
                return BadRequest("Utente non trovato");
            //l'utente non può autoeliminarsi
            if (id == identita.IdUtente)
            {
                return BadRequest("non puoi eliminare il tuo stesso account.");
            }

            var utente = await _context.Utenti.FindAsync(id);
            if (utente == null)
            {
                return NotFound($"Utente non trovato.");
            }

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