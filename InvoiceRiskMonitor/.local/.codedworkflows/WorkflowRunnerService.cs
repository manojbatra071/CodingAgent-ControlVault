using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UiPath.CodedWorkflows;
using UiPath.CodedWorkflows.Interfaces;
using UiPath.Activities.Contracts;
using InvoiceRiskMonitor;
using System.IO;
using System.Linq;
using UiPath.Core;

[assembly: WorkflowRunnerServiceAttribute(typeof(InvoiceRiskMonitor.WorkflowRunnerService))]
namespace InvoiceRiskMonitor
{
    public class WorkflowRunnerService
    {
        private readonly ICodedWorkflowServices _services;
        public WorkflowRunnerService(ICodedWorkflowServices services)
        {
            _services = services;
        }

        /// <summary>
        /// Invokes the Main.xaml
        /// </summary>
        /// <param name="isolated">Indicates whether to isolate executions (run them within a different process)</param>
        public void Main(System.Boolean isolated = false)
        {
            var result = _services.WorkflowInvocationService.RunWorkflow(@"Main.xaml", new Dictionary<string, object> { }, default, isolated, default, GetAssemblyName());
        }

        /// <summary>
        /// Invokes the ProcessInvoices.cs
        /// </summary>
        /// <param name="isolated">Indicates whether to isolate executions (run them within a different process)</param>
        public void ProcessInvoices(System.String in_ConfigPath = "Config.xlsx", System.Boolean isolated = false)
        {
            var result = _services.WorkflowInvocationService.RunWorkflow(@"ProcessInvoices.cs", new Dictionary<string, object> { { "in_ConfigPath", in_ConfigPath } }, default, isolated, default, GetAssemblyName());
        }

        private string GetAssemblyName()
        {
            var assemblyProvider = _services.Container.Resolve<ILibraryAssemblyProvider>();
            return assemblyProvider.GetLibraryAssemblyName(GetType().Assembly);
        }
    }
}