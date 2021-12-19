using EEG_Project.Services;
using Microsoft.Extensions.DependencyInjection;
using Prism.Ioc;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EEG_Project
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {

            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var container = containerRegistry.GetContainer();
            var services = new ServiceCollection();
            containerRegistry.Register<IRecordingsService, RecordingsService>();
            containerRegistry.Register<IHttpService, HttpService>();
            containerRegistry.RegisterDialog<RawDataView>();
            containerRegistry.RegisterDialog<TrainModelView>();

        }
    }
}
