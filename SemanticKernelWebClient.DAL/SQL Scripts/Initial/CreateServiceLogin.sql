USE [master]
GO

/* For security reasons the login is created disabled and with a random password. */
/****** Object:  Login [semanticKernelWebClientServiceLogin]    Script Date: 12/29/2025 4:00:08 PM ******/
CREATE LOGIN [semanticKernelWebClientServiceLogin] WITH PASSWORD=N'iZOvg4svC5qoTe+z4wSFVm+F6ZJSXjckVFB7IrxuylU=', DEFAULT_DATABASE=[SemanticKernelWebClient], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
GO

ALTER LOGIN [semanticKernelWebClientServiceLogin] DISABLE
GO

ALTER SERVER ROLE [sysadmin] ADD MEMBER [semanticKernelWebClientServiceLogin]
GO


