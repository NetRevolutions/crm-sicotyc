-- B.5.7: ejecutar en SSMS sobre el servidor SQL Server de integracion.
-- Solo lectura. Usar una cuenta con acceso existente a los diagnosticos del servidor.
-- No crea sesiones, cambia permisos ni modifica datos.
-- Abrir la columna DeadlockXml y compartir el XML del evento de la prueba.
-- system_health captura xml_deadlock_report por defecto:
-- https://learn.microsoft.com/en-us/sql/relational-databases/sql-server-deadlocks-guide

-- 1. Eventos recientes que todavia permanecen en memoria.
SELECT TOP (10)
    event_node.value('@timestamp', 'datetime2') AS TimestampUtc,
    event_node.query('(data[@name="xml_report"]/value/deadlock)[1]') AS DeadlockXml
FROM
(
    SELECT CAST(target.target_data AS xml) AS TargetXml
    FROM sys.dm_xe_session_targets AS target
    INNER JOIN sys.dm_xe_sessions AS session
        ON session.address = target.event_session_address
    WHERE session.name = N'system_health'
      AND target.target_name = N'ring_buffer'
) AS buffer
CROSS APPLY buffer.TargetXml.nodes(
    '/RingBufferTarget/event[@name="xml_deadlock_report"]'
) AS events(event_node)
WHERE event_node.exist('data/value/deadlock/process-list/process[
    contains(@clientapp, "Sicotyc.Stress.")]') = 1
ORDER BY TimestampUtc DESC;

-- 2. Archivos retenidos: alternativa si el buffer ya no contiene el evento.
DECLARE @CurrentFile nvarchar(4000);
SELECT @CurrentFile = CAST(target.target_data AS xml).value(
    '(EventFileTarget/File/@name)[1]', 'nvarchar(4000)')
FROM sys.dm_xe_session_targets AS target
INNER JOIN sys.dm_xe_sessions AS session
    ON session.address = target.event_session_address
WHERE session.name = N'system_health'
  AND target.target_name = N'event_file';

IF @CurrentFile IS NULL
BEGIN
    SELECT N'system_health no tiene un destino event_file disponible.' AS Diagnostico;
END
ELSE
BEGIN
    -- Conservar el directorio informado por SQL Server (Windows o Linux).
    DECLARE @SeparatorPosition int = CHARINDEX(N'\', REVERSE(@CurrentFile));
    IF @SeparatorPosition = 0
        SET @SeparatorPosition = CHARINDEX(N'/', REVERSE(@CurrentFile));
    DECLARE @FilePattern nvarchar(4000) =
        CASE WHEN @SeparatorPosition > 0
             THEN LEFT(@CurrentFile, LEN(@CurrentFile) - @SeparatorPosition + 1)
             ELSE N'' END + N'system_health*.xel';

    ;WITH Events AS
    (
        SELECT CAST(event_data AS xml) AS EventXml
        FROM sys.fn_xe_file_target_read_file(@FilePattern, NULL, NULL, NULL)
        WHERE object_name = N'xml_deadlock_report'
    )
    SELECT TOP (10)
        EventXml.value('(event/@timestamp)[1]', 'datetime2') AS TimestampUtc,
        EventXml.query('(event/data[@name="xml_report"]/value/deadlock)[1]') AS DeadlockXml
    FROM Events
    WHERE EventXml.exist('event/data/value/deadlock/process-list/process[
        contains(@clientapp, "Sicotyc.Stress.")]') = 1
    ORDER BY TimestampUtc DESC;
END;
