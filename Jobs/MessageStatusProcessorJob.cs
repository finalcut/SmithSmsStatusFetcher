using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using SmithSmsStatusFetcher.Models;
using SmithSmsStatusFetcher.Settings;
using System.Threading.Tasks;

namespace SmithSmsStatusFetcher.Jobs
{
    public class MessageStatusProcessorJob : IJob
    {
        private readonly TwilioSecrets _twilioSecrets;
        private readonly ILogger<MessageStatusProcessorJob> _logger;
        private readonly SmithDbContext _dbContext;

        public MessageStatusProcessorJob(
            IOptions<TwilioSecrets> secrets,
            SmithDbContext dbContext,
            ILogger<MessageStatusProcessorJob> logger)
        {
            _twilioSecrets = secrets.Value;
            _dbContext = dbContext;
            _logger = logger;

            _logger.LogInformation("Created instance of Job");
        }





        public async Task Execute(IJobExecutionContext context)
        {

            _logger.LogInformation("Executing MessageStatusProcessorJob");
            _logger.LogInformation($"AccountSid: {_twilioSecrets.AccountSid}");
            await Task.Delay(1000);
            _logger.LogInformation("Finished Executing MessageStatusProcessorJob");
            /*

            TwilioClient.Init(_twilioSecrets.AccountSid, _twilioSecrets.AuthToken);

            Twilio.Base.ResourceSet<MessageResource> messages = MessageResource.Read(
                dateSentBefore: DateTime.Now,
                dateSentAfter: new DateTime(2020, 4, 1, 0, 0, 0),
                limit: 1000
           );

            messages.AutoPaging = true;


            foreach (var record in messages)
            {
                // look up the message in the database using the message_sid column of the assigment_messages table

                // if it exists; insert the message_sid, contact_id, the status of the current message into the assigment_messages_status table

                Console.WriteLine(record.Sid);
            }
            */
        }


    }
}
