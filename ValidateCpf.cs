using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace ValidateCpfFunction
{
    public static class ValidateCpf
    {
        [FunctionName("ValidateCpf")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Processing CPF validation request.");

            string cpf = req.Query["cpf"];

            if (string.IsNullOrEmpty(cpf))
            {
                return new BadRequestObjectResult("Please provide a CPF as a query parameter, e.g., ?cpf=12345678909");
            }

            cpf = cpf.Replace(".", "").Replace("-", "").Trim();

            if (!Regex.IsMatch(cpf, "^\\d{11}$"))
            {
                return new BadRequestObjectResult("Invalid CPF format. CPF must contain 11 numeric digits.");
            }

            if (IsValidCpf(cpf))
            {
                return new OkObjectResult(new { Valid = true, Message = "CPF is valid." });
            }
            else
            {
                return new OkObjectResult(new { Valid = false, Message = "CPF is invalid." });
            }
        }

        private static bool IsValidCpf(string cpf)
        {
            // Verifica se todos os dígitos são iguais, o que é inválido
            if (cpf.Distinct().Count() == 1)
                return false;

            // Calcula o primeiro dígito verificador
            int[] multiplicador1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(cpf[i].ToString()) * multiplicador1[i];

            int resto = soma % 11;
            int digito1 = resto < 2 ? 0 : 11 - resto;

            // Calcula o segundo dígito verificador
            int[] multiplicador2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            soma = 0;

            for (int i = 0; i < 10; i++)
                soma += int.Parse(cpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            int digito2 = resto < 2 ? 0 : 11 - resto;

            // Verifica se os dígitos calculados são iguais aos informados
            return cpf.EndsWith(digito1.ToString() + digito2.ToString());
        }
    }
}
