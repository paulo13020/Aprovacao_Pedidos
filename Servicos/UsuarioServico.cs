using System.Text;
using Aprovacao_Pedidos.Enumeraveis;
using Aprovacao_Pedidos.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Aprovacao_Pedidos.Servicos
{
    public class UsuarioServico : Controller
    {
        public async Task<Usuario> FazerLoginAsync(LoginUsuario loginUsuario)
        {
            Usuario usuario = null;

            string url = "https://api.dovale.com.br/LoginUsuario1";

            string json = JsonConvert.SerializeObject(loginUsuario);

            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.PostAsync(url, content);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // Parse the response JSON
                    JObject responseObject = JObject.Parse(responseBody);

                    // Access the "informacoesUsuario" array and get the first element
                    var infoUsuario = responseObject["informacoesUsuario"].First();

                    if (infoUsuario.HasValues)
                    {
                        usuario = new Usuario
                        {
                            Nome = infoUsuario["containername"].ToString(),
                            Email = infoUsuario["emailaddress"].ToString(),
                            NivelUsuario = (TipoDePermissaoDeUsuario)Enum.Parse(typeof(TipoDePermissaoDeUsuario), infoUsuario["nivelusuario"].ToString())
                        };                        
                    }
                }

                return usuario;
            }
        }
    }
}
