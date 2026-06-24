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
        public async Task<ActionResult<IEnumerable<SchedaDTO>>> GetSchede([FromQuery] int? idCategoria = null, [FromQuery] List<int>? idEnti = null, [FromQuery] string? testo = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            //controllo per valori < 0 
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            //verifica dll'identità di chi chiama e controllo dei ruoli
            var identita = HttpContext.Items[IdentitaUtente.HttpContextKey] as IdentitaUtente;
            bool puoVederePrivate = identita?.CanViewCard ?? false;
            bool puoVedereDisabilitate = identita?.CanCreateUser ?? false;

            var query = _context.Schede.AsQueryable();

            if (!puoVederePrivate)
                query = query.Where(s => !s.IsPrivate);

            if (!puoVedereDisabilitate)
                query = query.Where(s => !s.Disabilita);

            if (idCategoria.HasValue)
                query = query.Where(s => s.CategorieSchede.Any(cs => cs.IdCategoria == idCategoria.Value));

            //la scheda deve appartenere ad almeno uno degli enti selezionati 
            if (idEnti != null && idEnti.Count > 0)
                query = query.Where(s => s.IdEnte.HasValue && idEnti.Contains(s.IdEnte.Value));

            int totalRecords;
            int totalPages;
            List<SchedaDTO> schedePaginateDto;

            //se non c'è testo della barra di ricerca fa ordinamento e paginazione in SQL
            if (string.IsNullOrWhiteSpace(testo))
            {
                totalRecords = await query.CountAsync();

                query = query.OrderByDescending(s => s.DataCreazione);

                schedePaginateDto = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new SchedaDTO
                    {
                        IdScheda = s.IdScheda,
                        CodAlfabetico = s.CodAlfabetico,
                        CodNumerico = s.CodNumerico,
                        Titolo = s.Titolo,
                        Descrizione = s.Descrizione,
                        IdEnte = s.IdEnte,
                        DataScadenza = s.DataScadenza,
                        Categorie = s.CategorieSchede.Select(cs => new CategoriaSchedaInfoDTO
                        {
                            IdCategoria = cs.IdCategoria,
                            Descrizione = cs.Categoria.Descrizione
                        }).ToList()
                    })
                    .ToListAsync();

            }
            else
            {
                //ricerca testuale a 3 livelli su titolo e descrizione, il ranking per tier richiede il calcolo in memoria, quindi materializziamo tutti i candidati (già filtrati per ente/categoria/visibilità) prima di ordinare e paginare.
                var candidati = await query
                    .Select(s => new SchedaDTO
                    {
                        IdScheda = s.IdScheda,
                        CodAlfabetico = s.CodAlfabetico,
                        CodNumerico = s.CodNumerico,
                        Titolo = s.Titolo,
                        Descrizione = s.Descrizione,
                        IdEnte = s.IdEnte,
                        DataScadenza = s.DataScadenza,
                        Categorie = s.CategorieSchede.Select(cs => new CategoriaSchedaInfoDTO
                        {
                            IdCategoria = cs.IdCategoria,
                            Descrizione = cs.Categoria.Descrizione
                        }).ToList()
                    })
                    .ToListAsync();

                var risultatiOrdinati = candidati
                    .Select(s => new
                    {
                        Dto = s,
                        Punteggio = RicercaTestualeService.CalcolaPunteggio(testo, s.Titolo, s.Descrizione)
                    })
                    .Where(x => x.Punteggio.HasValue)
                    .OrderBy(x => x.Punteggio!.Value)
                    .ThenByDescending(x => x.Dto.IdScheda) // ASSUNZIONE: tie-break non specificato, vedi nota
                    .Select(x => x.Dto)
                    .ToList();

                totalRecords = risultatiOrdinati.Count;

                schedePaginateDto = risultatiOrdinati
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }
            totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var response = new PagedResponseDTO<SchedaDTO>
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
        [Authorize]
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
        //[Authorize]
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
        [Authorize]
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
