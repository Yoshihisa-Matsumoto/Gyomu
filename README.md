# Gyomu
Gyomu, which means Enterprise or Operation in Japanese, is used in enterprise.
The purpose of this library is 
1. Shorten development time through framework
2. Shorten trouble shooting in testing or in production 

This library doesn't exist by library itself. It's used with RDB ( Now Postgres & MSSQL is supported )

# Error handling / Logging
All status/ log to be stored in database
Based on setting, status/log to be sent via email.
Like log4net, etc, there are several logging type, such as Info / Warning / Business Notification / IT Error.
For IT error, the status/log contains stack trace so that developer could resolve issue very quickly.
The error is sent via email promptly so it doesn't take time to search inside log file.



