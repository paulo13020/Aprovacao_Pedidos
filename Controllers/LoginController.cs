using System.Security.Claims;
using Aprovacao_Pedidos.BancoDeDados;
using Aprovacao_Pedidos.Excecoes;
using Aprovacao_Pedidos.Models;
using Aprovacao_Pedidos.Servicos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aprovacao_Pedidos.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        #region Membros
        private readonly ContextoDeBancoDeDados _ctxBancoDeDados;
        #endregion

        #region Contrutores
        public LoginController(ContextoDeBancoDeDados ctxBancoDeDados)
        {
            _ctxBancoDeDados = ctxBancoDeDados;
        }
        #endregion

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginUsuario loginUsuario)
        {
            try
            {
                UsuarioServico srvUsuario = new UsuarioServico();

                if (string.IsNullOrWhiteSpace(loginUsuario.Usuario))
                {
                    //Informe o campo Usuário
                    TempData["Erro"] = "Informe o campo Usuário";
                    return View();
                }

                if (string.IsNullOrWhiteSpace(loginUsuario.Senha))
                {
                    //Informe o campo Senha
                    TempData["Erro"] = "Informe o campo Senha";
                    return View();
                }

                var usuarioLogado = await srvUsuario.FazerLoginAsync(loginUsuario);

                if (usuarioLogado is not null)
                {
                    var usuario = _ctxBancoDeDados.usuarios
                  .FirstOrDefault(entU => entU.Nome == loginUsuario.Usuario);

                    if (usuario == null)
                    {
                        usuario = new Usuario
                        {
                            Nome = loginUsuario.Usuario
                        };

                        _ctxBancoDeDados.usuarios.Add(usuario);
                        await _ctxBancoDeDados.SaveChangesAsync();
                    }

                    var claims = new List<Claim>
                     {
                        new Claim(ClaimTypes.Name, usuarioLogado.Nome),
                        new Claim(ClaimTypes.Role, usuarioLogado.NivelUsuario.ToString()),
                        new Claim("UsuarioID", usuario.ID.ToString()) // Adiciona o ID do usuário às claims
                     };

                    var claimsIdentity = new ClaimsIdentity(claims, "login");
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    return RedirectToAction("Index", "Pedidos");
                }

                TempData["Erro"] = "Usuário ou senha incorreto!";

                return View();
            }
            catch (ValorInvalidoExcecao ex)
            {

                TempData["Erro"] = ex.Message;
                return View();
            }
        }


        public async Task<IActionResult> FazerLogout()
        {
            await HttpContext.SignOutAsync();
            return View(nameof(Index));
        }
    }
}
