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
    public class LogSistemasController : Controller
    {
        private readonly AppDbContext _context;

        public LogSistemasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: LogSistemas
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.LogSistema.Include(l => l.Usuario);
            return View(await appDbContext.ToListAsync());
        }

        // GET: LogSistemas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var logSistema = await _context.LogSistema
                .Include(l => l.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (logSistema == null)
            {
                return NotFound();
            }

            return View(logSistema);
        }

        // GET: LogSistemas/Create
        public IActionResult Create()
        {
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Email");
            return View();
        }

        // POST: LogSistemas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DataHora,Usu_Id,Acao,Entidade,EntidadeId,Detalhes,IP,Resultado")] LogSistema logSistema)
        {
            if (ModelState.IsValid)
            {
                _context.Add(logSistema);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Usu_Id"] = new SelectList(_context.Usuarios, "Id", "Email", logSistema.Usu_Id);
            return View(logSistema);
        }

        // GET: LogSistemas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var logSistema = await _context.LogSistema.FindAsync(id);
            if (logSistema == null)
            {
                return NotFound();
            }
            ViewData["Usu_Id"] = new SelectList(_context.Usuarios, "Id", "Email", logSistema.Usu_Id);
            return View(logSistema);
        }

        // POST: LogSistemas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DataHora,Usu_Id,Acao,Entidade,EntidadeId,Detalhes,IP,Resultado")] LogSistema logSistema)
        {
            if (id != logSistema.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(logSistema);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LogSistemaExists(logSistema.Id))
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
            ViewData["Usu_Id"] = new SelectList(_context.Usuarios, "Id", "Email", logSistema.Usu_Id);
            return View(logSistema);
        }

        // GET: LogSistemas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var logSistema = await _context.LogSistema
                .Include(l => l.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (logSistema == null)
            {
                return NotFound();
            }

            return View(logSistema);
        }

        // POST: LogSistemas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var logSistema = await _context.LogSistema.FindAsync(id);
            if (logSistema != null)
            {
                _context.LogSistema.Remove(logSistema);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LogSistemaExists(int id)
        {
            return _context.LogSistema.Any(e => e.Id == id);
        }
    }
}
