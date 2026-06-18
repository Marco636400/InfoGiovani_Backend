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
[Route("api/[controller]")]
[ApiController]
public class CategoriaController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriaController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Categoria
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetCategoriaDTO>>> GetCategorie()
    {
        var categorie = await _context.Categorie
            .Select(c => new GetCategoriaDTO
            {
                IdCategoria = c.IdCategoria,
                IdParents = c.IdParents,
                Descrizione = c.Descrizione,
                Disabilita = c.Disabilita,
                IsPrivate = c.IsPrivate
            })
            .ToListAsync();

        return Ok(categorie);
    }

    // GET: api/Categoria/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GetCategoriaDTO>> GetCategoria(int id)
    {
        var categoria = await _context.Categorie
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
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCategoria(int id, CreaEModificaCategoriaDTO dto)
    {
        var categoria = await _context.Categorie.FindAsync(id);
        if (categoria == null)
        {
            return NotFound();
        }

        categoria.IdParents = dto.IdParents;
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
    [HttpPost]
    public async Task<ActionResult<GetCategoriaDTO>> PostCategoria(CreaEModificaCategoriaDTO dto)
    {
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

    // DELETE: api/Categoria/5 — logica esistente invariata
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
            return BadRequest("Impossibile eliminare la categoria: sono presenti schede collegate");
        }

        bool haSottocategorie = await _context.Categorie
            .AnyAsync(c => c.IdParents == id);
        if (haSottocategorie)
        {
            return BadRequest("Impossibile eliminare la categoria: sono presenti sottocategorie collegate");
        }

        _context.Categorie.Remove(categoria);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CategoriaExists(int id)
    {
        return _context.Categorie.Any(e => e.IdCategoria == id);
    }
}