using System.Net;
using System.Text.RegularExpressions;

namespace ChatServer.Services
{
    public class EmailValidator
    {
        // Limites recomendados pela RFC 5321 (máximo de 254 caracteres para o endereço completo)
        private const int MaxEmailLength = 254;
        // Limite para a parte local (antes do @): 64 caracteres (RFC 5321)
        private const int MaxLocalPartLength = 64;
        // Tempo limite para consultas DNS (em milissegundos)
        private const int DnsTimeoutMs = 3000;

        // Regex simples e segura (evita backtracking excessivo) para validar a parte local.
        // Permite caracteres comuns, incluindo ponto, hífen, sublinhado, +, etc.
        // Restrições adicionais são aplicadas fora da regex para evitar ReDoS.
        private static readonly Regex LocalPartRegex = new Regex(
            @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+$",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture,
            TimeSpan.FromMilliseconds(100) // timeout para evitar travamentos
        );

        // Regex para validar o domínio (nome de host): letras, números, hífen e pontos.
        // Também evita backtracking problemático.
        private static readonly Regex DomainRegex = new Regex(
            @"^[a-zA-Z0-9]([a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture,
            TimeSpan.FromMilliseconds(100)
        );

        /// <summary>
        /// Valida um endereço de e-mail quanto ao formato, comprimento e (opcionalmente) registros MX.
        /// </summary>
        /// <param name="email">Endereço de e-mail a ser validado.</param>
        /// <param name="checkMx">Se true, verifica se o domínio possui registros MX.</param>
        /// <returns>True se o e-mail é válido; caso contrário, false.</returns>
        public static bool IsValidEmail(string email, bool checkMx = false)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // 1. Normalização: trim e conversão para lowercase (opcional, mas recomendado)
            email = email.Trim().ToLowerInvariant();

            // 2. Verificar comprimento máximo total
            if (email.Length > MaxEmailLength)
                return false;

            // 3. Dividir em parte local e domínio
            int atIndex = email.LastIndexOf('@');
            if (atIndex <= 0 || atIndex == email.Length - 1)
                return false;

            string localPart = email.Substring(0, atIndex);
            string domain = email.Substring(atIndex + 1);

            // 4. Validar comprimento da parte local
            if (localPart.Length > MaxLocalPartLength)
                return false;

            // 5. Validar parte local com regex (evita caracteres inválidos)
            if (!LocalPartRegex.IsMatch(localPart))
                return false;

            // 6. Restrições adicionais na parte local (evitar pontos consecutivos, começar/terminar com ponto)
            if (localPart.StartsWith(".") || localPart.EndsWith(".") || localPart.Contains(".."))
                return false;

            // 7. Validar domínio (formato básico)
            if (!DomainRegex.IsMatch(domain))
                return false;

            // 8. Verificar se o domínio tem pelo menos um ponto e um TLD com letras
            int lastDot = domain.LastIndexOf('.');
            if (lastDot <= 0 || lastDot == domain.Length - 1)
                return false;

            string tld = domain.Substring(lastDot + 1);
            if (!IsValidTld(tld)) // validação básica do TLD
                return false;

            // 9. (Opcional) Verificação de registros MX via DNS
            if (checkMx && !HasMxRecord(domain))
                return false;

            return true;
        }

        /// <summary>
        /// Versão assíncrona da validação com verificação de MX (evita bloquear a thread).
        /// </summary>
        public static async Task<bool> IsValidEmailAsync(string email, bool checkMx = false)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // Mesmas validações síncronas iniciais (sem MX)
            if (!IsValidEmail(email, checkMx: false))
                return false;

            if (checkMx)
            {
                int atIndex = email.LastIndexOf('@');
                string domain = email.Substring(atIndex + 1);
                return await HasMxRecordAsync(domain).ConfigureAwait(false);
            }

            return true;
        }

        // Validação simples de TLD: deve ter apenas letras e pelo menos 2 caracteres.
        // Para maior rigor, pode-se usar uma lista atualizada de TLDs (ex: via IANA).
        private static bool IsValidTld(string tld)
        {
            if (string.IsNullOrEmpty(tld) || tld.Length < 2)
                return false;
            // Verifica se contém apenas letras (evita caracteres especiais no TLD)
            foreach (char c in tld)
            {
                if (!char.IsLetter(c))
                    return false;
            }
            return true;
        }

        // Verificação síncrona de MX (com timeout)
        private static bool HasMxRecord(string domain)
        {
            try
            {
                var task = Task.Run(() => HasMxRecordAsync(domain));
                if (task.Wait(DnsTimeoutMs))
                    return task.Result;
                return false; // timeout
            }
            catch
            {
                return false; // qualquer exceção (DNS, rede, etc.) trata como inválido
            }
        }

        // Verificação assíncrona de MX (recomendada para aplicações reais)
        private static async Task<bool> HasMxRecordAsync(string domain)
        {
            try
            {
                var mxRecords = await Dns.GetHostAddressesAsync(domain);
                // Nota: Dns.GetHostAddressesAsync não retorna registros MX diretamente.
                // Para verificar MX, é necessário usar uma biblioteca como DnsClient.
                // Aqui, simulamos uma verificação: se o domínio resolve para algum IP, 
                // assumimos que existe um registro MX (simplificação).
                // Em produção, use uma biblioteca específica (ex: ARSoft.Tools.Net.Dns).
                return mxRecords.Length > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
