CREATE EXTENSION IF NOT EXISTS timescaledb
    VERSION "1.3.2";
--CREATE DATABASE "TestLiveDataDb"
--    WITH 
--    OWNER = postgres
--    ENCODING = 'UTF8'
--    CONNECTION LIMIT = -1;


CREATE TABLE public.livedata
(
    "Timestamp" timestamp without time zone,
    "Value" double precision,
    "SiteId" uuid,
    "EquipmentId" uuid,
    "PointEntityId" uuid
)
WITH (
    OIDS = FALSE
);

ALTER TABLE public.livedata
    OWNER to postgres;

SELECT create_hypertable('LiveData', 'Timestamp');

CREATE INDEX ON livedata ("PointEntityId", "Timestamp" DESC);

CREATE INDEX ON livedata ("EquipmentId", "Timestamp" DESC);