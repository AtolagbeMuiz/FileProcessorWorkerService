using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileProcessor.BackgroundService
{
    public class FileProcessor : Microsoft.Extensions.Hosting.BackgroundService
    {
        private readonly ILogger<FileProcessor> _logger;
        private readonly IServiceScopeFactory _serviceScope;

        public FileProcessor(ILogger<FileProcessor> logger, IServiceScopeFactory serviceScope)
        {
            this._serviceScope = serviceScope;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //background service that checks for new files created
            //checks if the newly created file has the same name as the a corresponding pdf file
            //if not, check thrice: then if not, log failure
            //if it has the same name (matches) as the corresponding pdf file, 
            //proceed to process the xml file in the following steps
            //open the xml file and convert the node values and names to a C# object
            //copy the pdf to another folder named "Output" nameing the file in the format mrn_documentType_encounterDate NB ecnpunterdate format is dd-mm-yyyy e.g. 1234567D_LCVGP_07-05-2024.pdf
            //once this is done, i.e. pdf file has been placed into the output folder.
            //remove/delete the xml and pdf from the input folder
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunJob(stoppingToken);
                    await Task.Delay(18000000, stoppingToken); //5hrs
                    //await Task.Delay(180000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // catch the cancellation exception
                    // to stop execution
                    return;
                }
            }

        }

        private async Task RunJob(CancellationToken token)
        {
            using (var scope = _serviceScope.CreateScope())
            {
                var fileProcessorService = scope.ServiceProvider.GetRequiredService<FileProcessorService>();
                fileProcessorService.FileWatcher();
            }
        }

    }
}
