using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Aprovacao_Pedidos.Enumeraveis;

namespace Aprovacao_Pedidos.Models
{
    [Table("APROVACAO_TELEVENDAS_USUARIOS")]
    public class Usuario
    {
        [Key]
        public int ID { get; set; }

        [Column("NomeUsuario")]
        public string Nome { get; set; }

        [NotMapped]
        public string Email { get; set; }

        [NotMapped]
        public TipoDePermissaoDeUsuario NivelUsuario { get; set; }
    }

    public class LoginUsuario
    {
        public string Usuario { get; set; }
        public string Senha { get; set; }
    }

   }
