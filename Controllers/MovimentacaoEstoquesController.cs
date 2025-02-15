using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FacilComercio.Models;
using WebApplication1.Data; // Ajuste para o namespace correto
using Microsoft.Data.SqlClient;
using iTextSharp.text.pdf;
using iTextSharp.text;


namespace WebApplication1.Controllers
{
    public class MovimentacaoEstoquesController : Controller
    {
        private readonly AppDbContext _context;

        public MovimentacaoEstoquesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: MovimentacaoEstoques
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.MovimentacoesEstoque.Include(m => m.Loja).Include(m => m.Produto);
            return View(await appDbContext.ToListAsync());
        }

        // GET: MovimentacaoEstoques/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimentacaoEstoque = await _context.MovimentacoesEstoque
                .Include(m => m.Loja)
                .Include(m => m.Produto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movimentacaoEstoque == null)
            {
                return NotFound();
            }

            return View(movimentacaoEstoque);
        }

        // GET: MovimentacaoEstoques/Create
        public IActionResult Create()
        {
            ViewData["Id"] = new SelectList(_context.Lojas, "Id", "Nome");
            ViewData["Id"] = new SelectList(_context.Produtos, "Id", "Nome");
            return View();
        }

        // POST: MovimentacaoEstoques/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProdutoId,Loj_Id,Quantidade,TipoMov,DataMov")] MovimentacaoEstoque movimentacaoEstoque)
        {
            if (ModelState.IsValid)
            {
                _context.Add(movimentacaoEstoque);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Id"] = new SelectList(_context.Lojas, "Id", "Nome", movimentacaoEstoque.Id);
            ViewData["Id"] = new SelectList(_context.Produtos, "Id", "Nome", movimentacaoEstoque.Id);
            return View(movimentacaoEstoque);
        }

        // GET: MovimentacaoEstoques/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimentacaoEstoque = await _context.MovimentacoesEstoque.FindAsync(id);
            if (movimentacaoEstoque == null)
            {
                return NotFound();
            }
            ViewData["Id"] = new SelectList(_context.Lojas, "Id", "Nome", movimentacaoEstoque.Id);
            ViewData["Id"] = new SelectList(_context.Produtos, "Id", "Nome", movimentacaoEstoque.Id);
            return View(movimentacaoEstoque);
        }

        // POST: MovimentacaoEstoques/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProdutoId,Loj_Id,Quantidade,TipoMov,DataMov")] MovimentacaoEstoque movimentacaoEstoque)
        {
            if (id != movimentacaoEstoque.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movimentacaoEstoque);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovimentacaoEstoqueExists(movimentacaoEstoque.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Id"] = new SelectList(_context.Lojas, "Id", "Nome", movimentacaoEstoque.Id);
            ViewData["Id"] = new SelectList(_context.Produtos, "Id", "Nome", movimentacaoEstoque.Id);
            return View(movimentacaoEstoque);
        }

        // GET: MovimentacaoEstoques/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimentacaoEstoque = await _context.MovimentacoesEstoque
                .Include(m => m.Loja)
                .Include(m => m.Produto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movimentacaoEstoque == null)
            {
                return NotFound();
            }

            return View(movimentacaoEstoque);
        }

        // POST: MovimentacaoEstoques/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movimentacaoEstoque = await _context.MovimentacoesEstoque.FindAsync(id);
            if (movimentacaoEstoque != null)
            {
                _context.MovimentacoesEstoque.Remove(movimentacaoEstoque);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
     
        private bool MovimentacaoEstoqueExists(int id)
        {
            return _context.MovimentacoesEstoque.Any(e => e.Id == id);
        }


    }
}
