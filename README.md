#Install guideline
- Clone the project and unzip.
- Go to Web.config Change the Data Source = (your database server)
- Open the Package Manager Console from Tools → Library Package Manager → Package Manager Console and then run the command: enable-migrations –EnableAutomaticMigration:$true
- And update the database with command: update-database.

