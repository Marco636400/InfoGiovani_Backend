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
        [HttpGet("{id}")]
        public async Task<ActionResult<Province>> GetProvince(int id)
        {
            var province = await _context.Province.FindAsync(id);

            if (province == null)
            {
                return NotFound();
            }

            return province;
        }

        private bool ProvinceExists(int id)
        {
            return _context.Province.Any(e => e.IdProvincia == id);
        }
    }
}
