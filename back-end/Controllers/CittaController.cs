using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InfoGiovani_Back.Models;
using Microsoft.AspNetCore.Authorization;

namespace back_end.Controllers
{
    [Route("api/[controller]")]
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
        private bool CittaExists(int id)
        {
            return _context.Citta.Any(e => e.IdCitta == id);
        }
    }
}
