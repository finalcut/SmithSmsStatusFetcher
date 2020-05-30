using Microsoft.Extensions.Logging;
using Quartz;
using SmithSmsStatusFetcher.Services;
using System.Threading.Tasks;

namespace SmithSmsStatusFetcher.Jobs
{
    public class MessageStatusProcessorJob : IJob
    {
        private readonly ILogger<MessageStatusProcessorJob> _logger;
        private readonly TwilioProcessingService _service;

        public MessageStatusProcessorJob(
            TwilioProcessingService service,
            ILogger<MessageStatusProcessorJob> logger)
        {
            _logger = logger;
            _service = service;

            _logger.LogInformation("Created instance of Job");
        }





        public async Task Execute(IJobExecutionContext context)
        {

            // await _service.ReadOneMessagesAsync(sid);

            int batchSize = 50;
            await _service.ReadBatchoFMesssagesAsync(batchSize);

            //await _service.ReadAllMessagesAsync();

            //await Task.Delay(1000);


            _logger.LogInformation("Executing MessageStatusProcessorJob");

            _logger.LogInformation("Finished Executing MessageStatusProcessorJob");


        }



    }
}
