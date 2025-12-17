using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using RESTfulAPIPWeb.Data;
using RESTfulAPIPWeb.Entities;
using RESTfulAPIPWeb.Repositories;

namespace RESTfulAPIPWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class produtosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IProdutoRepository _produtoRepository;

        public produtosController(ApplicationDbContext context, IProdutoRepository produtoRepository)
        {
            _context = context;
            _produtoRepository = produtoRepository;
        }

        // GET: api/produtos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<produto>>> GetProdutos()
        {
            return Ok(await _produtoRepository.GetProdutosAsync());
        }

        // GET: api/produtos
        [HttpGet("byCategory")]
        public async Task<ActionResult<IEnumerable<produto>>> GetProdutosByCategory(int id)
        {
            return Ok(await _produtoRepository.GetProdutosPorCategoriaAsync(id));
        }

        // GET: api/produtos
        [HttpGet("Promos")]
        public async Task<ActionResult<IEnumerable<produto>>> GetProdutosPromocao()
        {
            return Ok(await _produtoRepository.GetProdutosPromocaoAsync());
        }

        // GET: api/produtos
        [HttpGet("bestSellers")]
        public async Task<ActionResult<IEnumerable<produto>>> GetProdutosMaisVendidos()
        {
            return Ok(await _produtoRepository.GetProdutosMaisVendidosAsync());
        }

        //// GET: api/produtos
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<produto>>> GetProdutos()
        //{
        //    return await _context.Produtos.ToListAsync();
        //}

        // GET: api/produtos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<produto>> Getproduto(int id)
        {

            return Ok(await _produtoRepository.GetProdutoDetalhesAsync(id));
        }

        //// GET: api/produtos/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<produto>> Getproduto(int id)
        //{
        //    var produto = await _context.Produtos.FindAsync(id);

        //    if (produto == null)
        //    {
        //        return NotFound();
        //    }

        //    return produto;
        //}

        // PUT: api/produtos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Putproduto(int id, [FromBody] produto produto)
        {
            if (id != produto.Id)
            {
                return BadRequest();
            }

            try
            {
                // CORREÇÃO: Capturar o resultado booleano
                var sucesso = await _produtoRepository.UpdateProdutosAsync(id, produto);

                if (!sucesso)
                {
                    return NotFound();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                // Agora usamos o repositório para verificar
                if (!await _produtoRepository.ProdutoExisteAsync(id))
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

        // POST: api/produtos
        [HttpPost]
        public async Task<ActionResult<produto>> Postproduto([FromBody] produto produto)
        {
            var result = await _produtoRepository.AdicionarProdutosAsync(produto);

            // Certifique-se que "Getproduto" corresponde ao nome exato do método GET por ID
            return CreatedAtAction(nameof(Getproduto), new { id = produto.Id }, produto);
        }

        // DELETE: api/produtos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deleteproduto(int id)
        {
            var result = await _produtoRepository.DeleteProdutosAsync(id);

            if (!result)
                return NotFound();

            return NoContent();
        }




        //// PUT: api/produtos/5
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> Putproduto(int id, produto produto)
        //{
        //    if (id != produto.Id)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(produto).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!produtoExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        //// POST: api/produtos
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost]
        //public async Task<ActionResult<produto>> Postproduto(produto produto)
        //{
        //    _context.Produtos.Add(produto);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("Getproduto", new { id = produto.Id }, produto);
        //}

        //// DELETE: api/produtos/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Deleteproduto(int id)
        //{
        //    var produto = await _context.Produtos.FindAsync(id);
        //    if (produto == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Produtos.Remove(produto);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        //private bool produtoExists(int id)
        //{
        //    return _context.Produtos.Any(e => e.Id == id);
        //}
    }
}
