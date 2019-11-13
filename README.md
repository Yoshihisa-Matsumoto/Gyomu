# Gyomu
Gyomu, which means Enterprise or Operation in Japanese, is used in enterprise.
The purpose of this library is 
1. Shorten development time through framework
2. Shorten trouble shooting in testing or in production 

This library won't be used by library itself. It's used with RDB ( Now Postgres & MSSQL is supported )

# Error handling / Logging
All status/ log to be stored in database
Based on setting, status/log to be sent via email.
Like log4net, etc, there are several logging type, such as Info / Warning / Business Notification / IT Error.
For IT error, the status/log contains stack trace so that developer could resolve issue very quickly.
The error is sent via email promptly so it doesn't take time to search inside log file.

When you use this framework, at least all public method should return StatusCode object.
StatusCode object contains error/log information and when it's instantiated, it would be recorded in DB and be sent via email if necessary.

StatusCode's id is Int (32bit)
1st 12bit is ApplicationID which must be unique per your library
2nd 4bit is logging type, such as Info / Warning / Business Notification / IT Error.
Remaining 16bit is Error ID which must be unique within your library.
Combining these number lead to uniqueness among all libraries.

Your assigned ApplicationID must be registered in DB table, apps_info_cdtbl. apps_info_cdtbl column would be ApplicationID.
1-4095 can be assigned and 1 is already used in Gyomu library.
I would recommend you to set some rule by yourselves, such as common library would be <= 99, business logic library would be >=100 <500, Server side like web library would be >=500 <2000, GUI would be >=2000 or something like that.

Email recipient information need to be setup per ApplicationID on status_handler table. You can set To / CC address on this table.



