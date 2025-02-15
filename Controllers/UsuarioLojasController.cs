using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FacilComercio.Models;
using WebApplication1.Data; // Ajuste para o namespace correto


namespace WebApplication1.Controllers
{
    public class UsuarioLojasController : Controller
    {
        private readonly AppDbContext _context;

        public UsuarioLojasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: UsuarioLojas
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.UsuarioLojas.Include(u => u.Loja).Include(u => u.Usuario);
            return View(await appDbContext.ToListAsync());
        }

        // GET: UsuarioLojas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuarioLoja = await _context.UsuarioLojas
                .Include(u => u.Loja)
                .Include(u => u.Usuario)
                .FirstOrDefaultAsync(m => m.Usu_Id == id);



            if (usuarioLoja == null)
            {
                return NotFound();
            }

            return View(usuarioLoja);
        }

        // GET: UsuarioLojas/Create
        public IActionResult Create()
        {
            ViewData["Loj_Id"] = new SelectList(_context.Lojas, "Id", "Nome");
            ViewData["Usu_Id"] = new SelectList(_context.Usuarios, "Id", "Email");
            return View();
        }

        // POST: UsuarioLojas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Usu_Id,Loj_Id,Permissao")] UsuarioLoja usuarioLoja)
        {
            if (ModelState.IsValid)
            {
                _context.Add(usuarioLoja);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Loj_Id"] = new SelectList(_context.Lojas, "Id", "Nome", usuarioLoja.Loj_Id);
            ViewData["Usu_Id"] = new SelectList(_context.Usuarios, "Id", "Email", usuarioLoja.Usu_Id);
            return View(usuarioLoja);
        }

        // GET: UsuarioLojas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuarioLoja = await _context.UsuarioLojas.FindAsync(id);
            if (usuarioLoja == null)
            {
                return NotFound();
            }
            ViewData["Loj_Id"] = new SelectList(_context.Lojas, "Id", "Nome", usuarioLoja.Loj_Id);
            ViewData["Usu_Id"] = new SelectList(_context.Usuarios, "Id", "Email", usuarioLoja.Usu_Id);
            return View(usuarioLoja);
        }

        // POST: UsuarioLojas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Usu_Id,Loja_Id,Permissao")] UsuarioLoja usuarioLoja)
        {
            if (id != usuarioLoja.Usu_Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuarioLoja);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioLojaExists(usuarioLoja.Usu_Id))
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
            ViewData["Loj_Id"] = new SelectList(_context.Lojas, "Id", "Nome", usuarioLoja.Loj_Id);
            ViewData["Usu_Id"] = new SelectList(_context.Usuarios, "Id", "Email", usuarioLoja.Usu_Id);
            return View(usuarioLoja);
        }

        // GET: UsuarioLojas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuarioLoja = await _context.UsuarioLojas
                .Include(u => u.Loja)
                .Include(u => u.Usuario)
                .FirstOrDefaultAsync(m => m.Usu_Id == id);
            if (usuarioLoja == null)
            {
                return NotFound();
            }

            return View(usuarioLoja);
        }

        // POST: UsuarioLojas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuarioLoja = await _context.UsuarioLojas.FindAsync(id);
            if (usuarioLoja != null)
            {
                _context.UsuarioLojas.Remove(usuarioLoja);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioLojaExists(int id)
        {
            return _context.UsuarioLojas.Any(e => e.Usu_Id == id);
        }
    }
}
