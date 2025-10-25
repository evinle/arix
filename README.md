# arix

Competitive PvP math-based online game 

# PROJECT SETUP
## Front End
- Install NPM and Node.js
- `cd arix-front`
- `npm install`
- `npm run dev`

## Back End
- Install .NET >= 9.0
- `cd ArixBack`
- `dotnet run`

## Database
- Install Docker
- Make sure you're in the projects root directory
- `docker-compose up -d`

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