using System.Linq;

namespace FI.AtividadeEntrevista.BLL
{
    public static class CpfUtils
    {
        public static string NormalizarCpf(string CPF)
        {
            if (string.IsNullOrWhiteSpace(CPF))
                return string.Empty;

            return new string(CPF.Where(c => c >= '0' && c <= '9').ToArray());
        }

        public static bool ValidarCPF(string CPF)
        {
            CPF = NormalizarCpf(CPF);

            // Valida tamanho e rejeita CPFs com todos os dígitos iguais (000..., 111..., etc.)
            if (CPF.Length != 11 || CPF.Distinct().Count() == 1)
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