using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTfulAPIPWeb.Data;
using RESTfulAPIPWeb.Entities;
using RESTfulAPIPWeb.Repositories;

namespace RESTfulAPIPWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class categoriasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICategoriaRepository _categoriaRepository;

        public categoriasController(ApplicationDbContext context, ICategoriaRepository categoriaRepository)
        {
            _context = context;
            _categoriaRepository = categoriaRepository;
        }

        // GET: api/categorias
        [HttpGet("repository")]
        public async Task<ActionResult<IEnumerable<categoria>>> GetCategoriasRep()
        {
            // Use repository to get categorias
            var categorias = await _categoriaRepository.GetCategorias();
            return Ok(categorias);
        }

        // Alternative GET using EF Core directly: GET api/categorias/raw
        [HttpGet]
        public async Task<ActionResult<IEnumerable<categoria>>> GetCategorias()
        {
            return await _context.Categorias.ToListAsync();
        }

        // GET: api/categorias/5
        [HttpGet("{id}")]
        public async Task<ActionResult<categoria>> Getcategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
            {
                return NotFound();
            }

            return categoria;
        }

        // PUT: api/categorias/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> Putcategoria(int id, categoria categoria)
        {
            if (id != categoria.Id)
            {
                return BadRequest();
            }

            _context.Entry(categoria).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!categoriaExists(id))
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

        // POST: api/categorias
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<categoria>> Postcategoria(categoria categoria)
        {
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Getcategoria", new { id = categoria.Id }, categoria);
        }

        // DELETE: api/categorias/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletecategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool categoriaExists(int id)
        {
            return _context.Categorias.Any(e => e.Id == id);
        }
    }
}
