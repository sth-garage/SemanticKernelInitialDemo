CREATE DATABASE [Cooking]
GO

USE [Cooking]
GO

/****** Object:  Table [dbo].[CustomRecipes]    Script Date: 10/4/2025 11:11:43 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CustomRecipes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[FilePath] [varchar](1000) NOT NULL,
 CONSTRAINT [PK_CustomRecipes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CustomRecipes] ADD  CONSTRAINT [DF_CustomRecipes_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO

-- DATA

INSERT INTO [dbo].[CustomRecipes] ([FilePath]) VALUES ('Test')
GO

-- Login

USE [master]
GO

/* For security reasons the login is created disabled and with a random password. */
/****** Object:  Login [cookingAdmin]    Script Date: 10/4/2025 11:28:23 PM ******/
CREATE LOGIN [cookingAdmin] WITH PASSWORD=N'Testing777!!', DEFAULT_DATABASE=[Cooking], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
GO

ALTER SERVER ROLE [sysadmin] ADD MEMBER [cookingAdmin]
GO
