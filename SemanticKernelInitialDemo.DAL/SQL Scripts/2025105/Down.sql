USE [Cooking]

DELETE FROM [dbo].[CustomRecipes]

DROP TABLE CustomRecipes

DROP LOGIN [cookingAdmin]

USE [master]

alter database Cooking set single_user with rollback immediate

DROP DATABASE Cooking;