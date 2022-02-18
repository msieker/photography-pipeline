# photography-pipeline

## postgres setup

The database needs the postgis extension installed

```
sudo add-apt-repository ppa:ubuntugis/ppa
sudo apt install postgresql-12-postgis-3
```

And then this command ran in the database:

```
CREATE EXTENSION postgis;
```