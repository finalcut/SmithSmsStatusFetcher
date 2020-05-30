# Overview

This project is intended to utilize the Twilio API to fetch the status of messages the Steven Smith campiagn already has the message sid of.

It uses Quartz to schedule a job which fires off at a user-defined interval and process a batch of 50 messages at a time.

The primary database is known by the campaign and contains a table called `assignment_messages`.  In order to track the status of each message
and to capture the error code of each undelivered message a new table is created titled `assignment_messages_status` which can be used to join between the `assignment_messages` and the `drive_contacts` tables.

## Setup

Setup is pretty easy.

1. On your database server in the same schema as the `assignment_messages` table run the dbsetup/*.sql scripts in numeric order.  This will create the table `assignment_messages_status` and then populate it.
2. edit the appsettings.json file to put your twilio info and database connection information in there.


## Usage

Run the app; it's a console app and will log out information as it goes.  When it is done processing it is too dumb to quit so it just prints out a message `"*********************---ALL RECORDS PROCESSED---*********************"` over and over until you kill the app (ctrl+c).

## Gurantee
there is no guarantee that this app will work and that you won't have your API access blocked by Twilio.  I initially ran this with a seed of 160,000+ message ids and it ran without issue.

## Known Issues:

### Entity Framework
If you're good with Entity Framework - I'm not - you can improve this greatly by fixing the `TwilioProcessingService.ReadAllMessagesAsync` process.  It currently uses the Twilio helper library to fetch 1000 records at a time but the change tracking in EF was causing me some grief and I wasn't really sure how to resolve it and I wasn't feeling in the mood to do more research.  A PULL REQUEST would be greatly appreciated.

Basically, in the processing it loops and tries to get the current status record from the db for the message id, if none exists it creates one and inserts the record into the db.  I think during the paging it may get the same message id multiple times so after it creates the record it later tries to fetch it and then update it.  But the update fails becuase EF is already tracking the changes.  I don't know what to do to get rid of the first model EF knows about so it can update the subsequent one.

### Batch Size and Job Frequency

I really should move the batch size and job frequency into appsettings as well.  Its pretty dumb to have them embedded in there.
I could also probably make a setting that determines if it will run on a batch or do the `ReadAllMessagesAsync` process when that is fixed.