update SchemaColumn set DataType = 'Number'
where Name in ('InitDelay','Interval', 'MaxDevicesPerThread','MaxNumberThreads', 'MaxRetry', 'Port', 'ScanInterval', 'ThreadsPerNetwork', 'Timeout') and
        SchemaId in ('E73E8E7A-BD79-4813-ACFF-51C8E51500A3','8E5F3305-4BF9-4B14-B510-30713FC8A10B','41CFE979-4B56-40F4-8AAB-0292D2F96BB2','38CF1F48-0B28-4790-BAE7-D43C948FCE7A','21E52A57-1DCF-4C13-A0F8-C65B2F2B99D2');
