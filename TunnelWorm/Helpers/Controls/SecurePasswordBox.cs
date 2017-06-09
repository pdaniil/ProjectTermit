namespace Termit.Helpers.Controls
{
    using System.Windows;
    using System.Security;
    using System.Windows.Controls;

    public class SecurePasswordBox
    {
        #region Private Members

        private static bool _updating = false;
        private static SecureString _secureString = new SecureString();

        #endregion

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
        private static void OnSecurePasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var password = d as PasswordBox;

            if (password != null)
                password.PasswordChanged -= PasswordChanged;

            if (e.NewValue != null)
            {
                if (!_updating)
                    _secureString = e.NewValue as SecureString;
            }
            else
                _secureString = new SecureString();

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
        public static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox password = sender as PasswordBox;
            _updating = true;
            SetSecurePassword(password, password.SecurePassword);
            _updating = false;
        }
    }
}
