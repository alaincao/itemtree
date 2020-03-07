
Create database:
================
- Set `sa` password in the `docker-compose.yml` file  
  *i.e.* Uncomment the `SA_PASSWORD` variable's line  
- To connect using e.g. ManagementStudio, (temporarily) uncomment the `ports:` section & choose an available port in the `docker-compose.yml`
- Create MSSQL's directory & ensure it is read/writeable to the MSSQL's process (ID 10001):  
  ```bash
  $ mkdir -p ./volumes/dbserver/
  $ sudo chown 10001 ./volumes/dbserver/
  ```
- Start the database container  
  ```bash
  $ docker-compose up -d dbserver
  ```
- Connect to the server as `sa` & create the user & the database  
  *nb:* use the `<PASSWORD>` that is specified in the `appsettings.json` file
  ```sql
  CREATE LOGIN ttt WITH PASSWORD = '<PASSWORD>'
  CREATE DATABASE itemttt
  USE itemttt
  CREATE USER ttt
  EXEC sp_addrolemember 'db_owner', 'ttt'
  EXEC sp_change_users_login 'Update_One', 'ttt', 'ttt'
  ```
- Re-comment the `ports:` and `SA_PASSWORD` sections of the `docker-compose.yml` file and recreate the container  
  ```bash
  $ docker-compose up -d dbserver
  ```
- Run a `bash` prompt from within a container to run *Dotnet Entity Framework*'s migration
  ```bash
  $ docker-compose run --rm webserver /bin/bash
  # Now, we're in a container:
    $ dotnet ef database update
    $ exit
  ```
- Set admin password
  ```sql
  -- Set password here:
  delete from dbo.Configuration where "Key" = 'AdminPasswordHash'
  insert into dbo.Configuration("Key", "Value") values ('AdminPasswordHash', '123')

  -- Append seed as specified in file 'app/src/Services/LoginController.cs':
  update dbo.Configuration set "Value" = "Value"+'ItemTTT'
  where "Key" = 'AdminPasswordHash'

  update dbo.Configuration set "Value" = convert( varchar(32), HashBytes('MD5', "Value"), 2 )
  where "Key" = 'AdminPasswordHash'

  update dbo.Configuration set "Value" = lower("Value")
  where "Key" = 'AdminPasswordHash'
  ```

Cheatsheet:
===========
```bash
$ cd app/
$ dotnet ef migrations add <migration>
$ dotnet ef database update
```
