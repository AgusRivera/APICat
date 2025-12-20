
/* Crear base de datos APICat si no existe */
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'APICat')
BEGIN
    CREATE DATABASE [APICat];
END
GO

/* Usar la base APICat */
USE [APICat];
GO

/* Asegurar que el schema dbo existe (en SQL Server ya viene por defecto) */
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'dbo')
BEGIN
    EXEC('CREATE SCHEMA [dbo]');
END
GO

/* Crear tabla Breed */
IF OBJECT_ID(N'[dbo].[Breeds]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Breeds] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(200) NOT NULL,
        [Origin] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(200) NOT NULL,
        [Temperament] NVARCHAR(300) NOT NULL,
        [Wikipedia_url] NVARCHAR(100) NOT NULL,
        CONSTRAINT [PK_Breed] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO


