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

        public long Incluir(DML.Beneficiario beneficiario)
        {
            DAL.DaoBeneficiario dao = new DAL.DaoBeneficiario();

            ValidarDados(beneficiario);

            if (beneficiario.IdCliente <= 0)
                throw new Exception("Cliente inválido para inclusão de beneficiário.");

            if (ExisteCpfNoCliente(beneficiario.IdCliente, beneficiario.CPF, 0))
                throw new Exception("Já existe um beneficiário cadastrado com esse CPF para este cliente!");

            return dao.Incluir(beneficiario);
        }

        public void Alterar(DML.Beneficiario beneficiario)
        {
            DAL.DaoBeneficiario dao = new DAL.DaoBeneficiario();

            if (beneficiario.Id <= 0)
                throw new Exception("Beneficiário inválido para alteração.");

            ValidarDados(beneficiario);

            if (beneficiario.IdCliente <= 0)
                throw new Exception("Cliente inválido para alteração de beneficiário.");

            if (ExisteCpfNoCliente(beneficiario.IdCliente, beneficiario.CPF, beneficiario.Id))
                throw new Exception("Já existe um beneficiário cadastrado com esse CPF para este cliente!");

            dao.Alterar(beneficiario);
        }

        public void Excluir(long idBeneficiario)
        {
            if (idBeneficiario <= 0)
                throw new Exception("Beneficiário inválido para exclusão.");

            DAL.DaoBeneficiario dao = new DAL.DaoBeneficiario();
            dao.Excluir(idBeneficiario);
        }

        /// <summary>
        /// Aplica as alterações de beneficiários de um cliente.
        /// Só exclui os beneficiários cujos Ids vierem explicitamente em <paramref name="idsExcluidos"/>;
        /// um beneficiário que simplesmente não veio na lista NUNCA é apagado.
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
            HashSet<long> idsAtuais = new HashSet<long>(atuais.Select(b => b.Id));

            // Só exclui Ids que realmente pertencem a este cliente
            HashSet<long> excluir = new HashSet<long>((idsExcluidos ?? new List<long>()).Where(idsAtuais.Contains));

            if (recebidos.Any(b => b.Id > 0 && !idsAtuais.Contains(b.Id)))
                throw new Exception("Um dos beneficiários informados não existe mais para este cliente. Recarregue a página e tente novamente.");

            if (recebidos.Any(b => b.Id > 0 && excluir.Contains(b.Id)))
                throw new Exception("Beneficiário informado para alteração e exclusão ao mesmo tempo. Recarregue a página e tente novamente.");

            // Estado final = atuais que permanecem sem alteração + recebidos (novos e alterados)
            HashSet<long> idsAlterados = new HashSet<long>(recebidos.Where(b => b.Id > 0).Select(b => b.Id));
            IEnumerable<string> cpfsFinais = atuais
                .Where(b => !excluir.Contains(b.Id) && !idsAlterados.Contains(b.Id))
                .Select(b => b.CPF)
                .Concat(recebidos.Select(b => b.CPF));

            if (cpfsFinais.GroupBy(c => c).Any(g => g.Count() > 1))
                throw new Exception("Já existe um beneficiário cadastrado com esse CPF para este cliente!");

            foreach (long id in excluir)
                dao.Excluir(id);

            foreach (DML.Beneficiario item in recebidos)
            {
                if (item.Id <= 0)
                    dao.Incluir(item);
                else
                    dao.Alterar(item);
            }
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

        private bool ExisteCpfNoCliente(long idCliente, string cpfNormalizado, long idIgnorar)
        {
            DAL.DaoBeneficiario dao = new DAL.DaoBeneficiario();

            List<DML.Beneficiario> lista = dao.ListarPorCliente(idCliente);

            return lista.Any(b =>
                b.CPF == cpfNormalizado &&
                (idIgnorar <= 0 || b.Id != idIgnorar));
        }
    }
}