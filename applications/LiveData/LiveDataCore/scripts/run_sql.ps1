docker stop $(docker ps -a -q)
docker run -d --rm --name timescaledb -p 5432:5432 -e POSTGRES_PASSWORD=Password01! timescale/timescaledb:latest-pg11
