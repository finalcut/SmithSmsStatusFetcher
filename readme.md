# Overview

This project is intended to utilize the Twilio API to fetch the status of messages the Steven Smith campiagn already has the message sid of.

It uses Quartz to schedule a job which fires off at a user-defined interval and process a batch of 50 messages at a time.

The primary database is known by the campaign and contains a table called `assignment_messages`.  In order to track the status of each message
and to capture the error code of each undelivered message a new table is created titled `assignment_messages_status` which can be used to join between the `assignment_messages` and the `drive_contacts` tables.

## Parallelization
Yeah, this would seriously be better if it were parallelized but; it isn't.  I might refactor it a bit and try that path.  Like, a batch of 100 ids could be loaded from the db (or a flat file) and then a bunch of parallel tasks could go through the list and do the api call and update the db.

## Entity Framework and Quartz
I used them both becuase I am sort of familiar with them and I couldn't find any particularly useful documentation for just connecting to MySQL (MariaDb) that didn't go down the EF road.  I'd prefer to not use EF for this; I don't need change tracking or any of the other crap involved.

After I'd written it I found this blog post: https://www.blinkingcaret.com/2018/04/25/orm-less-data-access-in-net-core/  which I might use to refactor with.  I'm really not digging the whole EF part of this project.  Alternatively it sounds like Dapper would have been a better choice for this project giving me some basic ORM without all the overhead of EF.

I used quartz because I've used it before and it's easy to spin up additional tasks every second.

### Concern with this approach
The negative is I'm almost certainly repeating myself with some of the ids being processed in the batch.  I am not tracking that part but I'm pretty sure the first job hasn't finished by the time the second one starts so some of the ids are being re-processed because of the way I'm loading ids records.  I could probably reduce that by looping and pulling out one record per loop and process it.. then get the next unprocessed record.  There still might be a few contention points but for the most part it would probably involve fewer api calls at the expense of more Db calls.

### Other Problems
This is not fast.  Like, this could be A LOT faster with parallelization.  I'm not sure how long it took to process the many tens of thousands of records but it took A LOT longer than it should have.  I also stopped and restarted a couple times to test out the different methods.  The bulk runner really got bogged down and slowed things down - but even the default runner is slower than it needs to be.

I wish I had set a timer.

## Alternative Run Modes

You have to edit the source at the moment to switch run modes.. but here they are.

1. Default - runs the batch of 50 twilio lookups at a time every second.  Basically, gets a message id from the db, asks twilio about it, updates the status.
2. All - Gets pages of results back from Twilio.  For each message returned looks up the message in the db, updates it if found, and saves it. Currently ignores messages the db doesn't know about.  See Known Issues below.  Also note this gets slow over time if you are processing a lot  of records becuase EF gets slow managing so many things in memory.  Finally, make sure you comment out line 65 and uncomment line 66 of QuartzHostedService or update the code to make it better.  Basically, you only want it running one time at start up if you go this route.

## Setup

Setup is pretty easy.

1. On your database server in the same schema as the `assignment_messages` table run the dbsetup/*.sql scripts in numeric order.  This will create the table `assignment_messages_status` and then populate it.
2. edit the appsettings.json file to put your twilio info and database connection information in there.


## Usage

Run the app; it's a console app and will log out information as it goes.  When it is done processing (if you're using the batch processing) it is too dumb to quit so it just prints out a message `"*********************---ALL RECORDS PROCESSED---*********************"` over and over until you kill the app (ctrl+c).

## Gurantee
there is no guarantee that this app will work and that you won't have your API access blocked by Twilio.  I initially ran this with a seed of many tens of thousands of message ids and it ran without issue.

## Known Issues:

### Entity Framework
If you're good with Entity Framework - I'm not - you can improve this greatly by fixing the `TwilioProcessingService.ReadAllMessagesAsync` process.  It currently uses the Twilio helper library to fetch 1000 records at a time but the change tracking in EF was causing me some grief and I wasn't really sure how to resolve it and I wasn't feeling in the mood to do more research.  A PULL REQUEST would be greatly appreciated.

Basically, in the processing it loops and tries to get the current status record from the db for the message id, if none exists it creates one and inserts the record into the db.  I think during the paging it may get the same message id multiple times so after it creates the record it later tries to fetch it and then update it.  But the update fails becuase EF is already tracking the changes.  I don't know what to do to get rid of the first model EF knows about so it can update the subsequent one.

### Batch Size and Job Frequency

I really should move the batch size and job frequency into appsettings as well.  Its pretty dumb to have them embedded in there.
I could also probably make a setting that determines if it will run on a batch or do the `ReadAllMessagesAsync` process when that is fixed.
