# HonesRemit API
This a simple RESTful API built for my blog series on MassTransit.

HonesRemit is a fictitious money transfer service that allows users to send money to their loved ones in other countries. 
This API was built to simulate the backend of the HonesRemit service.

> [!NOTE] Note
> This project is built to demonstrate how to use MassTransit in a .NET Core application. It is not meant to be used in a production environment. There are certain decisions 
> made in the project that are not best practices, or at least don't make sense for a production application.

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

## RabbitMQ Management
You can access the RabbitMQ management console on `http://localhost:15672`. The default username and password are `guest`.

## Email Client
You can access the email client on `http://localhost:9510/` to view emails sent by the API.