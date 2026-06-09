namespace biblioteca_api.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int Idade { get; set; }
        public string Telefone { get; set; } = string.Empty;
    }
}
