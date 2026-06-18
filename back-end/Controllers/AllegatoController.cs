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



        // GET: api/Allegato/5
        [HttpGet("{idScheda}")]
        public async Task<IActionResult> GetAllegati(int? idScheda)
        {
            // Partiamo dalla query base sugli Allegati
            var query = _context.Allegati.AsQueryable();

            // Se l'utente ha passato l'idScheda nella query string, filtriamo!
            if (idScheda.HasValue)
            {
                query = query.Where(a => a.IdScheda == idScheda.Value);
            }

            // Estraiamo i dati usando un tipo anonimo per evitare errori sul private set
            var risultato = await query
                .Select(a => new
                {
                    IdAllegato = a.IdAllegato,
                    IdScheda = a.IdScheda,
                    Nome = a.Nome,
                    Estensione = a.Estensione,
                    Url = a.Url,
                    Documento = a.Documento
                })
                .ToListAsync();

            return Ok(risultato);
        }



        // GET: api/Allegato/5/2
        [HttpGet("{id} , {idScheda}")]
        public async Task<IActionResult> GetAllegato(int id, int idScheda)
        {
            // Cerchiamo l'allegato che corrisponde SIA all'id dell'allegato SIA all'id della scheda
            var allegato = await _context.Allegati
                .Where(a => a.IdAllegato == id && a.IdScheda == idScheda)
                .Select(a => new
                {
                    IdAllegato = a.IdAllegato,
                    IdScheda = a.IdScheda,
                    Nome = a.Nome,
                    Estensione = a.Estensione,
                    Url = a.Url,
                    Documento = a.Documento
                })
                .FirstOrDefaultAsync();

            if (allegato == null)
            {
                // Ritorna 404 se l'allegato non esiste OPPURE se esiste ma appartiene a un'altra scheda
                return NotFound("Allegato non trovato o non associato a questa scheda.");
            }

            return Ok(allegato);
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