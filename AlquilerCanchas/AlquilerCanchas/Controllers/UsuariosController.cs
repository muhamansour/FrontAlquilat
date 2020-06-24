﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AlquilerCanchas.Database;
using AlquilerCanchas.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AlquilerCanchas.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly AlquilerCanchasDbContext _context;

        public UsuariosController(AlquilerCanchasDbContext context)
        {
            _context = context;
        }

        public IActionResult Login(string returnUrl)
        {
            TempData["returnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([Bind("Id,Email,Contrasenia")] string email, string password)
        {
            string returnUrl = TempData["returnUrl"] as string;
            Usuario usuario = _context.Usuario.FirstOrDefault(usr => usr.Email == email);

            if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
            {
                byte[] data = System.Text.Encoding.ASCII.GetBytes(password);
                data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);

                if (usuario.Contrasenia.SequenceEqual(data))
                {
                    ClaimsIdentity identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                    identity.AddClaim(new Claim(ClaimTypes.Name, email));
                    ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    usuario.FechaUltimoAcceso = DateTime.Now;
                    await _context.SaveChangesAsync();

                    if (!string.IsNullOrWhiteSpace(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Create", "Reservas");
                }
            }

            ViewBag.Error = "Usuario o contraseña incorrectos";
            ViewBag.Email = email;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        // GET: Usuarios/SignUp
        public IActionResult SignUp()
        {
            return View();
        }

        // POST: Usuarios/SignUp
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp([Bind("Id,Username,Contrasenia,Email,Dni,FechaDeNacimineto,Telefono")] Usuario usuario, string password)
        {
            if (!string.IsNullOrWhiteSpace(password))
            {
                byte[] data = System.Text.Encoding.ASCII.GetBytes(password);
                data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);

                usuario.Contrasenia = data;

                if (ModelState.IsValid)
                {
                    _context.Add(usuario);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Create", "Reservas");
                }
            }
            else
            {
                ModelState.AddModelError("Contrasenia", "La contraseña no puede estar vacía");
            }

            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return NotFound();
            }

            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Username,Contrasenia,Email,Dni,FechaDeNacimineto,Telefono")] Usuario usuario, string password)
        {
            if (id != usuario.Id)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                byte[] data = System.Text.Encoding.ASCII.GetBytes(password);
                data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);

                usuario.Contrasenia = data;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Home");
            }
            return View(usuario);
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuario.Any(e => e.Id == id);
        }
    }
}