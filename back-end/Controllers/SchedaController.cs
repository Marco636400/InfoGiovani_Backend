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
    public class SchedaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SchedaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Scheda
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetSchedaDTO>>> GetSchede([FromQuery] int? idCategoria = null, [FromQuery] List<int>? idEnti = null, [FromQuery] string? testo = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // Controllo per valori < 0 
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            // Verifica dell'identità di chi chiama e controllo dei ruoli
            var identita = HttpContext.Items[IdentitaUtente.HttpContextKey] as IdentitaUtente;
            bool puoVederePrivate = identita != null;
            bool puoVedereDisabilitate = identita?.CanCreateEntity ?? false;

            // Precarichiamo tutte le categorie per la valutazione dell'albero genealogico
            var tutteLeCategorie = await _context.Categorie.ToListAsync();

            // Query di base senza filtri di stato (saranno calcolati dinamicamente)
            var query = _context.Schede.AsQueryable();

            if (idCategoria.HasValue)
                query = query.Where(s => s.CategorieSchede.Any(cs => cs.IdCategoria == idCategoria.Value));

            // La scheda deve appartenere ad almeno uno degli enti selezionati 
            if (idEnti != null && idEnti.Count > 0)
                query = query.Where(s => s.IdEnte.HasValue && idEnti.Contains(s.IdEnte.Value));

            // Estraiamo i record candidati per elaborarli in memoria
            var candidatiDb = await query
                .Select(s => new
                {
                    Scheda = s,
                    IdCategorieCollegate = s.CategorieSchede.Select(cs => cs.IdCategoria).ToList(),
                    CategorieDto = s.CategorieSchede.Select(cs => new GetCategoriaSchedaDTO
                    {
                        IdCategoria = cs.IdCategoria,
                        Descrizione = cs.Categoria.Descrizione
                    }).ToList()
                })
                .ToListAsync();

            var schedeProcessate = new List<GetSchedaDTO>();

            foreach (var candidato in candidatiDb)
            {
                // Una scheda è disabilitata se lo è lei stessa o se lo è ALMENO UNA delle categorie collegate
                bool schedaIsDisabilitata = candidato.Scheda.Disabilita ||
                    candidato.IdCategorieCollegate.Any(idCat => CalcolaDisabilitazioneCategoriaRicorsiva(idCat, tutteLeCategorie));

                // Una scheda è privata se lo è lei stessa o se lo è ALMENO UNA delle categorie collegate
                bool schedaIsPrivata = candidato.Scheda.IsPrivate ||
                    candidato.IdCategorieCollegate.Any(idCat => CalcolaPrivacyCategoriaRicorsiva(idCat, tutteLeCategorie));

                // CONTROLLI DI SICUREZZA API (Filtro definitivo)
                if (schedaIsDisabilitata && !puoVedereDisabilitate) continue;
                if (schedaIsPrivata && !puoVederePrivate) continue;

                // Se l'utente ha i permessi, mappiamo il DTO alterando i flag solo visivamente
                schedeProcessate.Add(new GetSchedaDTO
                {
                    IdScheda = candidato.Scheda.IdScheda,
                    Titolo = candidato.Scheda.Titolo,
                    Descrizione = candidato.Scheda.Descrizione,
                    IdEnte = candidato.Scheda.IdEnte,
                    DataCreazione = candidato.Scheda.DataCreazione,
                    Disabilita = schedaIsDisabilitata,
                    IsPrivate = schedaIsPrivata,
                    Categorie = candidato.CategorieDto
                });
            }

            int totalRecords;
            List<GetSchedaDTO> schedePaginateDto;

            // Se non c'è testo della barra di ricerca ordina per data
            if (string.IsNullOrWhiteSpace(testo))
            {
                var risultatiOrdinati = schedeProcessate
                    .OrderByDescending(s => s.DataCreazione)
                    .ToList();

                totalRecords = risultatiOrdinati.Count;

                schedePaginateDto = risultatiOrdinati
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }
            else
            {
                // Ricerca testuale a 3 livelli calcolata sui candidati validati
                var risultatiOrdinati = schedeProcessate
                    .Select(s => new
                    {
                        Dto = s,
                        Punteggio = RicercaTestualeService.CalcolaPunteggioTotale(testo, s.Titolo, s.Descrizione)
                    })
                    .Where(x => x.Punteggio.HasValue)
                    .OrderBy(x => x.Punteggio!.Value)
                    .ThenByDescending(x => x.Dto.IdScheda)
                    .Select(x => x.Dto)
                    .ToList();

                totalRecords = risultatiOrdinati.Count;

                schedePaginateDto = risultatiOrdinati
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }

            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var response = new GetPagineDTO<GetSchedaDTO>
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages == 0 ? 1 : totalPages,
                Schede = schedePaginateDto
            };

            return Ok(response);
        }

        // GET: api/Scheda/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GetSchedaDTO>> GetScheda(int id)
        {
            var identita = HttpContext.Items[IdentitaUtente.HttpContextKey] as IdentitaUtente;

            var query = _context.Schede.AsQueryable();

            bool puoVederePrivate = identita != null;
            bool puoVedereDisabilitate = identita?.CanCreateEntity ?? false;

            if (!puoVederePrivate)
                query = query.Where(s => !s.IsPrivate);

            if (!puoVedereDisabilitate)
                query = query.Where(s => !s.Disabilita);

            var schedaDto = await query
                .Where(s => s.IdScheda == id)
                .Select(s => new GetSchedaDTO
                {
                    IdScheda = s.IdScheda,
                    Titolo = s.Titolo,
                    Descrizione = s.Descrizione,
                    IdEnte = s.IdEnte,
                    DataCreazione = s.DataCreazione,
                    DataUltimaModifica = s.DataUltimaModifica,
                    Disabilita = s.Disabilita,
                    IsPrivate = s.IsPrivate,
                    Categorie = s.CategorieSchede.Select(cs => new GetCategoriaSchedaDTO
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
        [Authorize(Policy = "Entity")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSchede(int id, ModificaSchedaDTO dto)
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
            schede.Titolo = dto.Titolo;
            schede.Descrizione = dto.Descrizione;
            schede.IdEnte = dto.IdEnte;
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

        private bool SchedaExists(int id)
        {
            return _context.Schede.Any(e => e.IdScheda == id);
        }

        // POST: api/Scheda
        [Authorize(Policy = "Entity")]
        [HttpPost]
        public async Task<ActionResult<CreaSchedaDTO>> PostScheda(CreaSchedaDTO dto)
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
                Titolo = dto.Titolo,
                Descrizione = dto.Descrizione,
                IdEnte = dto.IdEnte, // Valorizzato dall'utente che invia la richiesta
                IdUtenteCreazione = identita.IdUtente,
                IsPrivate = dto.IsPrivate,
                Disabilita = dto.Disabilita
            };

            _context.Schede.Add(schede);
            await _context.SaveChangesAsync();
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
            }

            // Mappiamo l'oggetto appena creato nel DTO di risposta
            var ruoloDto = new CreaSchedaDTO
            {
                Titolo = schede.Titolo,
                Descrizione = schede.Descrizione,
                IdEnte = schede.IdEnte,
                IsPrivate = schede.IsPrivate
            };

            return CreatedAtAction(nameof(GetScheda), new { id = schede.IdScheda }, ruoloDto);
        }


        // DELETE: api/Scheda/5
        [Authorize(Policy = "Entity")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScheda(int id)
        {
            // 1. Cerchiamo la scheda
            var scheda = await _context.Schede.FindAsync(id);
            if (scheda == null)
            {
                return NotFound();
            }

            // Usiamo una transazione per essere sicuri che entrambe le eliminazioni vadano a buon fine
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 2. Eliminiamo prima tutti i legami nella tabella molti-a-molti 'categorieschede'
                var relazioni = _context.CategorieSchede.Where(cs => cs.IdScheda == id);
                _context.CategorieSchede.RemoveRange(relazioni);

                // 3. Ora che la scheda è "libera", possiamo eliminarla dalla tabella principale
                _context.Schede.Remove(scheda);

                // Salva tutto nel database e conferma la transazione
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                // Se qualcosa va storto, annulla le modifiche per non corrompere i dati
                await transaction.RollbackAsync();
                return StatusCode(500, $"Errore del database durante l'eliminazione: {ex.Message}");
            }
        }
        [NonAction]
        private bool CalcolaDisabilitazioneCategoriaRicorsiva(int? idCategoriaCorrente, List<Categoria> tutteLeCategorie)
        {
            if (!idCategoriaCorrente.HasValue) return false;

            var corrente = tutteLeCategorie.FirstOrDefault(c => c.IdCategoria == idCategoriaCorrente.Value);
            if (corrente == null) return false;
            if (corrente.Disabilita) return true;

            // Chiamata ricorsiva usando la tua proprietà IdParents
            return CalcolaDisabilitazioneCategoriaRicorsiva(corrente.IdParents, tutteLeCategorie);
        }

        [NonAction]
        private bool CalcolaPrivacyCategoriaRicorsiva(int? idCategoriaCorrente, List<Categoria> tutteLeCategorie)
        {
            if (!idCategoriaCorrente.HasValue) return false;

            var corrente = tutteLeCategorie.FirstOrDefault(c => c.IdCategoria == idCategoriaCorrente.Value);
            if (corrente == null) return false;
            if (corrente.IsPrivate) return true;

            // Chiamata ricorsiva usando la tua proprietà IdParents
            return CalcolaPrivacyCategoriaRicorsiva(corrente.IdParents, tutteLeCategorie);
        }
    }
}

