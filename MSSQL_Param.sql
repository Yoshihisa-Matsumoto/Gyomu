DROP TABLE [dbo].[param_master]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[param_master](
	[item_key] [varchar](50) NOT NULL,
	[item_value] ntext NOT NULL,
	[item_fromdate] [varchar](8) NOT NULL default ''
 CONSTRAINT [PK_param_master] PRIMARY KEY CLUSTERED 
(
	[item_key] ASC,[item_fromdate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO