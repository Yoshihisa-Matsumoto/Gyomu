DROP INDEX if exists  [CX_gyomu_status_info] ON [dbo].[gyomu_status_info] WITH ( ONLINE = OFF ) 
GO

DROP TABLE if exists gyomu_status_info;

DROP TABLE if exists gyomu_status_handler;

DROP TABLE if exists gyomu_apps_info_cdtbl;

DROP TABLE if exists gyomu_status_type_cdtbl;

DROP TABLE if exists gyomu_market_holiday;

DROP TABLE IF EXISTS gyomu_milestone_daily;
GO

DROP TABLE IF EXISTS gyomu_variable_parameter;
GO

DROP TABLE IF EXISTS gyomu_param_master;
GO


DROP TABLE if exists gyomu_task_data_log;
GO

DROP TABLE if exists gyomu_task_data_status;
GO

DROP TABLE if exists gyomu_task_instance_submit_information;
GO

DROP TABLE if exists gyomu_task_instance;
GO

DROP TABLE if exists gyomu_task_data;
GO

DROP TABLE if exists gyomu_task_info_access_list;
GO

DROP TABLE if exists gyomu_task_info_cdtbl;
GO

DROP TABLE if exists gyomu_service_cdtbl;
GO

DROP TABLE if exists gyomu_service_type_cdtbl;
GO

DROP TABLE if exists gyomu_task_scheduler_config;
GO


