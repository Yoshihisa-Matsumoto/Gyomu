
DROP TABLE if exists [dbo].[task_data_log]
GO

DROP TABLE if exists [dbo].[task_data_status]
GO

DROP TABLE if exists [dbo].[task_instance_submit_information]
GO

DROP TABLE if exists [dbo].[task_instance]
GO

DROP TABLE if exists [dbo].[task_data]
GO

DROP TABLE if exists [dbo].[task_info_access_list]
GO

DROP TABLE if exists [dbo].[task_info_cdtbl]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[task_info_cdtbl](
	[application_id] [smallint] NOT NULL,
	[task_id] [smallint] NOT NULL,
	[description] [varchar](100) NOT NULL,
	[assembly_name] [varchar](100) NOT NULL,
	[class_name] [varchar](100) NOT NULL,
	[restartable] [bit] NOT NULL,
 CONSTRAINT [PK_task_info_cdtbl] PRIMARY KEY CLUSTERED 
(
	[application_id] ASC,
	[task_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[task_info_access_list](
	[id] [bigint] IDENTITY(1,1)  NOT NULL,
	[application_id] [smallint] NOT NULL,
	[task_info_id] [smallint] NOT NULL,
	[account_name] [varchar](100) NOT NULL,
	[can_access] [bit] NOT NULL,
	[forbidden] [bit] NOT NULL,
 CONSTRAINT [PK_task_info_access_list] PRIMARY KEY NONCLUSTERED 
(
	[id] ASC

)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE CLUSTERED INDEX [CX_task_info_access_list] ON [dbo].[task_info_access_list]
(
	[application_id] ASC,
	[task_info_id] ASC,
	[account_name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO


CREATE TABLE [dbo].[task_data](
	[id] [bigint] IDENTITY(1,1)  NOT NULL,
	[application_id] [smallint] NOT NULL,
	[task_info_id] [smallint] NOT NULL,
	[entry_date] [datetime]  NOT NULL DEFAULT SYSUTCDATETIME(),
	[entry_author] varchar(30) NOT NULL,
	[parent_task_data_id] bigint NULL,
	[parameter] ntext NULL,
 CONSTRAINT [PK_task_data] PRIMARY KEY NONCLUSTERED 
(
	[id] ASC

)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE CLUSTERED INDEX [CX_task_data] ON [dbo].[task_data]
(
	[entry_date] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE INDEX IX_task_data1 ON dbo.task_data
(
	application_id ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE INDEX IX_task_data2 ON dbo.task_data
(
	task_info_id ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE INDEX IX_task_data3 ON dbo.task_data
(
	entry_author ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE TABLE [dbo].[task_instance](
	[id] [bigint] IDENTITY(1,1)  NOT NULL,
	[task_data_id] [bigint] NOT NULL,
	[entry_date] [datetime]  NOT NULL DEFAULT SYSUTCDATETIME(),
	[entry_author] varchar(30) NOT NULL,
	[task_status] varchar(10) NULL,
	[is_done] bit NOT NULL,
	[status_info_id] bigint NULL,
	[parameter] ntext NULL,
	[comment] ntext NULL,
 CONSTRAINT [PK_task_instance] PRIMARY KEY NONCLUSTERED 
(
	[id] ASC

)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO




CREATE CLUSTERED INDEX [CX_task_instance] ON [dbo].[task_instance]
(
	[entry_date] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE INDEX IX_task_instance1 ON dbo.[task_instance]
(
	task_data_id ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE INDEX IX_task_instance2 ON dbo.[task_instance]
(
	task_status ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO



CREATE TABLE [dbo].[task_instance_submit_information](
	[id] [bigint] IDENTITY(1,1)  NOT NULL,
	[task_instance_id] [bigint] NOT NULL,
	[submit_to] [varchar](30) NULL,
 CONSTRAINT [PK_task_instance_submit_information] PRIMARY KEY NONCLUSTERED 
(
	[id] ASC

)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE CLUSTERED INDEX [CX_task_instance_submit_information] ON [dbo].[task_instance_submit_information]
(
	[task_instance_id] ASC,
	[submit_to] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO


CREATE TABLE [dbo].[task_data_status](
	[task_data_id] [bigint] NOT NULL,
	[task_status] varchar(10) NULL,
	[latest_update_date] [datetime] NOT NULL,
	[latest_task_instance_id] [bigint] NOT NULL,
 CONSTRAINT [PK_task_data_status] PRIMARY KEY CLUSTERED 
(
	[task_data_id] ASC

)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO





CREATE TABLE [dbo].[task_data_log](
	[id] [bigint] IDENTITY(1,1)  NOT NULL,
	[task_data_id] [bigint] NOT NULL,
	[log_time] datetime  NOT NULL DEFAULT SYSUTCDATETIME(),
	[log] [ntext] NOT NULL,
 CONSTRAINT [PK_task_data_log] PRIMARY KEY NONCLUSTERED 
(
	id ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE CLUSTERED INDEX [CX_task_data_log] ON [dbo].[task_data_log]
(
	[task_data_id] ASC,
	[log_time] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
