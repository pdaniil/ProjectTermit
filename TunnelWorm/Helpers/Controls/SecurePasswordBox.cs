using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace TunnelWorm.Helpers.Controls
{
    public class SecurePasswordBox
    {
        private static bool _updating = false;
        private static SecureString secureStr = new SecureString();

        /// <summary>
        /// SecurePassword Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty SecurePasswordProperty =
            DependencyProperty.RegisterAttached("SecurePassword",
                typeof(SecureString),
                typeof(SecurePasswordBox),
                new FrameworkPropertyMetadata(new SecureString(), OnSecurePasswordChanged));

        /// <summary>
        /// Handles changes to the SecurePassword property.
        /// </summary>
        private static void OnSecurePasswordChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            PasswordBox password = d as PasswordBox;
            if (password != null)
            {
                // Disconnect the handler while we're updating.
                password.PasswordChanged -= PasswordChanged;
            }

            if (e.NewValue != null)
            {
                if (!_updating)
                {
                    secureStr = e.NewValue as SecureString;
                }
            }
            else
            {
                secureStr = new SecureString();
            }
            // Now, reconnect the handler.
            password.PasswordChanged += PasswordChanged;
        }

        /// <summary>
        /// Gets the SecurePassword property.
        /// </summary>
        public static SecureString GetSecurePassword(DependencyObject d)
        {
            return (SecureString)d.GetValue(SecurePasswordProperty);
        }

        /// <summary>
        /// Sets the SecurePassword property.
        /// </summary>
        public static void SetSecurePassword(DependencyObject d, SecureString value)
        {
            d.SetValue(SecurePasswordProperty, value);
        }
        
        /// <summary>
        /// Handles the password change event.
        /// </summary>
        static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox password = sender as PasswordBox;
            _updating = true;
            SetSecurePassword(password, password.SecurePassword);
            _updating = false;
        }
    }
}
