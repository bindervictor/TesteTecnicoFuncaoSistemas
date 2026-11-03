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

            cliente.CPF = CpfUtils.NormalizarCpf(cliente.CPF);

            if (!CpfUtils.ValidarCPF(cliente.CPF))
                throw new Exception("O CPF informado não é válido. Por favor, digite novamente");

            if (VerificarExistencia(cliente.CPF))
                throw new Exception("Já existe um cliente cadastrado com esse CPF!");

            BoBeneficiario boBeneficiario = new BoBeneficiario();
            boBeneficiario.ValidarLista(cliente.Beneficiarios);

            long idCliente = cli.Incluir(cliente);

            boBeneficiario.SincronizarBeneficiariosDoCliente(idCliente, cliente.Beneficiarios, null);

            return idCliente;
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

            cliente.CPF = CpfUtils.NormalizarCpf(cliente.CPF);

            if (!CpfUtils.ValidarCPF(cliente.CPF))
                throw new Exception("O CPF informado não é válido. Por favor, digite novamente");

            if (cli.VerificarExistencia(cliente.CPF, cliente.Id))
                throw new Exception("Já existe um cliente cadastrado com esse CPF!");

            BoBeneficiario boBeneficiario = new BoBeneficiario();
            boBeneficiario.ValidarLista(cliente.Beneficiarios);

            cli.Alterar(cliente);

            boBeneficiario.SincronizarBeneficiariosDoCliente(cliente.Id, cliente.Beneficiarios, cliente.BeneficiariosExcluidos);
        }

        /// <summary>
        /// Consulta o cliente pelo id
        /// </summary>
        /// <param name="id">id do cliente</param>
        /// <returns></returns>
        public DML.Cliente Consultar(long id)
        {
            DAL.DaoCliente cli = new DAL.DaoCliente();
            DML.Cliente cliente = cli.Consultar(id);

            if (cliente != null)
            {
                BoBeneficiario boBeneficiario = new BoBeneficiario();
                cliente.Beneficiarios = boBeneficiario.ListarPorCliente(cliente.Id);
            }

            return cliente;
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
    }
}
