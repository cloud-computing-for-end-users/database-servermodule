USE [CCFEU_DB]
GO

CREATE TABLE [dbo].[User](
	[UserID] [int] IDENTITY(1,1) NOT NULL,
	[Email] [nvarchar](256) NOT NULL UNIQUE,
	[Password] [nvarchar](256) NOT NULL,
	CONSTRAINT [PK_User] PRIMARY KEY ( [UserID] ASC )
)	
GO


