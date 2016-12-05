/** Recreate users table with longer hash and salt **/

USE [scamps]
GO

DROP TABLE [dbo].[users]
GO

/****** Object:  Table [dbo].[users]    Script Date: 12/5/2016 3:41:18 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[users](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[username] [nvarchar](32) NULL,
	[firstname] [varchar](32) NULL CONSTRAINT [DF_users_firstname]  DEFAULT (''),
	[lastname] [varchar](32) NULL CONSTRAINT [DF_users_lastname]  DEFAULT (''),
	[email] [varchar](128) NULL CONSTRAINT [DF_users_email]  DEFAULT (''),
	[password] [nvarchar](64) NULL,
	[authmode] [int] NULL CONSTRAINT [DF_users_authmode]  DEFAULT ((0)),
	[permissions] [int] NULL CONSTRAINT [DF_users_permissions]  DEFAULT ((0)),
	[hash] [nvarchar](64) NULL,
	[active] [bit] NULL,
	[activedate] [datetime] NULL,
	[expiredate] [datetime] NULL,
	[created] [datetime] NULL CONSTRAINT [DF_users_created]  DEFAULT (getdate()),
	[edited] [datetime] NULL,
	[deleted] [bit] NULL CONSTRAINT [DF_users_deleted]  DEFAULT ((0)),
	[salt] [nvarchar](64) NULL,
 CONSTRAINT [PK_users] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

EXEC sys.sp_addextendedproperty @name=N'description', @value=N'Application Table' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'users'
GO


