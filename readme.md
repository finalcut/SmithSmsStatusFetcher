# Overview

This project is intended to utilize the Twilio API to fetch the status of messages the Steven Smith campiagn already has the message sid of.

Now uses parallellization to do 50 at a time every second.  No longer uses quartz or EFCore; does use Dapper for db interactions.

The primary database is known by the campaign and contains a table called `assignment_messages`.  In order to track the status of each message
and to capture the error code of each undelivered message a new table is created titled `assignment_messages_status` which can be used to join between the `assignment_messages` and the `drive_contacts` tables.

This tool uses the Twilio [Fetch a Message api end point](https://www.twilio.com/docs/sms/api/message-resource#fetch-a-message-resource).  Follow that link for more documentation about how that works.  Rather than crafting custom HTTP requests I am using the [C# Twilio Helper Library](https://www.twilio.com/docs/libraries/reference/twilio-csharp/5.13.5/annotated.html).

## Prequisites

1. A twilio account with API access
2. A mariadb or mysql database


## Setup

Setup is pretty easy.

1. On your database server in the same schema as the `assignment_messages` table run the dbsetup/*.sql scripts in numeric order.  This will create the table `assignment_messages_status` and then populate it.
2. edit the appsettings.json file to put specify your settings. See Usage below for all settings.

NOTE: you can alternatively change your structure you'll just have to update some of the code.  I'd keep the assignment_messages_status table if I were you but if you don't have an `assigment_messages` table with a `message_sid` and `contact_id` column just make sure you update `dbsetup\1-create_table.sql` to account for the new structure.  I'd suggest you keep the `status, error_code, and message_id` columns in the `assigment_message_status` table but the `contact_id` isn't really needed.

If you don't have an `assignment_messages` table you'll need to update the `dbsetup\2-seed_table.sql` to get the message_ids to start with from somewhere.  This app is driven entirely from the resultant `assignement_message_status` table and looks for records where the `status` column is null.  It utilizes the three columns `status, error_code, and message_id`.  So again, you need to have those columns and the `message_id` column needs to be populated for this app to be useful.

## Usage

Run the app; it's a console app and will NOT log out any information as it goes.  When it is done processing it will just quit.

* There are a few arguments you need to provide in one fashion or another:
  * Batch:Size  - the number of messages to process each iteration - recommended max is 50
  * Batch:ApiCallPauseInMilliseconds - number of milliseconds between each batch iteraiton - recommended minimum is 1000 (1 second)
  * TwilioSecrets:AuthToken - the authorization token provided by twilio.  Either pass this at the command line or put in usersecrets
  * TwilioSecrets:AccountSid - your account id.  This isn't sensitive but Id still put it at the commnad line or in user secrets
  * Database:Username - username that has access to your mariadb/mysql database with read/write privledges
  * Database:Server - server name of your mariadb/mysql database.  I used localhost.
  * Database:Password - password associatied with the Database:Username user.
  * Database:Database - the database that contains the tables to be read/write by Database:Username

## Gurantee
There is no guarantee that this app will work and that you won't have your API access blocked by Twilio.  I initially ran this with a seed of many tens of thousands of message ids and it ran without issue.

## Credits

I didn't know anything about the `Parallel` class before I wrote this so [this blog article](https://www.nimaara.com/practical-parallelization-with-map-reduce-in-c/) by Nima Ara was super helpful.