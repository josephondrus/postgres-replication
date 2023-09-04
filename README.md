Note this project skips:
- production environment configuration
- configuring storage, memory, and CPU of containers

Warning about developer storage - running out of storage may be an issue. A replica of the cluster will be created and WAL are being archived which means that storage for this project will quickly grow into gigabytes when data is being generated.

This demonstrates how to set up streaming replication between two Postgres instances.

In this example, the primary is the source of the replication and the standby will be the replication.
For verbiage, standby may be used interchangeably with replica.

This shows how to create a replicated cluster by performing a point-in-time recovery (PITR) backup of the cluster.
It also shows how to perform streaming replication to maintain a replica of another cluster.

Since streaming replication works off of the WAL, the WAL must be archived because without it the standby could not "catch up" to the primary.

Before we spin up the second Postgres instance, we want to spin up just the first instance: postgres_primary.
Then we want to seed postgres_primary with some data.
After seeding we will want to start postgres_standby so that we can observe the replication process.

If you make any changes to user names and/or passwords, ensure these get updated appropriately.

1.) Start the primary database via the docker:  
docker compose -f postgres_primary.yaml up

2.) Feel free to add a few tables and some data by using the included project. Run migrations on the database:  
dotnet ef database update

3.) Seed the database with some data by running DatabaseSeeding. If you want to continue to add data, uncomment the "while" loop in the code.

While running we can examine what is happening during replication in the standby's shell's output and observe the replication logs. These logs are provided via "log_min_messages=DEBUG2" in the Docker "command".

This allows us to inspect what goes on during replication.  
This way enables us to learn what we should be seeing and how to observe these things while replication is on-going versus when it has stopped due to network failure versus replication being finished.

Due to how fast replication can occur, we need a way to create a delta between the data stored in each database. We can do this by removing the network between the primary and standby. This is why there are three networks in this example.

4.) Stop replication.

4a.) Option 1: Simulate a network failure by disconnecting the standby from the shared_network while ensuring we can still perform queries against the standby by connecting it to the standby_network. This can be done using the docker:  
docker network connect standby_network postgres_standby  
docker network disconnect shared_network postgres_standby

4b.) Option 2: Tell Postgres to pause replication. We can use psql to pause replication.  
docker exec -it postgres_standby bash  
su postgres  
psql  
select pg_wal_replay_pause();  

5.) Run the DatabaseSeeding again to add more data to the primary.

6.) Observe a difference between the clusters.

6a.) Check to see that we have more data on primary using your preferred method. Since we know we added more data to the primary, a simple way to check the difference is look at the count in the table "Product". Observe the difference by checking the count or "Product" on each database:  
select count(*) from "Product";

6b.) Since the standby does not kow if it is behind or not, we need to look at the master to determine this.  Examine Postgres's replication status:
select * from pg_stat_replication;  
Or more specifically:  
  
select client_addr, state, sent_lsn, write_lsn, flush_lsn, replay_lsn from pg_stat_replication;  
  
On the standby we can view these values:  
select pg_last_wal_receive_lsn(), pg_last_wal_replay_lsn(), pg_last_xact_replay_timestamp();  

7.) Resume replication:

7a.) Option 1: Restore the connection between the primary and standby:  
docker network connect standby_network postgres_standby

7b.) Option 2: Resume replication:  
docker exec -it postgres_standby bash  
su postgres  
psql  
select pg_wal_replay_resume();  

8.) After replication finished, promote the standby to primary:  
docker exec -it postgres_standby bash  
su postgres  
psql  
select pg_promote();  