CREATE TABLE [dbo].[gyomu_apps_info_cdtbl](
	[application_id] [smallint] NOT NULL,
	[description] [varchar](50) NULL,
	[mail_from_address] [varchar](200) NULL,
	[mail_from_name] [varchar](200) NULL,
 CONSTRAINT [PK_gyomu_apps_info_cdtbl] PRIMARY KEY CLUSTERED 
(
	[application_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[gyomu_status_type_cdtbl](
	[status_type] [smallint] NOT NULL,
	[description] [varchar](15) NULL,
 CONSTRAINT [PK_gyomu_status_type_cdtbl] PRIMARY KEY CLUSTERED 
(
	[status_type] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

INSERT INTO [gyomu_status_type_cdtbl] VALUES (0,'INFO');
INSERT INTO [gyomu_status_type_cdtbl] VALUES (1,'WARNING');
INSERT INTO [gyomu_status_type_cdtbl] VALUES (2,'ERROR_BUSINESS');
INSERT INTO [gyomu_status_type_cdtbl] VALUES (8,'ERROR_DEVEL');

CREATE TABLE [dbo].[gyomu_status_handler](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[application_id] [smallint] NOT NULL,
	[region] [varchar](3) NULL,
	[status_type] [smallint] NULL,
	[recipient_address] [varchar](200) NULL,
	[recipient_type] [varchar](3) NULL,
 CONSTRAINT [PK_gyomu_status_handler_cdtbl] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[gyomu_status_info](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[application_id] [smallint] NOT NULL,
	[entry_date] [datetime] NOT NULL DEFAULT SYSUTCDATETIME(),
	[entry_author] [varchar](30) NOT NULL,
	[status_type] [smallint] NOT NULL,
  error_id smallint NOT NULL,
	[instance_id] [int] NOT NULL,
	[hostname] [varchar](50) NULL,
	[summary] [nvarchar](400) NULL,
	[description] [nvarchar](1000) NULL,
	[developer_info] ntext NULL,
 CONSTRAINT [PK_gyomu_status_info] PRIMARY KEY NONCLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


CREATE CLUSTERED INDEX [CX_gyomu_status_info] ON [dbo].[gyomu_status_info]
(
	[entry_date] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO




CREATE TABLE [dbo].[gyomu_market_holiday](
	[market] [varchar](10) NOT NULL,
	year smallint NOT NULL,
	holiday char(8) NOT NULL,
 CONSTRAINT [PK_gyomu_market_holiday] PRIMARY KEY CLUSTERED 
(
	[market] ASC,
	[holiday] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE INDEX IX_gyomu_market_holiday ON gyomu_market_holiday
(market ASC,year ASC)
GO

CREATE TABLE [dbo].[gyomu_milestone_daily](
	[target_date] [varchar](8) NOT NULL,
	[milestone_id] varchar(200) NOT NULL,
	[update_time] [datetime] NOT NULL DEFAULT SYSUTCDATETIME(),
 CONSTRAINT [PK_gyomu_milestone_daily] PRIMARY KEY CLUSTERED 
(
	[target_date] ASC,[milestone_id]
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE  INDEX IX_gyomu_milestone_daily ON gyomu_milestone_daily
(
	milestone_id
)
GO

CREATE TABLE [dbo].[gyomu_variable_parameter](
	variable_key varchar(20) NOT NULL,
	description varchar(200) NOT NULL,
 CONSTRAINT [PK_gyomu_variable_parameter] PRIMARY KEY CLUSTERED 
(
	variable_key
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

INSERT INTO gyomu_variable_parameter VALUES('BBOM','Business Day of Beginning Of Month');
INSERT INTO gyomu_variable_parameter VALUES('BBOY','Business Day of Beginning Of Year');
INSERT INTO gyomu_variable_parameter VALUES('BOM','Beginning of Month');
INSERT INTO gyomu_variable_parameter VALUES('BOY','Beginning of Year');
INSERT INTO gyomu_variable_parameter VALUES('BEOM','Business Day of End Of Month');
INSERT INTO gyomu_variable_parameter VALUES('BEOY','Business Day of End Of Year');
INSERT INTO gyomu_variable_parameter VALUES('EOM','End of Month');
INSERT INTO gyomu_variable_parameter VALUES('EOY','End Day of Year');
INSERT INTO gyomu_variable_parameter VALUES('NEXTBBOM','Business Day of Next Beginning Of Month');
INSERT INTO gyomu_variable_parameter VALUES('NEXTBUS','Previous Business Day');
INSERT INTO gyomu_variable_parameter VALUES('NEXTDAY','Next Day');
INSERT INTO gyomu_variable_parameter VALUES('NEXTBEOM','Business Day of Next End Of Month');
INSERT INTO gyomu_variable_parameter VALUES('PARAMMASTER','From param_master');
INSERT INTO gyomu_variable_parameter VALUES('PREVBUS','Previous Business Day');
INSERT INTO gyomu_variable_parameter VALUES('PREVDAY','Previous Day');
INSERT INTO gyomu_variable_parameter VALUES('PREVBEOM',' Business Day of Previous End Of Month');
INSERT INTO gyomu_variable_parameter VALUES('TODAY','Today');
GO


CREATE TABLE [dbo].[gyomu_param_master](
	[item_key] [varchar](50) NOT NULL,
	[item_value] ntext NOT NULL,
	[item_fromdate] [varchar](8) NOT NULL default ''
 CONSTRAINT [PK_gyomu_param_master] PRIMARY KEY CLUSTERED 
(
	[item_key] ASC,[item_fromdate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO



CREATE TABLE [dbo].[gyomu_task_info_cdtbl](
	[application_id] [smallint] NOT NULL,
	[task_id] [smallint] NOT NULL,
	[description] [varchar](100) NOT NULL,
	[assembly_name] [varchar](100) NOT NULL,
	[class_name] [varchar](100) NOT NULL,
	[restartable] [bit] NOT NULL,
 CONSTRAINT [PK_gyomu_task_info_cdtbl] PRIMARY KEY CLUSTERED 
(
	[application_id] ASC,
	[task_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[gyomu_task_info_access_list](
	[id] [bigint] IDENTITY(1,1)  NOT NULL,
	[application_id] [smallint] NOT NULL,
	[task_info_id] [smallint] NOT NULL,
	[account_name] [varchar](100) NOT NULL,
	[can_access] [bit] NOT NULL,
	[forbidden] [bit] NOT NULL,
 CONSTRAINT [PK_gyomu_task_info_access_list] PRIMARY KEY NONCLUSTERED 
(
	[id] ASC

)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE CLUSTERED INDEX [CX_gyomu_task_info_access_list] ON [dbo].[gyomu_task_info_access_list]
(
	[application_id] ASC,
	[task_info_id] ASC,
	[account_name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO


CREATE TABLE [dbo].[gyomu_task_data](
	[id] [bigint] IDENTITY(1,1)  NOT NULL,
	[application_id] [smallint] NOT NULL,
	[task_info_id] [smallint] NOT NULL,
	[entry_date] [datetime]  NOT NULL DEFAULT SYSUTCDATETIME(),
	[entry_author] varchar(30) NOT NULL,
	[parent_task_data_id] bigint NULL,
	[parameter] ntext NULL,
 CONSTRAINT [PK_gyomu_task_data] PRIMARY KEY NONCLUSTERED 
(
	[id] ASC

)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE CLUSTERED INDEX [CX_gyomu_task_data] ON [dbo].[gyomu_task_data]
(
	[entry_date] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE INDEX IX_gyomu_task_data1 ON dbo.gyomu_task_data
(
	application_id ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE INDEX IX_gyomu_task_data2 ON dbo.gyomu_task_data
(
	task_info_id ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE INDEX IX_gyomu_task_data3 ON dbo.gyomu_task_data
(
	entry_author ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE TABLE [dbo].[gyomu_task_instance](
	[id] [bigint] IDENTITY(1,1)  NOT NULL,
	[task_data_id] [bigint] NOT NULL,
	[entry_date] [datetime]  NOT NULL DEFAULT SYSUTCDATETIME(),
	[entry_author] varchar(30) NOT NULL,
	[task_status] varchar(10) NULL,
	[is_done] bit NOT NULL,
	[status_info_id] bigint NULL,
	[parameter] ntext NULL,
	[comment] ntext NULL,
 CONSTRAINT [PK_gyomu_task_instance] PRIMARY KEY NONCLUSTERED 
(
	[id] ASC

)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO




CREATE CLUSTERED INDEX [CX_gyomu_task_instance] ON [dbo].[gyomu_task_instance]
(
	[entry_date] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE INDEX IX_gyomu_task_instance1 ON dbo.[gyomu_task_instance]
(
	task_data_id ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE INDEX IX_gyomu_task_instance2 ON dbo.[gyomu_task_instance]
(
	task_status ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO



CREATE TABLE [dbo].[gyomu_task_instance_submit_information](
	[id] [bigint] IDENTITY(1,1)  NOT NULL,
	[task_instance_id] [bigint] NOT NULL,
	[submit_to] [varchar](30) NULL,
 CONSTRAINT [PK_gyomu_task_instance_submit_information] PRIMARY KEY NONCLUSTERED 
(
	[id] ASC

)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE CLUSTERED INDEX [CX_gyomu_task_instance_submit_information] ON [dbo].[gyomu_task_instance_submit_information]
(
	[task_instance_id] ASC,
	[submit_to] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO


CREATE TABLE [dbo].[gyomu_task_data_status](
	[task_data_id] [bigint] NOT NULL,
	[task_status] varchar(10) NULL,
	[latest_update_date] [datetime] NOT NULL,
	[latest_task_instance_id] [bigint] NOT NULL,
 CONSTRAINT [PK_gyomu_task_data_status] PRIMARY KEY CLUSTERED 
(
	[task_data_id] ASC

)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO





CREATE TABLE [dbo].[gyomu_task_data_log](
	[id] [bigint] IDENTITY(1,1)  NOT NULL,
	[task_data_id] [bigint] NOT NULL,
	[log_time] datetime  NOT NULL DEFAULT SYSUTCDATETIME(),
	[log] [ntext] NOT NULL,
 CONSTRAINT [PK_gyomu_task_data_log] PRIMARY KEY NONCLUSTERED 
(
	id ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE CLUSTERED INDEX [CX_gyomu_task_data_log] ON [dbo].[gyomu_task_data_log]
(
	[task_data_id] ASC,
	[log_time] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO


CREATE TABLE [dbo].[gyomu_service_type_cdtbl](
	[id] [smallint] NOT NULL,
	[description] [varchar](100) NOT NULL,
	[assembly_name] [varchar](100) NULL,
	[class_name] [varchar](100) NULL,
 CONSTRAINT [PK_gyomu_service_type_cdtbl] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[gyomu_service_cdtbl](
	[id] [smallint] NOT NULL,
	[description] [varchar](100) NOT NULL,
	[service_type_id] [smallint] NOT NULL,
	[parameter] ntext NULL,
 CONSTRAINT [PK_gyomu_server_service_cdtbl] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO



CREATE TABLE [dbo].[gyomu_task_scheduler_config](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	service_id smallint NOT NULL,
	description varchar(200) NOT NULL,
	application_id smallint NOT NULL,
	task_id smallint NOT NULL,
	monitor_parameter ntext NOT NULL,
	next_trigger_time datetime NOT NULL,
	task_parameter ntext NULL,
	is_enabled bit NOT NULL,
 CONSTRAINT [PK_gyomu_task_scheduler_config] PRIMARY KEY CLUSTERED 
(
	id
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE INDEX IX_gyomu_task_scheduler_config on gyomu_task_scheduler_config
(
	description
)

CREATE INDEX IX_gyomu_task_scheduler_config2 on gyomu_task_scheduler_config
(service_id)
go

CREATE INDEX IX_gyomu_task_scheduler_config3 on gyomu_task_scheduler_config
(application_id,task_id)
go

CREATE INDEX IX_gyomu_task_scheduler_config4 on gyomu_task_scheduler_config
(is_enabled)
go
