DROP INDEX if exists  [CX_status_info] ON [dbo].[status_info] WITH ( ONLINE = OFF ) 
GO

DROP TABLE if exists status_info;

DROP TABLE if exists status_handler;

DROP TABLE if exists apps_info_cdtbl;

DROP TABLE if exists status_type_cdtbl;

DROP TABLE if exists market_holiday;

DROP TABLE IF EXISTS milestone_daily;
GO

DROP TABLE IF EXISTS variable_parameter;
GO

CREATE TABLE [dbo].[apps_info_cdtbl](
	[application_id] [smallint] NOT NULL,
	[description] [varchar](50) NULL,
	[mail_from_address] [varchar](200) NULL,
	[mail_from_name] [varchar](200) NULL,
 CONSTRAINT [PK_apps_info_cdtbl] PRIMARY KEY CLUSTERED 
(
	[application_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[status_type_cdtbl](
	[status_type] [smallint] NOT NULL,
	[description] [varchar](15) NULL,
 CONSTRAINT [PK_status_type_cdtbl] PRIMARY KEY CLUSTERED 
(
	[status_type] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

INSERT INTO [status_type_cdtbl] VALUES (0,'INFO');
INSERT INTO [status_type_cdtbl] VALUES (1,'WARNING');
INSERT INTO [status_type_cdtbl] VALUES (2,'ERROR_BUSINESS');
INSERT INTO [status_type_cdtbl] VALUES (8,'ERROR_DEVEL');

CREATE TABLE [dbo].[status_handler](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[application_id] [smallint] NOT NULL,
	[region] [varchar](3) NULL,
	[status_type] [smallint] NULL,
	[recipient_address] [varchar](200) NULL,
	[recipient_type] [varchar](3) NULL,
 CONSTRAINT [PK_status_handler_cdtbl] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[status_info](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[application_id] [smallint] NOT NULL,
	[entry_date] [datetime] NOT NULL DEFAULT SYSUTCDATETIME(),
	[entry_author] [varchar](30) NOT NULL,
	[status_type] [smallint] NULL,
  error_id smallint NOT NULL,
	[instance_id] [int] NULL,
	[hostname] [varchar](50) NULL,
	[summary] [nvarchar](400) NULL,
	[description] [nvarchar](1000) NULL,
	[developer_info] ntext NULL,
 CONSTRAINT [PK_status_info] PRIMARY KEY NONCLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


/****** Object:  Index [CX_status_info]    Script Date: 11/6/2017 4:23:19 PM ******/
CREATE CLUSTERED INDEX [CX_status_info] ON [dbo].[status_info]
(
	[entry_date] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO




CREATE TABLE [dbo].[market_holiday](
	[market] [varchar](10) NOT NULL,
	year smallint NOT NULL,
	holiday char(8) NOT NULL,
 CONSTRAINT [PK_market_holiday] PRIMARY KEY CLUSTERED 
(
	[market] ASC,
	[holiday] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE INDEX IX_market_holiday ON market_holiday
(market ASC,year ASC)
GO

CREATE TABLE [dbo].[milestone_daily](
	[target_date] [varchar](8) NOT NULL,
	[milestone_id] varchar(200) NOT NULL,
	[update_time] [datetime] NOT NULL DEFAULT SYSUTCDATETIME(),
 CONSTRAINT [PK_milestone_daily] PRIMARY KEY CLUSTERED 
(
	[target_date] ASC,[milestone_id]
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE  INDEX IX_target_date ON milestone_daily
(
	milestone_id
)
GO

CREATE TABLE [dbo].[variable_parameter](
	variable_key varchar(20) NOT NULL,
	description varchar(200) NOT NULL,
 CONSTRAINT [PK_variable_parameter] PRIMARY KEY CLUSTERED 
(
	variable_key
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

INSERT INTO VARIABLE_PARAMETER VALUES('BBOM','Business Day of Beginning Of Month');
INSERT INTO VARIABLE_PARAMETER VALUES('BBOY','Business Day of Beginning Of Year');
INSERT INTO VARIABLE_PARAMETER VALUES('BOM','Beginning of Month');
INSERT INTO VARIABLE_PARAMETER VALUES('BOY','Beginning of Year');
INSERT INTO VARIABLE_PARAMETER VALUES('BEOM','Business Day of End Of Month');
INSERT INTO VARIABLE_PARAMETER VALUES('BEOY','Business Day of End Of Year');
INSERT INTO VARIABLE_PARAMETER VALUES('EOM','End of Month');
INSERT INTO VARIABLE_PARAMETER VALUES('EOY','End Day of Year');
INSERT INTO VARIABLE_PARAMETER VALUES('NEXTBBOM','Business Day of Next Beginning Of Month');
INSERT INTO VARIABLE_PARAMETER VALUES('NEXTBUS','Previous Business Day');
INSERT INTO VARIABLE_PARAMETER VALUES('NEXTDAY','Next Day');
INSERT INTO VARIABLE_PARAMETER VALUES('NEXTBEOM','Business Day of Next End Of Month');
INSERT INTO VARIABLE_PARAMETER VALUES('PARAMMASTER','From param_master');
INSERT INTO VARIABLE_PARAMETER VALUES('PREVBUS','Previous Business Day');
INSERT INTO VARIABLE_PARAMETER VALUES('PREVDAY','Previous Day');
INSERT INTO VARIABLE_PARAMETER VALUES('PREVBEOM',' Business Day of Previous End Of Month');
INSERT INTO VARIABLE_PARAMETER VALUES('TODAY','Today');
GO