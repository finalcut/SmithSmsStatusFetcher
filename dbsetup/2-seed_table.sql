insert into assignment_messages_status (message_sid, contact_id)
select message_sid, contact_id from assignment_messages;
