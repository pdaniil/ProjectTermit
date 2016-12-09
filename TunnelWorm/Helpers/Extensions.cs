using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Caliburn.Micro;

namespace TunnelWorm.Helpers
{
    using System.Text.RegularExpressions;

    public static class Extensions
    {
        /// <summary>
        /// Extension method for validate <see cref="System.String"/> value to port fomart 
        /// </summary>
        /// <param name="value">Validated value</param>
        /// <returns>Result of validation</returns>
        public static bool IsPort(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            Regex numeric = new Regex(@"^[0-9]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            if (numeric.IsMatch(value))
            {
                try
                {
                    if (Convert.ToInt32(value) < 65536)
                        return true;
                }
                catch (OverflowException)
                {
                    throw new FormatException("Incorrect port number: " + value);
                }
            }
            return false;
        }

        public static string GetString(this SecureString source)
        {
            string result = null;
            int length = source.Length;
            IntPtr pointer = IntPtr.Zero;
            char[] chars = new char[length];

            try
            {
                pointer = Marshal.SecureStringToBSTR(source);
                Marshal.Copy(pointer, chars, 0, length);

                result = string.Join("", chars);
            }
            finally
            {
                if (pointer != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(pointer);
                }
            }

            return result;
        }

        public static BindableCollection<T> ToBindableCollection<T>(this IEnumerable<T> source) 
        {
            return new BindableCollection<T>(source);
        }

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
    }

}
