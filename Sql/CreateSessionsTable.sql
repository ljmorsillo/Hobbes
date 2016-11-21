USE [Hobbes]
GO

/****** Object:  Table [dbo].[Sessions]    Script Date: 11/20/2016 5:16:35 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Sessions](
	[Session_ID] [numeric](18, 0) IDENTITY(100000,1) NOT NULL,
	[Nonce] [nchar](10) NOT NULL,
	[Hash] [nchar](10) NULL,
	[User_ID] [numeric](10, 0) NULL,
	[Source] [nvarchar](50) NULL,
	[StartTime] [datetime] NULL,
	[IP_Address] [nchar](12) NULL,
	[Request] [nvarchar](1024) NULL
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'IP address of session source' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Sessions', @level2type=N'COLUMN',@level2name=N'IP_Address'
GO


