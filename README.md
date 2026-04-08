# arix

Competitive PvP math-based online game

# PROJECT SETUP

## Front End

- Install NPM and Node.js
- `cd arix-front`
- `npm install`
- `npm run dev:front`

## Back End

- Install .NET >= 9.0
- `cd ArixBack`
- `dotnet run` or `dotnet watch run` to watch for changes

## Database

- Install Docker
- Either
  - Make sure you're in the projects root directory
  - `docker-compose up -d`
- Or
  - If you're in front/back end
  - `docker-compose -f ../docker-compose.yml up -d`
    
## Auth

- You will also need to set the .NET client secrets for Google
`dotnet user-secrets set "Authentication:Google:Client_id" "<ask-maintainer-for-id>"`
`dotnet user-secrets set "Authentication:Google:Client_secret" "<ask-maintainer-for-secret>"`

### Is it running properly?

- To double check that everything is working as expected
- `docker ps`
- If you see a mongo container instance up and running, you're good to go

### Restarting container

- `docker-compose down`

### Recommended Install

- MongoDB database server
- MongoDB Compass (GUI)
- mongosh (TUI)

# RUNNING WHOLE APP LOCALLY

- From the front end folder `npm run dev`
