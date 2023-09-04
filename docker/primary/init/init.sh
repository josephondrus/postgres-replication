#!/bin/sh
# Exit on any non-successful exit status.
set -e

# Create a directory where the WAL will be archived to.
mkdir $PGDATA/pg_wal/archive_dir

# Update the configuration to support streaming replication.
psql -v ON_ERROR_STOP=1 -U "$POSTGRES_USER" <<-EOSQL
	CREATE USER $POSTGRES_REPLICATION_USER REPLICATION LOGIN CONNECTION LIMIT 100 ENCRYPTED PASSWORD '$POSTGRES_REPLICATION_PASSWORD';
	ALTER SYSTEM SET wal_level TO 'replica';
	ALTER SYSTEM SET max_wal_senders TO '3';
	ALTER SYSTEM SET archive_mode TO 'ON';
	ALTER SYSTEM SET listen_addresses TO '*';
	ALTER SYSTEM SET hot_standby TO 'ON';
	ALTER SYSTEM SET wal_keep_size TO '20';
	ALTER SYSTEM SET archive_command TO 'test ! -f /var/lib/postgresql/data/pg_wal/archive_dir/%f && cp %p /var/lib/postgresql/data/pg_wal/archive_dir/%f';
	SELECT pg_reload_conf();
EOSQL

# Restart the cluster to read all configuration changes in.
pg_ctl -D $PGDATA restart

# Update the allowed authentcation list for the Postgres replication user. 
echo "host replication $POSTGRES_REPLICATION_USER 0.0.0.0/0 md5" >> "$PGDATA/pg_hba.conf"

# Reload the configuration to read in the authentication changes.
pg_ctl -D $PGDATA reload

# Let Docker run the "command" from the official Postgres image.
exec "$@"