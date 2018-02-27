WITH HistoricData AS (
	SELECT
		Stream.[deviceId] AS [DeviceId],
		Stream.[msgId] AS [MessageId],
		Stream.[temperature] AS [Temperature],
		Stream.[humidity] AS [Humidity],
		Stream.[time] AS [LocalTime],
		Stream.[EventEnqueuedUtcTime] AS [EventEnqueuedUtcTime]
	FROM 
		[iothub] Stream
	WHERE GetMetadataPropertyValue(Stream, '[User].[SensorType]') = 'thermometer'
)

SELECT * INTO [blob] FROM HistoricData
SELECT * INTO [sqldb] FROM HistoricData