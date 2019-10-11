Naming convention for the SQL scripts in this folder is:
<Category ID>_<Sequential ID>_<Name of the script; spaces separated by underscore>.sql

such as:
1_001_Initialize_Database.sql

SQL scripts has to be ordered in a way that they are supposed to run one after another. 

See the reserved Category IDs below. In general, odd IDs are for creating, altering, etc., even are for dropping.
0 - DROP DATABASE statements
1 - CREATE DATABASE statements
5 - CREATE TABLE statements
7 - INSERT statements