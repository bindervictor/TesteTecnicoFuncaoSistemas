using System;
using System.Collections.Generic;
using System.Linq;

namespace FI.AtividadeEntrevista.BLL
{
    public class BoBeneficiario
    {
        public List<DML.Beneficiario> ListarPorCliente(long idCliente)
        {
            DAL.DaoBeneficiario dao = new DAL.DaoBeneficiario();
            return dao.ListarPorCliente(idCliente);
        }

        /// <summary>
        /// Aplica as alterações de beneficiários de um cliente.
        /// Só exclui os beneficiários cujos Ids vierem explicitamente em <paramref name="idsExcluidos"/>;
        /// um beneficiário que simplesmente não veio na lista NUNCA é apagado.
        /// Deve ser chamado dentro da transação do cliente (BoCliente).
        /// </summary>
        public void SincronizarBeneficiariosDoCliente(long idCliente, List<DML.Beneficiario> beneficiarios, List<long> idsExcluidos)
        {
            DAL.DaoBeneficiario dao = new DAL.DaoBeneficiario();

            if (idCliente <= 0)
                throw new Exception("Cliente inválido para manutenção de beneficiários.");

            List<DML.Beneficiario> recebidos = beneficiarios ?? new List<DML.Beneficiario>();

            ValidarLista(recebidos);

            foreach (DML.Beneficiario item in recebidos)
                item.IdCliente = idCliente;

            List<DML.Beneficiario> atuais = dao.ListarPorCliente(idCliente);
            Dictionary<long, string> cpfAtualPorId = atuais.ToDictionary(b => b.Id, b => b.CPF);

            HashSet<long> excluir = new HashSet<long>((idsExcluidos ?? new List<long>()).Where(cpfAtualPorId.ContainsKey));

            if (recebidos.Any(b => b.Id > 0 && !cpfAtualPorId.ContainsKey(b.Id)))
                throw new Exception("Um dos beneficiários informados não existe mais para este cliente. Recarregue a página e tente novamente.");

            if (recebidos.Any(b => b.Id > 0 && excluir.Contains(b.Id)))
                throw new Exception("Beneficiário informado para alteração e exclusão ao mesmo tempo. Recarregue a página e tente novamente.");

            HashSet<long> idsAlterados = new HashSet<long>(recebidos.Where(b => b.Id > 0).Select(b => b.Id));
            IEnumerable<string> cpfsFinais = atuais
                .Where(b => !excluir.Contains(b.Id) && !idsAlterados.Contains(b.Id))
                .Select(b => b.CPF)
                .Concat(recebidos.Select(b => b.CPF));

            if (cpfsFinais.GroupBy(c => c).Any(g => g.Count() > 1))
                throw new Exception("Já existe um beneficiário cadastrado com esse CPF para este cliente!");

            foreach (long id in excluir)
                dao.Excluir(id);

            List<DML.Beneficiario> alterados = recebidos.Where(b => b.Id > 0).ToList();

            foreach (DML.Beneficiario item in alterados.Where(b => cpfAtualPorId[b.Id] != b.CPF))
            {
                dao.Alterar(new DML.Beneficiario
                {
                    Id = item.Id,
                    Nome = item.Nome,
                    CPF = "T" + item.Id,
                    IdCliente = idCliente
                });
            }

            foreach (DML.Beneficiario item in alterados)
                dao.Alterar(item);

            foreach (DML.Beneficiario item in recebidos.Where(b => b.Id <= 0))
                dao.Incluir(item);
        }

        /// <summary>
        /// Valida (e normaliza o CPF de) todos os beneficiários e garante que não há CPF repetido na lista.
        /// </summary>
        public void ValidarLista(List<DML.Beneficiario> beneficiarios)
        {
            if (beneficiarios == null)
                return;

            foreach (DML.Beneficiario item in beneficiarios)
                ValidarDados(item);

            if (beneficiarios.GroupBy(b => b.CPF).Any(g => g.Count() > 1))
                throw new Exception("Não é permitido informar beneficiários com o mesmo CPF para o mesmo cliente.");
        }

        public void ValidarDados(DML.Beneficiario beneficiario)
        {
            if (beneficiario == null)
                throw new Exception("Beneficiário inválido.");

            beneficiario.CPF = CpfUtils.NormalizarCpf(beneficiario.CPF);

            if (!CpfUtils.ValidarCPF(beneficiario.CPF))
                throw new Exception("O CPF do beneficiário informado não é válido. Por favor, digite novamente");

            if (string.IsNullOrWhiteSpace(beneficiario.Nome))
                throw new Exception("O nome do beneficiário é obrigatório.");
        }
    }
}
