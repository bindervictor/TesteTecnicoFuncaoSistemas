using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FI.AtividadeEntrevista.BLL
{
    public class BoCliente
    {
        /// <summary>
        /// Inclui um novo cliente
        /// </summary>
        /// <param name="cliente">Objeto de cliente</param>
        public long Incluir(DML.Cliente cliente)
        {
            DAL.DaoCliente cli = new DAL.DaoCliente();

            cliente.CPF = NormalizarCpf(cliente.CPF);

            if (!ValidarCPF(cliente.CPF))
                throw new Exception("O CPF informado não é válido. Por favor, digite novamente");

            if (VerificarExistencia(cliente.CPF))
                throw new Exception("Já existe um cliente cadastrado com esse CPF!");

            return cli.Incluir(cliente);
        }

        /// <summary>
        /// Altera um cliente
        /// </summary>
        /// <param name="cliente">Objeto de cliente</param>
        public void Alterar(DML.Cliente cliente)
        {
            DAL.DaoCliente cli = new DAL.DaoCliente();

            if (cliente.Id <= 0)
                throw new Exception("Cliente inválido para alteração.");

            cliente.CPF = NormalizarCpf(cliente.CPF);

            if (!ValidarCPF(cliente.CPF))
                throw new Exception("O CPF informado não é válido. Por favor, digite novamente");

            if (cli.VerificarExistencia(cliente.CPF, cliente.Id))
                throw new Exception("Já existe um cliente cadastrado com esse CPF!");
            
            cli.Alterar(cliente);
        }

        /// <summary>
        /// Consulta o cliente pelo id
        /// </summary>
        /// <param name="id">id do cliente</param>
        /// <returns></returns>
        public DML.Cliente Consultar(long id)
        {
            DAL.DaoCliente cli = new DAL.DaoCliente();
            return cli.Consultar(id);
        }

        /// <summary>
        /// Excluir o cliente pelo id
        /// </summary>
        /// <param name="id">id do cliente</param>
        /// <returns></returns>
        public void Excluir(long id)
        {
            DAL.DaoCliente cli = new DAL.DaoCliente();
            cli.Excluir(id);
        }

        /// <summary>
        /// Lista os clientes
        /// </summary>
        public List<DML.Cliente> Listar()
        {
            DAL.DaoCliente cli = new DAL.DaoCliente();
            return cli.Listar();
        }

        /// <summary>
        /// Lista os clientes
        /// </summary>
        public List<DML.Cliente> Pesquisa(int iniciarEm, int quantidade, string campoOrdenacao, bool crescente, out int qtd)
        {
            DAL.DaoCliente cli = new DAL.DaoCliente();
            return cli.Pesquisa(iniciarEm,  quantidade, campoOrdenacao, crescente, out qtd);
        }

        /// <summary>
        /// VerificaExistencia
        /// </summary>
        /// <param name="CPF"></param>
        /// <returns></returns>
        public bool VerificarExistencia(string CPF, long Id = 0)
        {
            DAL.DaoCliente cli = new DAL.DaoCliente();

            return cli.VerificarExistencia(CPF, Id);
        }

        private static string NormalizarCpf(string CPF)
        {
            if (string.IsNullOrWhiteSpace(CPF))
                return string.Empty;

            return new string(CPF.Where(c => c >= '0' && c <= '9').ToArray());
        }

        public bool ValidarCPF(string CPF)
        {
            CPF = NormalizarCpf(CPF);

            if (CPF.Length != 11)
                return false;

            // Rejeita CPFs com todos os dígitos iguais (000..., 111..., etc.)
            if (CPF.Distinct().Count() == 1)
                return false;

            int soma = 0;
            int resto;

            // 1º dígito verificador
            for (int i = 0; i < 9; i++)
                soma += (CPF[i] - '0') * (10 - i);

            resto = soma % 11;
            int digitoVerificador1 = (resto < 2) ? 0 : 11 - resto;

            // 2º dígito verificador
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += (CPF[i] - '0') * (11 - i);

            resto = soma % 11;
            int digitoVerificador2 = (resto < 2) ? 0 : 11 - resto;

            return CPF[9] - '0' == digitoVerificador1 && CPF[10] - '0' == digitoVerificador2;
        }
    }
}
