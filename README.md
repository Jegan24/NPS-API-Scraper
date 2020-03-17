# NPS-API-Scraper
C# Console Application that scrapes the National Parks Service Data API for park information and uploads it into a local SQL Server DB. 

You must execute the SQL Database Create Scripts and add an App.Config file before running the program.


The App.Config file requires an [NPS API Key](https://www.nps.gov/subjects/developer/get-started.htm) and SQL server database connection string

The content of the file should be as follows:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <connectionStrings>
    <add name="NPGeek" connectionString="[YOUR CONNECTION STRING HERE]"/>
    <add name="npsApiKey" connectionString="[YOUR API KEY HERE]"/>
  </connectionStrings>
</configuration>
```
