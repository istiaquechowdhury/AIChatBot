# AIChatBot
## 🛠️ Project Setup Instructions
#step by step approach

1.Clone the repository from here
https://github.com/istiaquechowdhury/AIChatBot

2.setup the connection string with database name.

3. API Accounts Needed
- [Tavily AI API Key](https://app.tavily.com/home)) (Free tier available)

3.create an appsettings.development.json and put the api key there
{
  "Tavily": {
    "ApiKey": "Your-Api-key"
  }
}

4.run this command to update database
"dotnet ef database update --project AIChatBot.Web --context ApplicationDbContext"
preferable to run this command in Visual studio Package manager Console.


## 🛠️ Project Features


5.Two Roles "Admin" and "User"
Admin email = "admin@chatbot.com"
Admin Password = "Admin@123"
and whoever register will be an "User"

6.Admin can Delete,Edit,Approve where user can edit and delete.

7.infinite Scrolling,Crud Operations.

8.Admin can see all the users messages.But users will see only their messages

