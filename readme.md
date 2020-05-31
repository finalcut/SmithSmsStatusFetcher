# Overview

This project is intended to utilize the Twilio API to fetch the status of messages the Steven Smith campiagn already has the message sid of.

Now uses parallellization to do 50 at a time every second.  No longer uses quartz or EFCore; does use Dapper for db interactions.

The primary database is known by the campaign and contains a table called `assignment_messages`.  In order to track the status of each message
and to capture the error code of each undelivered message a new table is created titled `assignment_messages_status` which can be used to join between the `assignment_messages` and the `drive_contacts` tables.

## Setup

Setup is pretty easy.

1. On your database server in the same schema as the `assignment_messages` table run the dbsetup/*.sql scripts in numeric order.  This will create the table `assignment_messages_status` and then populate it.
2. edit the appsettings.json file to put your twilio info and database connection information in there.

## Usage

Run the app; it's a console app and will NOT log out any information as it goes.  When it is done processing it should just quit.

## Gurantee
there is no guarantee that this app will work and that you won't have your API access blocked by Twilio.  I initially ran this with a seed of many tens of thousands of message ids and it ran without issue.

## Known Issues:

### Batch Size and Batch Pause length

I really should move the batch size and pause length into appsettings as well.  Its pretty dumb to have them embedded in there.
