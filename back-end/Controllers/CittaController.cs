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
    [Route("[controller]")]
    [ApiController]
    public class CittaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CittaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Citta
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Citta>>> GetCitta()
        {
            return await _context.Citta.ToListAsync();
        }

        // GET: api/Citta/5
        [HttpGet("{idProvincia}")]
        public async Task<IActionResult> GetCittaPerProvincia(int idProvincia)
        {
            var cittaFiltrate = await _context.Citta
                .Where(c => c.IdProvincia == idProvincia)
                .Select(c => new
                {
                    IdCitta = c.IdCitta,
                    NomeCitta = c.NomeCitta,
                    IdProvincia = c.IdProvincia
                })
                .ToListAsync();

            // Se l'array è vuoto, lanciamo il NotFound come hai chiesto
            if (!cittaFiltrate.Any())
            {
                return NotFound($"Nessuna città trovata per la provincia con ID {idProvincia}.");
            }

            return Ok(cittaFiltrate);
        }
        // GET: api/Citta/Dettaglio/5
        [HttpGet("Dettaglio/{id}")]
        public async Task<ActionResult<GetDettaglioCittaDTO>> GetCittaConGerarchia(int id)
        {
            var risultato = await _context.Citta
                .Where(c => c.IdCitta == id)
                .Select(c => new GetDettaglioCittaDTO
                {
                    IdCitta = c.IdCitta,
                    NomeCitta = c.NomeCitta,
                    IdProvincia = c.IdProvincia,
                    NomeProvincia = c.Provincia != null ? c.Provincia.NomeProvincia : null,
                    IdRegione = c.Provincia != null ? c.Provincia.IdRegione : (int?)null,
                    NomeRegione = c.Provincia != null ? c.Provincia.Regione.NomeRegione : null
                })
                .FirstOrDefaultAsync();

            if (risultato == null)
                return NotFound();

            return Ok(risultato);
        }
        private bool CittaExists(int id)
        {
            return _context.Citta.Any(e => e.IdCitta == id);
        }
    }
}
