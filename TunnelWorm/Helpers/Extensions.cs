namespace Termit.Helpers
{
    using System;
    using System.Text;
    using System.Security;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text.RegularExpressions;

    using Caliburn.Micro;

    public static class Extensions
    {
        #region System.String

        /// <summary>
        /// Extension method for validate <see cref="System.String"/> value to port fomart 
        /// </summary>
        /// <param name="value">Validated value</param>
        /// <returns>Result of validation</returns>
        public static bool IsPort(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            var numeric = new Regex(@"^[0-9]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            if (numeric.IsMatch(value))
            {
                if (int.TryParse(value, out int i))
                    if (i < 65536)
                        return true;
            }

            return false;
        }

        /// <summary>
        /// Unprotecting <see cref="System.String"/> instance to use in program
        /// </summary>
        /// <param name="value">Protected <see cref="System.String"/> instance</param>
        /// <param name="optionalEntropy">Entropy</param>
        /// <param name="scope">Scope</param>
        /// <returns>Unprotected <see cref="System.String"/> instance</returns>
        public static SecureString Unprotect(this string value, string optionalEntropy = null, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var encryptedBytes = Convert.FromBase64String(value);
            var entropyBytes = string.IsNullOrEmpty(optionalEntropy) ? null : Encoding.UTF8.GetBytes(optionalEntropy);
            var clearBytes = ProtectedData.Unprotect(encryptedBytes, entropyBytes, scope);

            var temporaryPassword = Encoding.UTF8.GetString(clearBytes);
            var temporarySecureString = new SecureString();

            foreach (char t in temporaryPassword)
                temporarySecureString.AppendChar(t);

            return temporarySecureString;
        }

        #endregion

        #region SecureString

        /// <summary>
        /// Get <see cref="System.String"/> from <see cref="SecureString"/>
        /// </summary>
        /// <param name="source"><see cref="SecureString"/> instance</param>
        /// <returns><see cref="System.String"/> value</returns>
        public static string GetString(this SecureString source)
        {
            string result = null;

            var length = source.Length;
            var pointer = IntPtr.Zero;
            var chars = new char[length];

            try
            {
                pointer = Marshal.SecureStringToBSTR(source);
                Marshal.Copy(pointer, chars, 0, length);

                result = string.Join("", chars);
            }
            finally
            {
                if (pointer != IntPtr.Zero)
                    Marshal.ZeroFreeBSTR(pointer);
            }

            return result;
        }

        /// <summary>
        /// Protection <see cref="SecureString"/> instance to use in settings file
        /// </summary>
        /// <param name="value"><see cref="SecureString"/> instance</param>
        /// <param name="optionalEntropy">Entropy</param>
        /// <param name="scope">Scope</param>
        /// <returns>Protected string</returns>
        public static string Protect(this SecureString value, string optionalEntropy = null, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        {
            var stringPassword = value.GetString();

            if (stringPassword == null)
                throw new ArgumentNullException(nameof(stringPassword));

            var clearBytes = Encoding.UTF8.GetBytes(stringPassword);
            var entropyBytes = string.IsNullOrEmpty(optionalEntropy) ? null : Encoding.UTF8.GetBytes(optionalEntropy);
            var encryptedBytes = ProtectedData.Protect(clearBytes, entropyBytes, scope);

            return Convert.ToBase64String(encryptedBytes);
        }

        #endregion

        #region IEnumerable

        /// <summary>
        /// Converting <see cref="IEnumerable{T}"/> to <see cref="BindableCollection{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"><see cref="IEnumerable{T}"/> instance</param>
        /// <returns><see cref="BindableCollection{T}"/> instance</returns>
        public static BindableCollection<T> ToBindableCollection<T>(this IEnumerable<T> source)
        {
            return new BindableCollection<T>(source);
        }

        #endregion
    }
}
