# HonesRemit API
This a simple RESTful API built for my blog series on MassTransit.

HonesRemit is a fictitious money transfer service that allows users to send money to their loved ones in other countries. 
This API was built to simulate the backend of the HonesRemit service.

## Running the project
To run the project, you need to have the following installed on your machine:
- .NET 8 or later
- Docker

To run the project, follow these steps:
1. Run the infrastructure services required by the API using Docker Compose:
```bash
docker-compose up -d
```

2. Run the API from the IDE of your choice.

3. You can now access the API on `http://localhost:5238`.