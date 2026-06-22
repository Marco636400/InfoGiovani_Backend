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
    [Route("[controller]")]
    [ApiController]
    public class ProvinceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProvinceController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Province
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Province>>> GetProvince()
        {
            return await _context.Province.ToListAsync();
        }

        // GET: api/Province/5
        [HttpGet("{idRegione}")]
        public async Task<IActionResult> GetProvincePerRegione(int idRegione)
        {
            // Filtriamo la tabella Province cercando solo quelle che hanno l'IdRegione richiesto
            var provinceFiltrate = await _context.Province
                .Where(p => p.IdRegione == idRegione)
                .Select(p => new
                {
                    IdProvincia = p.IdProvincia,
                    IdRegione = p.IdRegione,
                    SiglaProvincia = p.SiglaProvincia,
                    NomeProvincia = p.NomeProvincia
                })
                .ToListAsync();

            // Se la lista è vuota, significa che la regione non esiste o non ha province caricate
            if (!provinceFiltrate.Any())
            {
                return NotFound($"Nessuna provincia trovata per la regione con ID {idRegione}.");
            }

            return Ok(provinceFiltrate);
        }

        private bool ProvinceExists(int id)
        {
            return _context.Province.Any(e => e.IdProvincia == id);
        }
    }
}
