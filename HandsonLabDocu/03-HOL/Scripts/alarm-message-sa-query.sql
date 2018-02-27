WITH StreamAvgData AS 
(
    SELECT
        Stream.[deviceId],
		'thermometer' AS [SensorType],
        AVG(CAST(Stream.[temperature] AS Float)) AS AvgTemperature,
        System.TimeStamp AS CreatedAt
    FROM [iothub] Stream TIMESTAMP BY time
	WHERE GetMetadataPropertyValue(Stream, '[User].[SensorType]') = 'thermometer' 
    GROUP BY
        Stream.[deviceId],
    TumblingWindow(second, 30)
),
AlarmData AS
(
    SELECT
        TempAvgData.deviceId AS IoTHubDeviceID,
        'TempAlert' as [AlarmType],
        TempAvgData.AvgTemperature as [Reading],
        Ref.[TemperatureThreshold] as [Threshold],
        TempAvgData.CreatedAt as [CreatedAt]
    FROM [StreamAvgData] TempAvgData
    JOIN [devicerules] Ref 
    ON 
        TempAvgData.[SensorType] = Ref.[SensorType]
    WHERE Ref.[TemperatureThreshold] IS NOT null AND TempAvgData.AvgTemperature > Ref.[TemperatureThreshold]
)

SELECT * INTO [alarmsb] FROM AlarmData
