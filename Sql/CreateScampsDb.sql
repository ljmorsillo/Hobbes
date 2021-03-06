USE [master]
GO
/****** Object:  Database [scamps]    Script Date: 11/28/2016 12:53:59 PM ******/
CREATE DATABASE [scamps] ON  PRIMARY 
/*
( NAME = N'scampsweb', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQL\DATA\scampsweb.mdf' , SIZE = 3072KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'scampsweb_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQL\DATA\scampsweb_log.ldf' , SIZE = 10112KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
*/
GO
ALTER DATABASE [scamps] SET COMPATIBILITY_LEVEL = 100
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [scamps].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [scamps] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [scamps] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [scamps] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [scamps] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [scamps] SET ARITHABORT OFF 
GO
ALTER DATABASE [scamps] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [scamps] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [scamps] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [scamps] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [scamps] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [scamps] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [scamps] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [scamps] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [scamps] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [scamps] SET  DISABLE_BROKER 
GO
ALTER DATABASE [scamps] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [scamps] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [scamps] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [scamps] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [scamps] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [scamps] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [scamps] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [scamps] SET RECOVERY FULL 
GO
ALTER DATABASE [scamps] SET  MULTI_USER 
GO
ALTER DATABASE [scamps] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [scamps] SET DB_CHAINING OFF 
GO
USE [scamps]
GO
/****** Object:  Table [dbo].[encounters]    Script Date: 11/28/2016 12:53:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[encounters](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[patientid] [int] NULL,
	[eocid] [int] NULL,
	[type] [int] NULL,
	[apptid] [bigint] NULL,
	[accountingref] [nvarchar](24) NULL,
	[externalref] [nvarchar](16) NULL,
	[startdate] [datetime] NULL,
	[enddate] [datetime] NULL,
	[locationid] [int] NULL,
	[providerid] [int] NULL,
	[userid] [int] NULL,
	[formname] [nvarchar](32) NULL,
	[major] [int] NULL,
	[minor] [int] NULL,
	[status] [int] NULL,
	[flag] [int] NULL,
	[source] [int] NULL,
	[created] [datetime] NULL,
	[edited] [datetime] NULL,
	[deleted] [bit] NULL,
 CONSTRAINT [PK_encounters] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[environment]    Script Date: 11/28/2016 12:53:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[environment](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](32) NULL,
	[value] [nvarchar](128) NULL CONSTRAINT [DF_environment_value]  DEFAULT (''),
	[type] [int] NULL CONSTRAINT [DF_environment_type]  DEFAULT ((0)),
	[created] [datetime] NULL CONSTRAINT [DF_envoronment_created]  DEFAULT (getdate()),
	[edited] [datetime] NULL,
	[deleted] [bit] NULL CONSTRAINT [DF_environment_deleted]  DEFAULT ((0)),
 CONSTRAINT [PK_environment] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[eoc]    Script Date: 11/28/2016 12:53:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[eoc](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[patientid] [int] NULL,
	[scamp] [nvarchar](48) NULL,
	[initialdate] [datetime] NULL,
	[status] [int] NULL,
	[created] [datetime] NULL,
	[edited] [datetime] NULL,
	[deleted] [bit] NULL,
 CONSTRAINT [PK_eoc] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[forms]    Script Date: 11/28/2016 12:53:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[forms](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[scamp] [nvarchar](16) NULL,
	[name] [nvarchar](32) NULL,
	[title] [nvarchar](64) NULL,
	[major] [int] NULL,
	[minor] [int] NULL,
	[iteration] [nvarchar](1024) NULL,
	[precedent] [int] NULL,
	[encountertype] [int] NULL,
	[created] [datetime] NULL,
	[edited] [datetime] NULL,
	[deleted] [bit] NULL,
 CONSTRAINT [PK_forms] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[legend]    Script Date: 11/28/2016 12:53:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[legend](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[title] [varchar](32) NULL,
	[key1] [nvarchar](32) NULL,
	[key2] [nvarchar](32) NULL,
	[created] [datetime] NULL,
	[edited] [datetime] NULL,
	[deleted] [bit] NULL,
 CONSTRAINT [PK_legand] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[listitems]    Script Date: 11/28/2016 12:53:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[listitems](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[listid] [int] NULL,
	[name] [nvarchar](32) NULL,
	[value] [nvarchar](128) NULL,
	[type] [int] NULL,
	[sort] [int] NULL,
	[created] [datetime] NULL,
	[edited] [datetime] NULL,
	[deleted] [bit] NULL,
 CONSTRAINT [PK_lists] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[lists]    Script Date: 11/28/2016 12:53:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[lists](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](32) NULL,
	[category] [nvarchar](32) NULL,
	[flag] [int] NULL,
	[local] [bit] NULL,
	[deprecated] [bit] NULL,
	[created] [datetime] NULL,
	[edited] [datetime] NULL,
	[deleted] [bit] NULL,
 CONSTRAINT [PK_lists_1] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[locations]    Script Date: 11/28/2016 12:53:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[locations](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](128) NULL,
	[types] [int] NULL,
	[created] [datetime] NULL,
	[edited] [datetime] NULL,
	[deleted] [bit] NULL,
 CONSTRAINT [PK_locations] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[log]    Script Date: 11/28/2016 12:53:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[log](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[logname] [nvarchar](32) NULL CONSTRAINT [DF_log_logname]  DEFAULT (''),
	[zone] [nvarchar](32) NULL CONSTRAINT [DF_log_zone]  DEFAULT (''),
	[type] [nvarchar](32) NULL CONSTRAINT [DF_log_type]  DEFAULT (''),
	[message] [nvarchar](128) NULL CONSTRAINT [DF_log_message]  DEFAULT (''),
	[data] [nvarchar](max) NULL CONSTRAINT [DF_log_data]  DEFAULT (''),
	[severity] [int] NULL CONSTRAINT [DF_log_severity]  DEFAULT ((0)),
	[userid] [int] NULL CONSTRAINT [DF_log_userid]  DEFAULT ((0)),
	[created] [datetime] NULL,
	[deleted] [bit] NULL CONSTRAINT [DF_log_deleted]  DEFAULT ((0))
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[map]    Script Date: 11/28/2016 12:53:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[map](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[map] [int] NULL,
	[key1] [int] NULL,
	[key2] [int] NULL,
 CONSTRAINT [PK_map] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[measurements]    Script Date: 11/28/2016 12:53:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[measurements](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[encounterid] [int] NULL,
	[element] [nvarchar](32) NULL,
	[value] [nvarchar](255) NULL,
	[sequence] [int] NULL,
	[instance] [int] NOT NULL,
	[source] [int] NULL,
	[flag] [int] NOT NULL,
	[created] [datetime] NULL,
	[edited] [datetime] NULL,
	[deleted] [bit] NULL,
 CONSTRAINT [PK_measurements] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[notes]    Script Date: 11/28/2016 12:53:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[notes](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[foreignid] [int] NULL,
	[entity] [nvarchar](32) NULL,
	[userid] [bigint] NULL,
	[context] [nvarchar](32) NULL,
	[note] [nvarchar](max) NULL,
	[flag] [int] NULL,
	[created] [datetime] NULL,
	[edited] [datetime] NULL,
	[deleted] [bit] NULL,
 CONSTRAINT [PK_notes] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[patients]    Script Date: 11/28/2016 12:53:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[patients](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[referencecode] [nvarchar](24) NULL,
	[firstname] [nvarchar](32) NULL,
	[lastname] [nvarchar](32) NULL,
	[dob] [date] NULL,
	[gender] [nvarchar](1) NULL,
	[status] [int] NULL CONSTRAINT [DF_patients_status]  DEFAULT ((0)),
	[source] [int] NULL CONSTRAINT [DF_patients_source]  DEFAULT ((0)),
	[created] [datetime] NULL CONSTRAINT [DF_patients_created]  DEFAULT (getdate()),
	[edited] [datetime] NULL,
	[deleted] [bit] NULL CONSTRAINT [DF_patients_deleted]  DEFAULT ((0)),
 CONSTRAINT [PK_patients] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[providers]    Script Date: 11/28/2016 12:53:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[providers](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[referenceid] [nvarchar](32) NULL,
	[name] [nvarchar](80) NULL,
	[title] [nvarchar](64) NULL,
	[email] [nvarchar](128) NULL,
	[type] [nvarchar](32) NULL,
	[flag] [int] NULL,
	[created] [datetime] NULL,
	[edited] [datetime] NULL,
	[deleted] [bit] NULL,
 CONSTRAINT [PK_provider] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[scamps]    Script Date: 11/28/2016 12:53:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[scamps](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](16) NULL,
	[title] [nvarchar](64) NULL,
	[active] [bit] NULL,
	[created] [datetime] NULL,
	[edited] [datetime] NULL,
	[deleted] [bit] NULL,
 CONSTRAINT [PK_scamps] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[test]    Script Date: 11/28/2016 12:53:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[test](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[fkid] [int] NOT NULL,
	[name] [nvarchar](32) NULL CONSTRAINT [DF_test_name]  DEFAULT (''),
	[flag] [int] NULL CONSTRAINT [DF_test_flag]  DEFAULT ((0)),
	[description] [nvarchar](max) NULL CONSTRAINT [DF_test_description]  DEFAULT (''),
	[created] [datetime] NULL CONSTRAINT [DF_test_create]  DEFAULT (getdate()),
	[edited] [datetime] NULL,
	[deleted] [bit] NULL CONSTRAINT [DF_test_deleted]  DEFAULT ((0)),
 CONSTRAINT [PK_test] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[typedefs]    Script Date: 11/28/2016 12:53:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[typedefs](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[type] [nvarchar](16) NULL CONSTRAINT [DF_typedefs_type]  DEFAULT (''),
	[idx] [int] NULL CONSTRAINT [DF_typedefs_idx]  DEFAULT ((0)),
	[item] [nvarchar](32) NULL CONSTRAINT [DF_typedefs_item]  DEFAULT (''),
	[sort] [int] NULL CONSTRAINT [DF_typedefs_sort]  DEFAULT ((0)),
	[created] [datetime] NULL CONSTRAINT [DF_typedefs_created]  DEFAULT (getdate()),
	[edited] [datetime] NULL,
	[deleted] [bit] NULL CONSTRAINT [DF_typedefs_deleted]  DEFAULT ((0)),
 CONSTRAINT [PK_typedefs] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[users]    Script Date: 11/28/2016 12:53:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[users](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[username] [nvarchar](32) NULL,
	[firstname] [varchar](32) NULL,
	[lastname] [varchar](32) NULL,
	[email] [varchar](128) NULL,
	[password] [nvarchar](64) NULL,
	[authmode] [int] NULL,
	[permissions] [int] NULL,
	[hash] [nvarchar](32) NULL,
	[active] [bit] NULL,
	[activedate] [datetime] NULL,
	[expiredate] [datetime] NULL,
	[created] [datetime] NULL,
	[edited] [datetime] NULL,
	[deleted] [bit] NULL,
 CONSTRAINT [PK_users] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[encounters] ADD  CONSTRAINT [DF_encounters_apptid]  DEFAULT ((0)) FOR [apptid]
GO
ALTER TABLE [dbo].[encounters] ADD  CONSTRAINT [DF_encounters_accountingref]  DEFAULT ('') FOR [accountingref]
GO
ALTER TABLE [dbo].[encounters] ADD  CONSTRAINT [DF_encounters_flag]  DEFAULT ((0)) FOR [flag]
GO
ALTER TABLE [dbo].[encounters] ADD  CONSTRAINT [DF_encounter_created]  DEFAULT (getdate()) FOR [created]
GO
ALTER TABLE [dbo].[encounters] ADD  CONSTRAINT [DF_encounters_deleted]  DEFAULT ((0)) FOR [deleted]
GO
ALTER TABLE [dbo].[eoc] ADD  CONSTRAINT [DF_eoc_patientid]  DEFAULT ((0)) FOR [patientid]
GO
ALTER TABLE [dbo].[eoc] ADD  CONSTRAINT [DF_eoc_scamp]  DEFAULT ('') FOR [scamp]
GO
ALTER TABLE [dbo].[eoc] ADD  CONSTRAINT [DF_eoc_status]  DEFAULT ((0)) FOR [status]
GO
ALTER TABLE [dbo].[eoc] ADD  CONSTRAINT [DF_eoc_deleted]  DEFAULT ((0)) FOR [deleted]
GO
ALTER TABLE [dbo].[forms] ADD  CONSTRAINT [DF_forms_scamp]  DEFAULT ('') FOR [scamp]
GO
ALTER TABLE [dbo].[forms] ADD  CONSTRAINT [DF_forms_name]  DEFAULT ('') FOR [name]
GO
ALTER TABLE [dbo].[forms] ADD  CONSTRAINT [DF_forms_title]  DEFAULT ('') FOR [title]
GO
ALTER TABLE [dbo].[forms] ADD  CONSTRAINT [DF_forms_iteration]  DEFAULT ('') FOR [iteration]
GO
ALTER TABLE [dbo].[forms] ADD  CONSTRAINT [DF_forms_precedent]  DEFAULT ((0)) FOR [precedent]
GO
ALTER TABLE [dbo].[forms] ADD  CONSTRAINT [DF_forms_encountertype]  DEFAULT ((0)) FOR [encountertype]
GO
ALTER TABLE [dbo].[forms] ADD  CONSTRAINT [DF_forms_created]  DEFAULT (getdate()) FOR [created]
GO
ALTER TABLE [dbo].[forms] ADD  CONSTRAINT [DF_forms_deleted]  DEFAULT ((0)) FOR [deleted]
GO
ALTER TABLE [dbo].[legend] ADD  CONSTRAINT [DF_legand_title]  DEFAULT ('') FOR [title]
GO
ALTER TABLE [dbo].[legend] ADD  CONSTRAINT [DF_legand_key1]  DEFAULT ('') FOR [key1]
GO
ALTER TABLE [dbo].[legend] ADD  CONSTRAINT [DF_legand_key2]  DEFAULT ('') FOR [key2]
GO
ALTER TABLE [dbo].[legend] ADD  CONSTRAINT [DF_legend_deleted]  DEFAULT ((0)) FOR [deleted]
GO
ALTER TABLE [dbo].[listitems] ADD  CONSTRAINT [DF_listitems_listid]  DEFAULT ((0)) FOR [listid]
GO
ALTER TABLE [dbo].[listitems] ADD  CONSTRAINT [DF_listitems_value]  DEFAULT ('') FOR [value]
GO
ALTER TABLE [dbo].[listitems] ADD  CONSTRAINT [DF_listitems_type]  DEFAULT ((0)) FOR [type]
GO
ALTER TABLE [dbo].[listitems] ADD  CONSTRAINT [DF_lists_sort]  DEFAULT ((0)) FOR [sort]
GO
ALTER TABLE [dbo].[listitems] ADD  CONSTRAINT [DF_listitems_created]  DEFAULT (getdate()) FOR [created]
GO
ALTER TABLE [dbo].[listitems] ADD  CONSTRAINT [DF_listitems_deleted]  DEFAULT ((0)) FOR [deleted]
GO
ALTER TABLE [dbo].[lists] ADD  CONSTRAINT [DF_lists_name]  DEFAULT ('') FOR [name]
GO
ALTER TABLE [dbo].[lists] ADD  CONSTRAINT [DF_lists_category]  DEFAULT ('') FOR [category]
GO
ALTER TABLE [dbo].[lists] ADD  CONSTRAINT [DF_lists_flag]  DEFAULT ((0)) FOR [flag]
GO
ALTER TABLE [dbo].[lists] ADD  CONSTRAINT [DF_lists_local]  DEFAULT ((0)) FOR [local]
GO
ALTER TABLE [dbo].[lists] ADD  CONSTRAINT [DF_lists_depricated]  DEFAULT ((0)) FOR [deprecated]
GO
ALTER TABLE [dbo].[lists] ADD  CONSTRAINT [DF_lists_deleted]  DEFAULT ((0)) FOR [deleted]
GO
ALTER TABLE [dbo].[locations] ADD  CONSTRAINT [DF_locations_name]  DEFAULT ('') FOR [name]
GO
ALTER TABLE [dbo].[locations] ADD  CONSTRAINT [DF_locations_types]  DEFAULT ((0)) FOR [types]
GO
ALTER TABLE [dbo].[locations] ADD  CONSTRAINT [DF_locations_created]  DEFAULT (getdate()) FOR [created]
GO
ALTER TABLE [dbo].[locations] ADD  CONSTRAINT [DF_locations_deleted]  DEFAULT ((0)) FOR [deleted]
GO
ALTER TABLE [dbo].[map] ADD  CONSTRAINT [DF_map_map]  DEFAULT ((0)) FOR [map]
GO
ALTER TABLE [dbo].[measurements] ADD  CONSTRAINT [DF_measurements_sequence]  DEFAULT ((0)) FOR [sequence]
GO
ALTER TABLE [dbo].[measurements] ADD  CONSTRAINT [DF_measurements_instance]  DEFAULT ((0)) FOR [instance]
GO
ALTER TABLE [dbo].[measurements] ADD  CONSTRAINT [DF_measurements_source]  DEFAULT ('') FOR [source]
GO
ALTER TABLE [dbo].[measurements] ADD  CONSTRAINT [DF_measurements_flag]  DEFAULT ((0)) FOR [flag]
GO
ALTER TABLE [dbo].[measurements] ADD  CONSTRAINT [DF_measurements_created]  DEFAULT (getdate()) FOR [created]
GO
ALTER TABLE [dbo].[measurements] ADD  CONSTRAINT [DF_measurements_deleted]  DEFAULT ((0)) FOR [deleted]
GO
ALTER TABLE [dbo].[notes] ADD  CONSTRAINT [DF_notes_context]  DEFAULT ('') FOR [context]
GO
ALTER TABLE [dbo].[notes] ADD  CONSTRAINT [DF_notes_flag]  DEFAULT ((0)) FOR [flag]
GO
ALTER TABLE [dbo].[notes] ADD  CONSTRAINT [DF_notes_created]  DEFAULT (getdate()) FOR [created]
GO
ALTER TABLE [dbo].[notes] ADD  CONSTRAINT [DF_notes_deleted]  DEFAULT ((0)) FOR [deleted]
GO
ALTER TABLE [dbo].[providers] ADD  CONSTRAINT [DF_provider_email]  DEFAULT ('') FOR [email]
GO
ALTER TABLE [dbo].[providers] ADD  CONSTRAINT [DF_provider_flag]  DEFAULT ((0)) FOR [flag]
GO
ALTER TABLE [dbo].[providers] ADD  CONSTRAINT [DF_provider_created]  DEFAULT (getdate()) FOR [created]
GO
ALTER TABLE [dbo].[providers] ADD  CONSTRAINT [DF_provider_deleted]  DEFAULT ((0)) FOR [deleted]
GO
ALTER TABLE [dbo].[scamps] ADD  CONSTRAINT [DF_scamps_name]  DEFAULT ('') FOR [name]
GO
ALTER TABLE [dbo].[scamps] ADD  CONSTRAINT [DF_scamps_title]  DEFAULT ('') FOR [title]
GO
ALTER TABLE [dbo].[scamps] ADD  CONSTRAINT [DF_scamps_active]  DEFAULT ((0)) FOR [active]
GO
ALTER TABLE [dbo].[scamps] ADD  CONSTRAINT [DF_scamps_created]  DEFAULT (getdate()) FOR [created]
GO
ALTER TABLE [dbo].[scamps] ADD  CONSTRAINT [DF_scamps_deleted]  DEFAULT ((0)) FOR [deleted]
GO
ALTER TABLE [dbo].[users] ADD  CONSTRAINT [DF_users_firstname]  DEFAULT ('') FOR [firstname]
GO
ALTER TABLE [dbo].[users] ADD  CONSTRAINT [DF_users_lastname]  DEFAULT ('') FOR [lastname]
GO
ALTER TABLE [dbo].[users] ADD  CONSTRAINT [DF_users_email]  DEFAULT ('') FOR [email]
GO
ALTER TABLE [dbo].[users] ADD  CONSTRAINT [DF_users_authmode]  DEFAULT ((0)) FOR [authmode]
GO
ALTER TABLE [dbo].[users] ADD  CONSTRAINT [DF_users_permissions]  DEFAULT ((0)) FOR [permissions]
GO
ALTER TABLE [dbo].[users] ADD  CONSTRAINT [DF_users_created]  DEFAULT (getdate()) FOR [created]
GO
ALTER TABLE [dbo].[users] ADD  CONSTRAINT [DF_users_deleted]  DEFAULT ((0)) FOR [deleted]
GO
ALTER TABLE [dbo].[encounters]  WITH CHECK ADD  CONSTRAINT [FK_encounters_eoc] FOREIGN KEY([eocid])
REFERENCES [dbo].[eoc] ([id])
GO
ALTER TABLE [dbo].[encounters] CHECK CONSTRAINT [FK_encounters_eoc]
GO
ALTER TABLE [dbo].[encounters]  WITH CHECK ADD  CONSTRAINT [FK_encounters_locations] FOREIGN KEY([locationid])
REFERENCES [dbo].[locations] ([id])
GO
ALTER TABLE [dbo].[encounters] CHECK CONSTRAINT [FK_encounters_locations]
GO
ALTER TABLE [dbo].[encounters]  WITH CHECK ADD  CONSTRAINT [FK_encounters_patients] FOREIGN KEY([patientid])
REFERENCES [dbo].[patients] ([id])
GO
ALTER TABLE [dbo].[encounters] CHECK CONSTRAINT [FK_encounters_patients]
GO
ALTER TABLE [dbo].[encounters]  WITH CHECK ADD  CONSTRAINT [FK_encounters_provider] FOREIGN KEY([providerid])
REFERENCES [dbo].[providers] ([id])
GO
ALTER TABLE [dbo].[encounters] CHECK CONSTRAINT [FK_encounters_provider]
GO
ALTER TABLE [dbo].[eoc]  WITH CHECK ADD  CONSTRAINT [FK_eoc_patients] FOREIGN KEY([patientid])
REFERENCES [dbo].[patients] ([id])
GO
ALTER TABLE [dbo].[eoc] CHECK CONSTRAINT [FK_eoc_patients]
GO
ALTER TABLE [dbo].[listitems]  WITH CHECK ADD  CONSTRAINT [FK_listitems_lists] FOREIGN KEY([listid])
REFERENCES [dbo].[lists] ([id])
GO
ALTER TABLE [dbo].[listitems] CHECK CONSTRAINT [FK_listitems_lists]
GO
ALTER TABLE [dbo].[measurements]  WITH CHECK ADD  CONSTRAINT [FK_measurements_encounters] FOREIGN KEY([encounterid])
REFERENCES [dbo].[encounters] ([id])
GO
ALTER TABLE [dbo].[measurements] CHECK CONSTRAINT [FK_measurements_encounters]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Foreign key to patient table' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'encounters', @level2type=N'COLUMN',@level2name=N'patientid'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK to episode of care table.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'encounters', @level2type=N'COLUMN',@level2name=N'eocid'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=Outpatient, 1=Inpatient,2 = Subunits 4 = Event + whatever else we might need...' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'encounters', @level2type=N'COLUMN',@level2name=N'type'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Hospital generated appointment ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'encounters', @level2type=N'COLUMN',@level2name=N'apptid'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Hospital generated value - reference to a hospital record, for example appointment_id' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'encounters', @level2type=N'COLUMN',@level2name=N'externalref'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Beginning date of the event, encounter, visit.  Admit date, test date, etc.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'encounters', @level2type=N'COLUMN',@level2name=N'startdate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'End date of the event or encounter, discharge date if appropriate' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'encounters', @level2type=N'COLUMN',@level2name=N'enddate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK to location table ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'encounters', @level2type=N'COLUMN',@level2name=N'locationid'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK to provider table id' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'encounters', @level2type=N'COLUMN',@level2name=N'providerid'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Primary user "owner" of the encounter, the primary DC' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'encounters', @level2type=N'COLUMN',@level2name=N'userid'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK to status look up table' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'encounters', @level2type=N'COLUMN',@level2name=N'status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Generated (0) or imported >0' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'encounters', @level2type=N'COLUMN',@level2name=N'source'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Is this record deleted?' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'environment', @level2type=N'COLUMN',@level2name=N'deleted'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'foreign key - patient.id' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'eoc', @level2type=N'COLUMN',@level2name=N'patientid'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'date the episode of care began' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'eoc', @level2type=N'COLUMN',@level2name=N'initialdate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'bitwise status flags' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'eoc', @level2type=N'COLUMN',@level2name=N'status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Is this record deleted?' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'eoc', @level2type=N'COLUMN',@level2name=N'deleted'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Unique name of the form, no spaces, no special characters, lower case' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'forms', @level2type=N'COLUMN',@level2name=N'name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Descriptive title of the form' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'forms', @level2type=N'COLUMN',@level2name=N'title'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Describe the changes to this form, generally, that represent this iteration' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'forms', @level2type=N'COLUMN',@level2name=N'iteration'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The order this form appears in a list of forms for this SCAMP.  Typically the SDF number.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'forms', @level2type=N'COLUMN',@level2name=N'precedent'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Unique interger value that identifies this form in the encounter table.  This supports schema V1.08 and below only' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'forms', @level2type=N'COLUMN',@level2name=N'encountertype'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'date and time this record was created' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'forms', @level2type=N'COLUMN',@level2name=N'created'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date and time this record was last modified' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'forms', @level2type=N'COLUMN',@level2name=N'edited'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'is this record deleted?' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'forms', @level2type=N'COLUMN',@level2name=N'deleted'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK to list table' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'listitems', @level2type=N'COLUMN',@level2name=N'listid'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Title or selected text' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'listitems', @level2type=N'COLUMN',@level2name=N'name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'bitwise sub select flags' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'listitems', @level2type=N'COLUMN',@level2name=N'type'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'sort order' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'listitems', @level2type=N'COLUMN',@level2name=N'sort'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'String category name for sub-selecting lists by type in the admin area of ScampsWeb.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'lists', @level2type=N'COLUMN',@level2name=N'category'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'bitwise integer flags for subselection and organization' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'lists', @level2type=N'COLUMN',@level2name=N'flag'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Is this a locally managed list?  If so, it can be managed by ePortal' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'lists', @level2type=N'COLUMN',@level2name=N'local'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Is this list no longer actively used for new scamp forms and is not used in authoring.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'lists', @level2type=N'COLUMN',@level2name=N'deprecated'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'bitwise membership flag to types.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'locations', @level2type=N'COLUMN',@level2name=N'types'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The name of the log (so that multiple logs can be decretely handled)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'log', @level2type=N'COLUMN',@level2name=N'logname'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The application, method, page, or other condition that informs the log message' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'log', @level2type=N'COLUMN',@level2name=N'zone'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Type of entry' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'log', @level2type=N'COLUMN',@level2name=N'type'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Any additional data stored with the log entry, generally in XML' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'log', @level2type=N'COLUMN',@level2name=N'data'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK ID from encounter table' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'measurements', @level2type=N'COLUMN',@level2name=N'encounterid'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'element code/name for this measurement' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'measurements', @level2type=N'COLUMN',@level2name=N'element'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'One of multiple answers for a single instance of this question.  Multiselect.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'measurements', @level2type=N'COLUMN',@level2name=N'sequence'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Individual instance where the question was asked.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'measurements', @level2type=N'COLUMN',@level2name=N'instance'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Where did this measurement come from?' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'measurements', @level2type=N'COLUMN',@level2name=N'source'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'bitwise status flag, bit 1 = Was visible {empty}' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'measurements', @level2type=N'COLUMN',@level2name=N'flag'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ID of the foreign table row' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'notes', @level2type=N'COLUMN',@level2name=N'foreignid'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Table or application data construct' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'notes', @level2type=N'COLUMN',@level2name=N'entity'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'contextural usage.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'notes', @level2type=N'COLUMN',@level2name=N'context'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'bitwise flags for various status' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'patients', @level2type=N'COLUMN',@level2name=N'status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'How was this patient entered?  0=Manual data entry, all other values via API or automation/integration' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'patients', @level2type=N'COLUMN',@level2name=N'source'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'date/time created' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'patients', @level2type=N'COLUMN',@level2name=N'created'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'date/time last edited' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'patients', @level2type=N'COLUMN',@level2name=N'edited'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Is this record deleted?' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'patients', @level2type=N'COLUMN',@level2name=N'deleted'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Hospital or external reference id' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'providers', @level2type=N'COLUMN',@level2name=N'referenceid'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Unique SCAMP name, lower case, no spaces or special characters' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'scamps', @level2type=N'COLUMN',@level2name=N'name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Descriptive title of the SCAMP' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'scamps', @level2type=N'COLUMN',@level2name=N'title'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'date and time this record was created' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'scamps', @level2type=N'COLUMN',@level2name=N'created'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date and time this record was last modified' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'scamps', @level2type=N'COLUMN',@level2name=N'edited'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'is this record deleted?' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'scamps', @level2type=N'COLUMN',@level2name=N'deleted'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Type name.  A type defination is all type items that share the same name' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'typedefs', @level2type=N'COLUMN',@level2name=N'type'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Index.  Integer value of the item' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'typedefs', @level2type=N'COLUMN',@level2name=N'idx'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'item name' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'typedefs', @level2type=N'COLUMN',@level2name=N'item'
GO
EXEC sys.sp_addextendedproperty @name=N'description', @value=N'Application Table' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'users'
GO
USE [master]
GO
ALTER DATABASE [scamps] SET  READ_WRITE 
GO
