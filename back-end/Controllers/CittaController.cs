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
        [HttpGet("{id}")]
        public async Task<ActionResult<Citta>> GetCitta(int id)
        {
            var citta = await _context.Citta.FindAsync(id);

            if (citta == null)
            {
                return NotFound();
            }

            return citta;
        }

        private bool CittaExists(int id)
        {
            return _context.Citta.Any(e => e.IdCitta == id);
        }
    }
}
