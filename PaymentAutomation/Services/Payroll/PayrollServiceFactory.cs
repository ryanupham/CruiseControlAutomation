using PaymentAutomation.Models;
using RazorLight;

namespace PaymentAutomation.Services.Payroll
{
    public interface IPayrollServiceFactory
    {
        IPayrollService GetService();
    }

    internal class PayrollServiceFactory : IPayrollServiceFactory
    {
        private readonly AppSettings appSettings;
        private readonly IReportingApiClient reportingApiClient;
        private readonly IRazorLightEngine razorEngine;
        private readonly IPrintToPdfService pdfService;
        private readonly IRolloverService rolloverService;

        public PayrollServiceFactory(
            AppSettings appSettings,
            IReportingApiClient reportingApiClient,
            IRazorLightEngine razorEngine,
            IPrintToPdfService pdfService,
            IRolloverService rolloverService
        )
        {
            this.appSettings = appSettings;
            this.reportingApiClient = reportingApiClient;
            this.razorEngine = razorEngine;
            this.pdfService = pdfService;
            this.rolloverService = rolloverService;
        }

        public IPayrollService GetService()
        {
            var manager = appSettings.Agents.Values.First(a => a.IsManager)!;

            var consolidatedPostProcessors = new List<IPayrollPostProcessor>();
            var agentPostProcessors = new List<IPayrollPostProcessor>();

            var isDryRun = Environment.GetCommandLineArgs().Contains("--dry-run");
            var isNoEmail = Environment.GetCommandLineArgs().Contains("--no-email");
            var shouldEmail = !(isDryRun || isNoEmail);

            if (shouldEmail)
            {
                consolidatedPostProcessors.AddRange(new List<IPayrollPostProcessor>
                {
                    new CopyConsolidatedFilePostProcessor(appSettings.OutputFolder),
                    new EmailConsolidatedReportPostProcessor(
                        appSettings.Email,
                        manager.Name,
                        manager.Email,
                        manager.Name,
                        manager.Email
                    ),
                });
                agentPostProcessors.AddRange(new List<IPayrollPostProcessor>
                {
                    new CopyAgentFilePostProcessor(appSettings.OutputFolder),
                    new EmailAgentReportPostProcessor(
                        appSettings.Email,
                        manager.Name,
                        manager.Email,
                        manager.Name,
                        manager.Email
                    ),
                });
            }

            return new PayrollService(
                reportingApiClient,
                razorEngine,
                pdfService,
                rolloverService,
                consolidatedPostProcessors,
                agentPostProcessors
            );
        }
    }
}
