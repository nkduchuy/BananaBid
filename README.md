
# Banana Bid

A web-based vehicle auction application that allows users to create auctions and participate in live bidding events. Built with Next.JS front-end and a system of .NET 8 microservices as back-end.

## Features

- CRUD operations for vehicle auctions
- Browse and search for available auctions
- Place bids on your favorite vehicles
- Track the status of your bids
- Receive real-time notifications for auction updates
- View auction history and winners

## Main Technologies Used

- **Backend**: Microservice architecture with the use of .NET 8, gRPC, RabbitMQ
- **Frontend**: Next.JS, Zustand, SignalR
- **Database**: Entity Framework, PostgreSQL, MongoDB
- **Authentication**: Duende IdentityServer, JWT
- **Gateway**: Setup using Microsoft YARP (Yet Another Reverse Proxy)
- **Others**: Docker for containerization, xUnit for unit and integration tests

## Running the application locally

1. Clone the project

```bash
  git clone https://github.com/nkduchuy/BananaBid
```

2. Go to the BananaBid directory

```bash
  cd BananaBid
```

3. Ensure you have Docker Desktop installed on your machine. If not download and install from Docker and review the instructions for your OS [here](https://docs.docker.com/desktop/).

4. Build the services locally:

```bash
  docker compose build
```

5. Once completed, start up the services:

```bash
  docker compose up -d
```

6. To see the app working you will need to provide it with an SSL certificate. If not done yet, please install 'mkcert' onto your machine ([instructions](https://github.com/FiloSottile/mkcert).) Once you have this you will need to install the local Certificate Authority:

```bash
  mkcert -install
```

7. You will then need to create the certificate and key file on your computer to replace the existing certificates. You will need to change into the 'devcerts' directory and then run the following command:

```bash
  cd devcerts
  mkcert -key-file bananabid.com.key -cert-file bananabid.com.crt app.bananabid.com api.bananabid.com id.bananabid.com

```

8. You will also need to create an entry in your host file so you can reach the app by its domain name ([instructions](https://phoenixnap.com/kb/how-to-edit-hosts-file-in-windows-mac-or-linux).) Create the following entry:

```bash
  127.0.0.1 id.bananabid.com app.bananabid.com api.bananabid.com
```

9. You should now be able to browse to the app on https://app.bananabid.com