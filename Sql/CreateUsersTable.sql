USE [Hobbes]
GO

/****** Object:  Table [dbo].[Users]    Script Date: 11/20/2016 8:43:27 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Users](
	[User_ID] [numeric](10, 0) NOT NULL,
	[User_Name] [nvarchar](100) NULL
) ON [PRIMARY]

GO

