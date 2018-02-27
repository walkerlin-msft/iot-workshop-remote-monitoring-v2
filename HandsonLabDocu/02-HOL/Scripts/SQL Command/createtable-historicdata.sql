CREATE SCHEMA [Prod]
CREATE TABLE [Prod].[HistoricData]
(
    [Id] INT IDENTITY (1, 1) NOT NULL,
    [DeviceId] NVARCHAR(128) NOT NULL,
    [MessageId] NVARCHAR(128) NOT NULL,
    [Temperature] FLOAT(10) NOT NULL,
    [Humidity] FLOAT(10) NOT NULL,
    [LocalTime] DATETIME DEFAULT (getdate()) NOT NULL,
	[EventEnqueuedUtcTime] DATETIME DEFAULT (getdate()) NOT NULL,
    [CreatedAt] DATETIME DEFAULT (getdate()) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
)