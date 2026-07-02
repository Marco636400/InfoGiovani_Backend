using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InfoGiovani_Back.Models;
using Microsoft.AspNetCore.Authorization;
using InfoGiovani_Back.DTOs;
using InfoGiovani_Back.Middleware;
using InfoGiovani_Back.Services;
[Route("[controller]")]
[ApiController]
public class CategoriaController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriaController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetCategoriaDTO>>> GetCategorie([FromQuery] string? ricerca = null)
    {
        var identita = HttpContext.Items[IdentitaUtente.HttpContextKey] as IdentitaUtente;
        bool puoVederePrivate = identita != null;
        bool puoVedereDisabilitate = identita?.CanCreateEntity ?? false;

        // 1. Carichiamo TUTTE le categorie pure dal DB per poter calcolare la gerarchia
        var tutteLeCategorie = await _context.Categorie.ToListAsync();

        var categorieProcessate = new List<GetCategoriaDTO>();

        // 2. Calcolo dinamico dello stato ereditato in memoria
        foreach (var cat in tutteLeCategorie)
        {
            // Una categoria è ereditariamente disabilitata se lo è lei stessa o un suo antenato
            bool catIsDisabilitata = cat.Disabilita || CalcolaDisabilitazioneCategoriaRicorsiva(cat.IdParents, tutteLeCategorie);

            // Una categoria è ereditariamente privata se lo è lei stessa o un suo antenato
            bool catIsPrivata = cat.IsPrivate || CalcolaPrivacyCategoriaRicorsiva(cat.IdParents, tutteLeCategorie);

            // CONTROLLI DI SICUREZZA API (Filtro definitivo)
            if (catIsDisabilitata && !puoVedereDisabilitate) continue;
            if (catIsPrivata && !puoVederePrivate) continue;

            // Se supera i controlli, mappiamo il DTO alterando i flag solo visivamente
            categorieProcessate.Add(new GetCategoriaDTO
            {
                IdCategoria = cat.IdCategoria,
                IdParents = cat.IdParents,
                Descrizione = cat.Descrizione,
                Disabilita = catIsDisabilitata,
                // La disabilitazione ha la priorità: se è disabilitata vince sulla privacy nel JSON
                IsPrivate = !catIsDisabilitata && catIsPrivata
            });
        }

        // 3. Gestione della ricerca testuale sui dati validati e filtrati
        if (string.IsNullOrWhiteSpace(ricerca))
        {
            return Ok(categorieProcessate);
        }

        // Ricerca a 3 livelli sulla Descrizione (eseguita in memoria sull'albero già filtrato)
        var risultatiOrdinati = categorieProcessate
            .Select(c => new
            {
                Dto = c,
                Punteggio = RicercaTestualeService.CalcolaPunteggioTotale(ricerca, c.Descrizione)
            })
            .Where(x => x.Punteggio.HasValue)
            .OrderBy(x => x.Punteggio!.Value)
            .ThenBy(x => x.Dto.Descrizione)
            .Select(x => x.Dto)
            .ToList();

        return Ok(risultatiOrdinati);
    }

    // GET: api/Categoria/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GetCategoriaDTO>> GetCategoria(int id)
    {
        var identita = HttpContext.Items[IdentitaUtente.HttpContextKey] as IdentitaUtente;

        var query = _context.Categorie.AsQueryable();

        bool puoVederePrivate = identita != null;
        bool puoVedereDisabilitate = identita?.CanCreateEntity ?? false;

        if (!puoVederePrivate)
            query = query.Where(s => !s.IsPrivate);

        if (!puoVedereDisabilitate)
            query = query.Where(s => !s.Disabilita);


        var categoria = await query
            .Where(c => c.IdCategoria == id)
            .Select(c => new GetCategoriaDTO
            {
                IdCategoria = c.IdCategoria,
                IdParents = c.IdParents,
                Descrizione = c.Descrizione,
                Disabilita = c.Disabilita,
                IsPrivate = c.IsPrivate
            })
            .FirstOrDefaultAsync();

        if (categoria == null)
        {
            return NotFound();
        }

        return categoria;
    }

    // PUT: api/Categoria/5    
    [Authorize(Policy = "Entity")]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCategoria(int id, ModificaCategoriaDTO dto)
    {
        var categoria = await _context.Categorie.FindAsync(id);
        if (categoria == null)
        {
            return NotFound();
        }

        // CONTROLLO DUPLICATO SOLO SE CAMBIA DESCRIZIONE
        if (!string.IsNullOrEmpty(dto.Descrizione) &&
            dto.Descrizione.ToLower() != categoria.Descrizione.ToLower())
        {
            bool nomeUsato = await _context.Categorie
                .AnyAsync(c => c.Descrizione.ToLower() == dto.Descrizione.ToLower());

            if (nomeUsato)
                return Conflict("Esiste già una categoria con questa descrizione");
        }

        categoria.IdParents = dto.IdParents;

        if (!string.IsNullOrEmpty(dto.Descrizione))
            categoria.Descrizione = dto.Descrizione;

        categoria.Disabilita = dto.Disabilita;
        categoria.IsPrivate = dto.IsPrivate;

        _context.Entry(categoria).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CategoriaExists(id))
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


    // POST: api/Categoria
    [Authorize(Policy = "Entity")]
    [HttpPost]
    public async Task<ActionResult<GetCategoriaDTO>> PostCategoria(CreaCategoriaDTO dto)
    {
        if (dto.Descrizione == null || dto.Descrizione.Trim() == "")
        {
            return BadRequest("La descrizione è obbligatoria");
        }

        // CONTROLLO DUPLICATO
        bool categoriaEsiste = await _context.Categorie
            .AnyAsync(c => c.Descrizione.ToLower() == dto.Descrizione.ToLower());

        if (categoriaEsiste)
            return Conflict("Esiste già una categoria con questa descrizione");

        var categoria = new Categoria
        {
            IdParents = dto.IdParents,
            Descrizione = dto.Descrizione,
            Disabilita = dto.Disabilita,
            IsPrivate = dto.IsPrivate
        };

        _context.Categorie.Add(categoria);
        await _context.SaveChangesAsync();

        var risultato = new GetCategoriaDTO
        {
            IdCategoria = categoria.IdCategoria,
            IdParents = categoria.IdParents,
            Descrizione = categoria.Descrizione,
            Disabilita = categoria.Disabilita,
            IsPrivate = categoria.IsPrivate
        };

        return CreatedAtAction(nameof(GetCategoria), new { id = categoria.IdCategoria }, risultato);
    }


    //DELETE: api/Categoria/5
    [Authorize(Policy = "Entity")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategoria(int id)
    {
        var categoria = await _context.Categorie.FindAsync(id);
        if (categoria == null)
        {
            return NotFound();
        }

        bool haSchedeCollegate = await _context.CategorieSchede
            .AnyAsync(cs => cs.IdCategoria == id);
        if (haSchedeCollegate)
        {
            return Conflict("Impossibile eliminare la categoria: sono presenti schede collegate");
        }

        bool haSottocategorie = await _context.Categorie
            .AnyAsync(c => c.IdParents == id);
        if (haSottocategorie)
        {
            return Conflict("Impossibile eliminare la categoria: sono presenti sottocategorie collegate");
        }

        _context.Categorie.Remove(categoria);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CategoriaExists(int id)
    {
        return _context.Categorie.Any(e => e.IdCategoria == id);
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