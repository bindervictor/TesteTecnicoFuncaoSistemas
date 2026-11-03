namespace FI.AtividadeEntrevista.DML
{
    /// <summary>
    /// Classe de beneficiário que representa o registro na tabela BENEFICIARIOS
    /// </summary>
    public class Beneficiario
    {
        /// <summary>
        /// Id do beneficiário
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// CPF do beneficiário
        /// </summary>
        public string CPF { get; set; }

        /// <summary>
        /// Nome do beneficiário
        /// </summary>
        public string Nome { get; set; }

        /// <summary>
        /// Id do cliente dono do beneficiário
        /// </summary>
        public long IdCliente { get; set; }
    }
}