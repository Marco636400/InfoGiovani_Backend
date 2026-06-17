using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InfoGiovani_Back.Models;



namespace back_end.Controllers

{

    [Route("api/[controller]")]

    [ApiController]

    public class AllegatoController : ControllerBase

    {

        private readonly AppDbContext _context;



        public AllegatoController(AppDbContext context)

        {

            _context = context;

        }



        // GET: api/Allegato

        [HttpGet]

        public async Task<ActionResult<IEnumerable<Allegato>>> GetAllegati()

        {

            return await _context.Allegati.ToListAsync();

        }



        // GET: api/Allegato/5

        [HttpGet("{id}")]

        public async Task<ActionResult<Allegato>> GetAllegato(int id)

        {

            var allegato = await _context.Allegati.FindAsync(id);



            if (allegato == null)

            {

                return NotFound();

            }



            return allegato;

        }



        // PUT: api/Allegato/5

        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

        [HttpPut("{id}")]

        public async Task<IActionResult> PutAllegato(int id, Allegato allegato)

        {

            if (id != allegato.IdAllegato)

            {

                return BadRequest();

            }



            _context.Entry(allegato).State = EntityState.Modified;



            try

            {

                await _context.SaveChangesAsync();

            }

            catch (DbUpdateConcurrencyException)

            {

                if (!AllegatoExists(id))

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



        // POST: api/Allegato

        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

        [HttpPost]

        public async Task<ActionResult<Allegato>> PostAllegato(Allegato allegato)

        {

            _context.Allegati.Add(allegato);

            await _context.SaveChangesAsync();



            return CreatedAtAction("GetAllegato", new { id = allegato.IdAllegato }, allegato);

        }



        // DELETE: api/Allegato/5

        [HttpDelete("{id}")]

        public async Task<IActionResult> DeleteAllegato(int id)

        {

            var allegato = await _context.Allegati.FindAsync(id);

            if (allegato == null)

            {

                return NotFound();

            }



            _context.Allegati.Remove(allegato);

            await _context.SaveChangesAsync();



            return NoContent();

        }



        private bool AllegatoExists(int id)

        {

            return _context.Allegati.Any(e => e.IdAllegato == id);

        }

    }

}