using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmithSmsStatusFetcher.Models;
using SmithSmsStatusFetcher.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace SmithSmsStatusFetcher.Services
{

    public class TwilioProcessingService
    {
        private readonly TwilioSecrets _twilioSecrets;
        private readonly SmithDbContext _dbContext;
        private readonly ILogger<TwilioProcessingService> _logger;
        private List<string> _processed;

        public TwilioProcessingService(IOptions<TwilioSecrets> twilioSecrets, SmithDbContext dbContext,
            ILogger<TwilioProcessingService> logger)
        {
            _twilioSecrets = twilioSecrets.Value;
            _dbContext = dbContext;
            _logger = logger;
            _processed = new List<string>();

        }

        public async Task ReadAllMessagesAsync()
        {
            TwilioClient.Init(_twilioSecrets.AccountSid, _twilioSecrets.AuthToken);

            var messages = MessageResource.Read(
                dateSentBefore: DateTime.Now,
                dateSentAfter: new DateTime(2020, 4, 1, 0, 0, 0),
                limit: 1000
           );

            messages.AutoPaging = true;

            foreach (var message in messages)
            {
                await ProcessOneAsync(message, null);
            }
        }


        public async Task ReadBatchoFMesssagesAsync(int batchSize)
        {
            var sql = $"SELECT * FROM assigment_messages_status WHERE STATUS IS NULL ORDER BY message_sid LIMIT {batchSize}";

            List<AssigmentMessagesStatus> statuses = await _dbContext.Set<AssigmentMessagesStatus>()
                                                                    .AsQueryable()
                                                                    .Where(o => o.Status == null)
                                                                    .OrderBy(o => o.MessageSid)
                                                                    .Take(batchSize)
                                                                    .ToListAsync();

            foreach (var status in statuses)
            {
                TwilioClient.Init(_twilioSecrets.AccountSid, _twilioSecrets.AuthToken);

                var message = MessageResource.Fetch(
                    pathSid: status.MessageSid
               );

                await ProcessOneAsync(message, status);
            }

        }

        public async Task ReadOneMessagesAsync(string sid)
        {
            TwilioClient.Init(_twilioSecrets.AccountSid, _twilioSecrets.AuthToken);

            var message = MessageResource.Fetch(
                pathSid: sid
           );

            await ProcessOneAsync(message, null);
        }

        public async Task<AssigmentMessagesStatus> GetMessageStatusRecordAsync(string sid)
        {
            return await _dbContext.Set<AssigmentMessagesStatus>().Where(o => o.MessageSid == sid).SingleOrDefaultAsync();

        }

        public async Task CreateNewMessageStatusAsync(MessageResource message)
        {
            AssigmentMessagesStatus status = new AssigmentMessagesStatus()
            {
                MessageSid = message.MessagingServiceSid,
                Status = message.Status.ToString()
            };
            _ = await _dbContext.AddAsync(status);
            _ = await _dbContext.SaveChangesAsync();



        }

        public async Task UpdateMessageStatusAsync(AssigmentMessagesStatus statusRecord, MessageResource message)
        {
            statusRecord.Status = message.Status.ToString();
            statusRecord.ErrorStatus = message.ErrorCode.ToString();
            //_dbContext.Update(statusRecord);
            _ = await _dbContext.SaveChangesAsync();
        }



        public async Task ProcessOneAsync(MessageResource message, AssigmentMessagesStatus? statusRecord)
        {
            _logger.LogInformation($"Processing message with SID: {message.Sid} and status {message.Status}");
            if (!_processed.Contains(message.Sid))
            {

                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        if (statusRecord == null)
                        {
                            statusRecord = await GetMessageStatusRecordAsync(message.Sid);
                        }
                        if (IsDefault(statusRecord))
                        {
                            // keep track of messages we didn't know about..
                            //await CreateNewMessageStatusAsync(message);

                        }
                        else
                        {
                            await UpdateMessageStatusAsync(statusRecord, message);

                        }
                        await transaction.CommitAsync();
                        _processed.Add(message.Sid);

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed Processing message with SID: {message.Sid} and status {message.Status}", null);
                        Console.WriteLine(ex.Message);
                        await transaction.RollbackAsync();
                        throw ex;

                    }
                }
            }


        }

        private bool IsDefault<TSomeType>(TSomeType obj)
        {
            return (EqualityComparer<TSomeType>.Default.Equals(obj, default));
        }
    }
}
