using Termit.Configuration;

namespace Termit.Core
{
    using System;
    using System.Windows;
    using System.Collections.Generic;

    using Providers;
    using Providers.Contracts;
    using ViewModels;

    using Caliburn.Micro;
    
    /// <summary>
    /// Application bootstrapper
    /// </summary>
    public class AppBootstrapper : BootstrapperBase
    {
        #region Private Members

        private readonly SimpleContainer _container = new SimpleContainer();

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public AppBootstrapper()
        {
            _container
                .Singleton<IWindowManager, WindowManager>()
                .Singleton<IEventAggregator, EventAggregator>()
                .Singleton<ILoggerProvider, AppLoggerProvider>()
                .Singleton<ISecureShellTunnelProvider, SecureShellTunnelProvider>();

            _container
                .PerRequest<MainWindowViewModel>();

            _container.
                Instance(new Settings());

            Initialize();
        }

        #region Overrides of BootstrapperBase

        /// <summary>
        /// OnStartup overrides of BootstrapperBase for custom application startup logic
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Startup arguments</param>
        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MainWindowViewModel>();
        }

        /// <summary>
        /// GetInstance override with using custom instances container
        /// </summary>
        /// <param name="service">Type of instance</param>
        /// <param name="key">Key of instance</param>
        /// <returns>Registered instance</returns>
        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }
        
        /// <summary>
        /// GetAllInstances override with using custom instances container
        /// </summary>
        /// <param name="service">Type of instance</param>
        /// <returns>List of instances</returns>
        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        /// <summary>
        /// BuildUp override with using custom instances container
        /// </summary>
        /// <param name="instance">Instance to buildup</param>
        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        #endregion
    }
}
