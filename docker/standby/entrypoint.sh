#!/bin/sh
# Exit on any non-successful exit status.
set -e
if [ ! -s "$PGDATA/PG_VERSION" ]; then
	# Store the replication user and assoicated password in the location Postgres expects to find it
	# so that pg_basebackup will be able to copy the existing cluster.
	echo "*:*:*:$POSTGRES_REPLICATION_USER:$POSTGRES_REPLICATION_PASSWORD" > ~/.pgpass
	# Update the required permissions as per Postgres.
	chmod 0600 ~/.pgpass
	
	# Restore this cluster from the primary.
	pg_basebackup -h postgres_primary -D ${PGDATA} -U ${POSTGRES_REPLICATION_USER} -vP -W -R
	
	# Update permissions as required by Postgres - similar to what the official Postgres docker image does.
	chown postgres ${PGDATA} -R
	chmod 700 ${PGDATA} -R
	
	# Update the allowed authentcation list for the Postgres replication user.
	echo "host replication $POSTGRES_REPLICATION_USER 0.0.0.0/0 md5" >> "$PGDATA/pg_hba.conf"
	
	# Start the Postgres using the replication.
	su postgres -c 'pg_ctl -D $PGDATA start'
	
	# Update the configuration so Postgres can process WAL files.
	psql -v ON_ERROR_STOP=1 -U "$POSTGRES_USER" <<-EOSQL 
		ALTER SYSTEM SET primary_conninfo TO 'host=postgres_primary port=5432 user=$POSTGRES_REPLICATION_USER password=$POSTGRES_REPLICATION_PASSWORD';
		ALTER SYSTEM SET restore_command TO 'cp /var/lib/postgresql/data/pg_wal/%f "%p"';
		SELECT pg_reload_conf();
	EOSQL
	
	# Stop the cluster as Docker will be starting the instance.
	su postgres -c 'pg_ctl -D $PGDATA stop'
fi

# Let Docker run the "command" that logs WAL information so we can observe the log output in our console.
exec "$@"