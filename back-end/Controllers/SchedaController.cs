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
    public class SchedaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SchedaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Scheda
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SchedaDTO>>> GetSchede([FromQuery] int? idCategoria, [FromQuery] int? idEnte, [FromQuery] string? testo)
        {
            var identita = HttpContext.Items[IdentitaUtente.HttpContextKey] as IdentitaUtente;

            var query = _context.Schede.AsQueryable();

            bool puoVederePrivate = identita?.CanViewCard ?? false;
            bool puoVedereDisabilitate = identita?.CanCreateUser ?? false;

            if (!puoVederePrivate)
                query = query.Where(s => !s.IsPrivate);

            if (!puoVedereDisabilitate)
                query = query.Where(s => !s.Disabilita);

            if (idCategoria.HasValue)
                query = query.Where(s => s.CategorieSchede.Any(cs => cs.IdCategoria == idCategoria.Value));

            if (idEnte.HasValue)
                query = query.Where(s => s.IdEnte == idEnte.Value);

            if (!string.IsNullOrWhiteSpace(testo))
            {
                var t = testo.Trim();
                query = query.Where(s => s.Titolo.Contains(t) || (s.Descrizione != null && s.Descrizione.Contains(t)));
            }

            var risultato = await query
                .Select(s => new SchedaDTO
                {
                    IdScheda = s.IdScheda,
                    CodAlfabetico = s.CodAlfabetico,
                    CodNumerico = s.CodNumerico,
                    Titolo = s.Titolo,
                    Descrizione = s.Descrizione,
                    IdEnte = s.IdEnte,
                    DataScadenza = s.DataScadenza,
                    IsPrivate = s.IsPrivate,
                    Disabilita = s.Disabilita,
                    Categorie = s.CategorieSchede.Select(cs => new CategoriaSchedaInfoDTO
                    {
                        IdCategoria = cs.IdCategoria,
                        Descrizione = cs.Categoria.Descrizione
                    }).ToList()
                })
                .ToListAsync();

            return Ok(risultato);
        }

        // GET: api/Scheda/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SchedaDTO>> GetScheda(int id)
        {
            var identita = HttpContext.Items[IdentitaUtente.HttpContextKey] as IdentitaUtente;

            var query = _context.Schede.AsQueryable();

            bool puoVederePrivate = identita?.CanViewCard ?? false;
            bool puoVedereDisabilitate = identita?.CanCreateUser ?? false;

            if (!puoVederePrivate)
                query = query.Where(s => !s.IsPrivate);

            if (!puoVedereDisabilitate)
                query = query.Where(s => !s.Disabilita);

            var schedaDto = await query
                .Where(s => s.IdScheda == id)
                .Select(s => new SchedaDTO
                {
                    IdScheda = s.IdScheda,
                    CodAlfabetico = s.CodAlfabetico,
                    CodNumerico = s.CodNumerico,
                    Titolo = s.Titolo,
                    Descrizione = s.Descrizione,
                    IdEnte = s.IdEnte,
                    DataScadenza = s.DataScadenza,
                    IsPrivate = s.IsPrivate,
                    Disabilita = s.Disabilita,
                    Categorie = s.CategorieSchede.Select(cs => new CategoriaSchedaInfoDTO
                    {
                        IdCategoria = cs.IdCategoria,
                        Descrizione = cs.Categoria.Descrizione
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (schedaDto == null)
            {
                return NotFound();
            }

            return Ok(schedaDto);
        }

        // PUT: api/Scheda/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSchede(int id, CreaEModificaSchedaDTO dto)
        {
            var identita = HttpContext.Items[IdentitaUtente.HttpContextKey] as IdentitaUtente;
            if (identita == null)
                return BadRequest("Utente non trovato");
            if (string.IsNullOrWhiteSpace(dto.Titolo))
            {
                return BadRequest("Il titolo è obbligatorio e non può essere vuoto.");
            }

            // 2. CONTROLLO (Opzionale): Lunghezza massima del titolo (es. massimo 100 caratteri)
            if (dto.Titolo.Length > 100)
            {
                return Conflict("Il titolo non può superare i 100 caratteri.");
            }

            // 3. CONTROLLO: Verifica se esiste già una scheda con lo stesso titolo nel Database
            bool titoloGiaEsistente = await _context.Schede
                .AnyAsync(s => s.Titolo.ToLower() == dto.Titolo.ToLower().Trim() && s.IdScheda != id);

            if (titoloGiaEsistente)
            {
                return Conflict($"Esiste già una scheda con il titolo '{dto.Titolo}'. Scegli un titolo diverso.");
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
            schede.Disabilita = dto.Disabilita;
            schede.IdUtenteModifica = identita.IdUtente;
            schede.DataUltimaModifica = DateTime.Now;


            var esistenti = await _context.CategorieSchede
                .Where(x => x.IdScheda == id)
                .ToListAsync();

            _context.CategorieSchede.RemoveRange(esistenti);

            var nuove = (dto.CategorieSchede ?? [])

                            .Select(idCat => new CategoriaScheda
                            {
                                IdScheda = id,
                                IdCategoria = idCat
                            });

            _context.CategorieSchede.AddRange(nuove);

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
            var identita = HttpContext.Items[IdentitaUtente.HttpContextKey] as IdentitaUtente;
            if (identita == null)
                return BadRequest("Utente non trovato");
            if (string.IsNullOrWhiteSpace(dto.Titolo))
            {
                return BadRequest("Il titolo è obbligatorio e non può essere vuoto.");
            }

            // 2. CONTROLLO (Opzionale): Lunghezza massima del titolo (es. massimo 100 caratteri)
            if (dto.Titolo.Length > 100)
            {
                return Conflict("Il titolo non può superare i 100 caratteri.");
            }

            // 3. CONTROLLO: Verifica se esiste già una scheda con lo stesso titolo nel Database
            bool titoloGiaEsistente = await _context.Schede
                .AnyAsync(s => s.Titolo.ToLower().Trim() == dto.Titolo.ToLower().Trim());

            if (titoloGiaEsistente)
            {
                return Conflict($"Esiste già una scheda con il titolo '{dto.Titolo}'. Scegli un titolo diverso.");
            }
            // Creiamo l'oggetto di database. IdScheda e DataScadenza vengono gestiti in automatico (private set)
            var schede = new Scheda
            {
                CodNumerico = dto.CodNumerico,
                CodAlfabetico = dto.CodAlfabetico,
                Titolo = dto.Titolo,
                Descrizione = dto.Descrizione,
                IdEnte = dto.IdEnte, // Valorizzato dall'utente che invia la richiesta
                IdUtenteCreazione = identita.IdUtente,
                DataScadenza = dto.DataScadenza,
                IsPrivate = dto.IsPrivate,
                Disabilita = dto.Disabilita
            };

            _context.Schede.Add(schede);

            if (dto.CategorieSchede?.Any() == true)
            {
                var categorie = dto.CategorieSchede
                    .Distinct()
                    .Select(id => new CategoriaScheda
                    {
                        IdScheda = schede.IdScheda,
                        IdCategoria = id
                    });

                _context.CategorieSchede.AddRange(categorie);
                await _context.SaveChangesAsync();
            }

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
