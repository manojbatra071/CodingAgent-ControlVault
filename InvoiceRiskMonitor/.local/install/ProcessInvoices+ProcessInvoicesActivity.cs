using System;
using System.Activities;
using UiPath.CodedWorkflows;
using UiPath.CodedWorkflows.Utils;
using System.Runtime;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UiPath.Core;
using InvoiceRiskMonitor;

namespace InvoiceRiskMonitor
{
    [System.ComponentModel.Browsable(false)]
    public class ProcessInvoicesActivity : System.Activities.Activity
    {
        public InArgument<System.String> in_ConfigPath { get; set; } = new InArgument<System.String>("Config.xlsx") ; public ProcessInvoicesActivity()
        {
            this.Implementation = () =>
            {
                return new ProcessInvoicesActivityChild()
                {
                    in_ConfigPath = (this.in_ConfigPath == null ? (InArgument<System.String>)Argument.CreateReference((Argument)new InArgument<System.String>(), "in_ConfigPath") : (InArgument<System.String>)Argument.CreateReference((Argument)this.in_ConfigPath, "in_ConfigPath")),
                };
            };
        }
    }

    internal class ProcessInvoicesActivityChild : UiPath.CodedWorkflows.AsyncTaskCodedWorkflowActivity
    {
        public InArgument<System.String> in_ConfigPath { get; set; } = new InArgument<System.String>("Config.xlsx") ; public System.Collections.Generic.IDictionary<string, object> newResult { get; set; }

        public ProcessInvoicesActivityChild()
        {
            DisplayName = "ProcessInvoices";
        }

        protected override async System.Threading.Tasks.Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, System.Threading.CancellationToken cancellationToken)
        {
            var var_in_ConfigPath = in_ConfigPath.Get(context);
            var codedWorkflow = new global::InvoiceRiskMonitor.ProcessInvoices();
            CodedWorkflowHelper.Initialize(codedWorkflow, new UiPath.CodedWorkflows.Utils.CodedWorkflowsFeatureChecker(new System.Collections.Generic.List<string>() { UiPath.CodedWorkflows.Utils.CodedWorkflowsFeatures.AsyncEntrypoints }), context);
            await System.Threading.Tasks.Task.Run(() => CodedWorkflowHelper.RunWithExceptionHandlingAsync(() =>
            {
                if (codedWorkflow is IBeforeAfterRun codedWorkflowWithBeforeAfter)
                {
                    codedWorkflowWithBeforeAfter.Before(new BeforeRunContext() { RelativeFilePath = "ProcessInvoices.cs" });
                }

                return System.Threading.Tasks.Task.CompletedTask;
            }, () =>
            {
                CodedExecutionHelper.Run(() =>
                {
                    {
                        codedWorkflow.Execute(var_in_ConfigPath);
                        newResult = new System.Collections.Generic.Dictionary<string, object>
                        {
                        };
                    }
                }, cancellationToken);
                return System.Threading.Tasks.Task.FromResult(newResult);
            }, (exception, outArgs) =>
            {
                if (codedWorkflow is IBeforeAfterRun codedWorkflowWithBeforeAfter)
                {
                    codedWorkflowWithBeforeAfter.After(new AfterRunContext() { RelativeFilePath = "ProcessInvoices.cs", Exception = exception });
                }

                return System.Threading.Tasks.Task.CompletedTask;
            }), cancellationToken);
            return endContext =>
            {
            };
        }
    }
}