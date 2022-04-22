# Overview
This is a .NET SDK to perform geolocation lookups by IP address, using ip-api.com.

# Initialization
Initialize the IpLocationService.Service class. If not passing in an apiKey parameter then you'll be using the free service that ip-api.com provides. If passing in an apiKey then you'll use the pro service that ip-api.com provides which has unlimited request limits.

# Methods
Get, GetAsync, GetBatch, GetBatchAsync.

All methods optionally allow you to pass in an array of Field query options which are documented on the ip-api.com site. The Field options allow you to dictate which properties get returned in the response. By default all Field options are returned in the response.

# Versions
1.0.1 - Initial version built on .NET Framework 4.8.
